
#include "text_service.h"
#include "class_factory.h"
#include "edit_sessions.h"

#include <shlwapi.h>
#include <sstream>

#pragma comment(lib, "shlwapi.lib")

static LONG g_dllRefs = 0;

static std::string wstring_to_utf8(const std::wstring& ws) {
    int len = WideCharToMultiByte(CP_UTF8, 0, ws.c_str(), (int)ws.size(), nullptr, 0, nullptr, nullptr);
    std::string out(len, '\0');
    WideCharToMultiByte(CP_UTF8, 0, ws.c_str(), (int)ws.size(), out.data(), len, nullptr, nullptr);
    return out;
}

static std::string json_escape(const std::wstring& ws) {
    std::string s = wstring_to_utf8(ws);
    std::string out;
    out.reserve(s.size() + 8);
    for (char c : s) {
        switch (c) {
            case '\\': out += "\\\\"; break;
            case '"':  out += "\\\""; break;
            case '\n': out += "\\n"; break;
            case '\r': out += "\\r"; break;
            case '\t': out += "\\t"; break;
            default: out.push_back(c); break;
        }
    }
    return out;
}

ScjTextService::ScjTextService() {
    InterlockedIncrement(&g_dllRefs);
}

ScjTextService::~ScjTextService() {
    _pipe.Stop();
    if (_keyMgr) _keyMgr->Release();
    if (_tm) _tm->Release();
    InterlockedDecrement(&g_dllRefs);
}

STDMETHODIMP ScjTextService::QueryInterface(REFIID riid, void** ppvObj) {
    if (!ppvObj) return E_POINTER;
    *ppvObj = nullptr;

    if (IsEqualIID(riid, IID_IUnknown) || IsEqualIID(riid, IID_ITfTextInputProcessor)) {
        *ppvObj = static_cast<ITfTextInputProcessor*>(this);
    } else if (IsEqualIID(riid, IID_ITfKeyEventSink)) {
        *ppvObj = static_cast<ITfKeyEventSink*>(this);
    } else {
        return E_NOINTERFACE;
    }
    AddRef();
    return S_OK;
}

ULONG ScjTextService::AddRef() { return (ULONG)InterlockedIncrement(&_refCount); }
ULONG ScjTextService::Release() {
    ULONG c = (ULONG)InterlockedDecrement(&_refCount);
    if (c == 0) delete this;
    return c;
}

static std::wstring GetModuleDir() {
    wchar_t path[MAX_PATH] = {};
    GetModuleFileNameW((HMODULE)&__ImageBase, path, MAX_PATH);
    PathRemoveFileSpecW(path);
    return path;
}

STDMETHODIMP ScjTextService::Activate(ITfThreadMgr* ptim, TfClientId tid) {
    _tm = ptim;
    if (_tm) _tm->AddRef();
    _clientId = tid;

    // load cin table: Engine/data/scj6.cin relative to dll (Engine/ScjTsfEngine/build/Release)
    std::wstring dir = GetModuleDir();
    std::wstring cin = dir + L"\\..\\..\\data\\scj6.cin";
    _table.LoadFromFile(cin);

    // key event sink
    HRESULT hr = _tm->QueryInterface(IID_PPV_ARGS(&_keyMgr));
    if (SUCCEEDED(hr) && _keyMgr) {
        _keyMgr->AdviseKeyEventSink(_clientId, static_cast<ITfKeyEventSink*>(this), TRUE);
    }

    _pipe.Start(L"\\.\pipe\scj.ui.v1", [this](const std::string& line) { HandleUiLine(line); });
    SendStateToUi();
    return S_OK;
}

STDMETHODIMP ScjTextService::Deactivate() {
    _pipe.Stop();
    if (_keyMgr) {
        _keyMgr->UnadviseKeyEventSink(_clientId);
        _keyMgr->Release();
        _keyMgr = nullptr;
    }
    if (_tm) {
        _tm->Release();
        _tm = nullptr;
    }
    _clientId = TF_CLIENTID_NULL;
    return S_OK;
}

STDMETHODIMP ScjTextService::OnSetFocus(BOOL) { return S_OK; }

STDMETHODIMP ScjTextService::OnTestKeyDown(ITfContext*, WPARAM wParam, LPARAM, BOOL* pfEaten) {
    if (!pfEaten) return E_POINTER;
    *pfEaten = FALSE;

    if ((wParam >= 'A' && wParam <= 'Z') || (wParam >= 'a' && wParam <= 'z')) *pfEaten = TRUE;
    else if (wParam == VK_BACK || wParam == VK_SPACE || wParam == VK_RETURN || wParam == VK_ESCAPE) *pfEaten = TRUE;
    else if (wParam >= '1' && wParam <= '9') *pfEaten = !_buffer.empty();
    return S_OK;
}

STDMETHODIMP ScjTextService::OnTestKeyUp(ITfContext*, WPARAM, LPARAM, BOOL* pfEaten) {
    if (pfEaten) *pfEaten = FALSE;
    return S_OK;
}

