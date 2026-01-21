#pragma once
#include <windows.h>

class ClassFactory final : public IClassFactory {
public:
    explicit ClassFactory(HRESULT(*creator)(REFIID, void**));
    STDMETHODIMP QueryInterface(REFIID riid, void** ppvObj) override;
    STDMETHODIMP_(ULONG) AddRef() override;
    STDMETHODIMP_(ULONG) Release() override;
    STDMETHODIMP CreateInstance(IUnknown* pUnkOuter, REFIID riid, void** ppvObject) override;
    STDMETHODIMP LockServer(BOOL fLock) override;
private:
    ~ClassFactory();
    LONG _refCount{1};
    HRESULT(*_creator)(REFIID, void**) = nullptr;
};
