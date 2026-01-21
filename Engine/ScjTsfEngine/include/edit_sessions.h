#pragma once
#include <msctf.h>
#include <string>

class InsertTextEditSession final : public ITfEditSession {
public:
    InsertTextEditSession(ITfContext* ctx, const std::wstring& text);

    STDMETHODIMP QueryInterface(REFIID riid, void** ppvObj) override;
    STDMETHODIMP_(ULONG) AddRef() override;
    STDMETHODIMP_(ULONG) Release() override;
    STDMETHODIMP DoEditSession(TfEditCookie ec) override;

private:
    ~InsertTextEditSession();
    LONG _refCount{1};
    ITfContext* _ctx{nullptr};
    std::wstring _text;
};
