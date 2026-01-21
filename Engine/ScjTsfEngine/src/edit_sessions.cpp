#include "edit_sessions.h"

InsertTextEditSession::InsertTextEditSession(ITfContext* ctx, const std::wstring& text) : _ctx(ctx), _text(text) {
    if (_ctx) _ctx->AddRef();
}
InsertTextEditSession::~InsertTextEditSession() { if (_ctx) _ctx->Release(); _ctx=nullptr; }

STDMETHODIMP InsertTextEditSession::QueryInterface(REFIID riid, void** ppvObj) {
    if (!ppvObj) return E_POINTER;
    *ppvObj = nullptr;
    if (IsEqualIID(riid, IID_IUnknown) || IsEqualIID(riid, IID_ITfEditSession)) {
        *ppvObj = static_cast<ITfEditSession*>(this);
        AddRef();
        return S_OK;
    }
    return E_NOINTERFACE;
}
ULONG InsertTextEditSession::AddRef() { return (ULONG)InterlockedIncrement(&_refCount); }
ULONG InsertTextEditSession::Release() { ULONG c=(ULONG)InterlockedDecrement(&_refCount); if(c==0) delete this; return c; }

STDMETHODIMP InsertTextEditSession::DoEditSession(TfEditCookie ec) {
    if (!_ctx) return E_FAIL;
    ITfInsertAtSelection* ins = nullptr;
    HRESULT hr = _ctx->QueryInterface(IID_PPV_ARGS(&ins));
    if (FAILED(hr) || !ins) return hr;
    ITfRange* range = nullptr;
    hr = ins->InsertTextAtSelection(ec, TF_IAS_NOQUERY, _text.c_str(), (LONG)_text.size(), &range);
    if (range) range->Release();
    ins->Release();
    return hr;
}