void ScjTextService::UpdateCandidates() {
    _cands = _table.Lookup(_buffer, 10);
    if (_selected >= (int)_cands.size()) _selected = (int)_cands.size() - 1;
    if (_selected < 0) _selected = 0;
}

void ScjTextService::SendStateToUi() {
    UpdateCandidates();

    std::ostringstream oss;
    oss << "{\"type\":\"state\",\"comp\":\"" << json_escape(_buffer) << "\",\"selected\":" << _selected << ",\"candidates\":[";
    for (size_t i = 0; i < _cands.size(); i++) {
        if (i) oss << ",";
        oss << "\"" << json_escape(_cands[i]) << "\"";
    }
    oss << "]}";
    _pipe.SendLine(oss.str());
}

void ScjTextService::HandleUiLine(const std::string& line) {
    // {"type":"select","index":n}
    auto pos = line.find("\"index\"");
    if (pos == std::string::npos) return;
    auto colon = line.find(':', pos);
    if (colon == std::string::npos) return;
    int idx = atoi(line.c_str() + colon + 1);
    if (idx < 0) return;
    _selected = idx;
    SendStateToUi();
}

void ScjTextService::CommitSelected(ITfContext* ctx) {
    if (!ctx) return;
    if (_cands.empty()) return;
    if (_selected < 0 || _selected >= (int)_cands.size()) return;

    ITfEditSession* es = new (std::nothrow) InsertTextEditSession(ctx, _cands[_selected]);
    if (!es) return;

    HRESULT hr = S_OK;
    ctx->RequestEditSession(_clientId, es, TF_ES_ASYNC | TF_ES_READWRITE, &hr);
    es->Release();

    _buffer.clear();
    _cands.clear();
    _selected = 0;
    SendStateToUi();
}

STDMETHODIMP ScjTextService::OnKeyDown(ITfContext* pic, WPARAM wParam, LPARAM, BOOL* pfEaten) {
    if (!pfEaten) return E_POINTER;
    *pfEaten = FALSE;

    if ((wParam >= 'A' && wParam <= 'Z') || (wParam >= 'a' && wParam <= 'z')) {
        wchar_t ch = (wchar_t)wParam;
        if (ch >= L'A' && ch <= L'Z') ch = (wchar_t)(ch - L'A' + L'a');
        _buffer.push_back(ch);
        SendStateToUi();
        *pfEaten = TRUE;
        return S_OK;
    }

    if (wParam == VK_BACK) {
        if (!_buffer.empty()) _buffer.pop_back();
        SendStateToUi();
        *pfEaten = TRUE;
        return S_OK;
    }

    if (wParam == VK_ESCAPE) {
        _buffer.clear();
        _cands.clear();
        _selected = 0;
        SendStateToUi();
        *pfEaten = TRUE;
        return S_OK;
    }

    if ((wParam >= '1' && wParam <= '9') && !_buffer.empty()) {
        _selected = (int)(wParam - '1');
        UpdateCandidates();
        CommitSelected(pic);
        *pfEaten = TRUE;
        return S_OK;
    }

    if ((wParam == VK_SPACE || wParam == VK_RETURN) && !_buffer.empty()) {
        UpdateCandidates();
        CommitSelected(pic);
        *pfEaten = TRUE;
        return S_OK;
    }

    return S_OK;
}

STDMETHODIMP ScjTextService::OnKeyUp(ITfContext*, WPARAM, LPARAM, BOOL* pfEaten) {
    if (pfEaten) *pfEaten = FALSE;
    return S_OK;
}

STDMETHODIMP ScjTextService::OnPreservedKey(ITfContext*, REFGUID, BOOL* pfEaten) {
    if (pfEaten) *pfEaten = FALSE;
    return S_OK;
}

// ---- COM exports ----
static HRESULT CreateInstance(REFIID riid, void** ppv) {
    if (!ppv) return E_POINTER;
    *ppv = nullptr;
    auto* svc = new (std::nothrow) ScjTextService();
    if (!svc) return E_OUTOFMEMORY;
    HRESULT hr = svc->QueryInterface(riid, ppv);
    svc->Release();
    return hr;
}

extern "C" HRESULT __stdcall DllGetClassObject(REFCLSID rclsid, REFIID riid, void** ppv) {
    if (!ppv) return E_POINTER;
    *ppv = nullptr;
    if (!IsEqualCLSID(rclsid, CLSID_ScjTextService)) return CLASS_E_CLASSNOTAVAILABLE;
    auto* fac = new (std::nothrow) ClassFactory(&CreateInstance);
    if (!fac) return E_OUTOFMEMORY;
    HRESULT hr = fac->QueryInterface(riid, ppv);
    fac->Release();
    return hr;
}

extern "C" HRESULT __stdcall DllCanUnloadNow() {
    return (g_dllRefs == 0) ? S_OK : S_FALSE;
}
