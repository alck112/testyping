#pragma once
#include <windows.h>
#include <msctf.h>
#include <string>
#include <vector>

#include "cin_table.h"
#include "pipe_client.h"
#include "guids.h"

class ScjTextService final : public ITfTextInputProcessor, public ITfKeyEventSink {
public:
    ScjTextService();

    STDMETHODIMP QueryInterface(REFIID riid, void** ppvObj) override;
    STDMETHODIMP_(ULONG) AddRef() override;
    STDMETHODIMP_(ULONG) Release() override;

    STDMETHODIMP Activate(ITfThreadMgr* ptim, TfClientId tid) override;
    STDMETHODIMP Deactivate() override;

    STDMETHODIMP OnSetFocus(BOOL fForeground) override;
    STDMETHODIMP OnTestKeyDown(ITfContext* pic, WPARAM wParam, LPARAM lParam, BOOL* pfEaten) override;
    STDMETHODIMP OnTestKeyUp(ITfContext* pic, WPARAM wParam, LPARAM lParam, BOOL* pfEaten) override;
    STDMETHODIMP OnKeyDown(ITfContext* pic, WPARAM wParam, LPARAM lParam, BOOL* pfEaten) override;
    STDMETHODIMP OnKeyUp(ITfContext* pic, WPARAM wParam, LPARAM lParam, BOOL* pfEaten) override;
    STDMETHODIMP OnPreservedKey(ITfContext* pic, REFGUID rguid, BOOL* pfEaten) override;

private:
    ~ScjTextService();

    void UpdateCandidates();
    void SendStateToUi();
    void HandleUiLine(const std::string& line);
    void CommitSelected(ITfContext* ctx);

    LONG _refCount{1};
    ITfThreadMgr* _tm{nullptr};
    TfClientId _clientId{TF_CLIENTID_NULL};
    ITfKeystrokeMgr* _keyMgr{nullptr};

    std::wstring _buffer;
    std::vector<std::wstring> _cands;
    int _selected{0};

    CinTable _table;
    PipeClient _pipe;
};
