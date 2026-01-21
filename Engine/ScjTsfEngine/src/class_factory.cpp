#include "class_factory.h"

ClassFactory::ClassFactory(HRESULT(*creator)(REFIID, void**)) : _creator(creator) {}
ClassFactory::~ClassFactory() = default;

STDMETHODIMP ClassFactory::QueryInterface(REFIID riid, void** ppvObj) {
    if (!ppvObj) return E_POINTER;
    *ppvObj = nullptr;
    if (IsEqualIID(riid, IID_IUnknown) || IsEqualIID(riid, IID_IClassFactory)) {
        *ppvObj = static_cast<IClassFactory*>(this);
        AddRef();
        return S_OK;
    }
    return E_NOINTERFACE;
}
ULONG ClassFactory::AddRef() { return (ULONG)InterlockedIncrement(&_refCount); }
ULONG ClassFactory::Release() { ULONG c=(ULONG)InterlockedDecrement(&_refCount); if(c==0) delete this; return c; }

STDMETHODIMP ClassFactory::CreateInstance(IUnknown* pUnkOuter, REFIID riid, void** ppvObject) {
    if (!ppvObject) return E_POINTER;
    *ppvObject = nullptr;
    if (pUnkOuter) return CLASS_E_NOAGGREGATION;
    return _creator ? _creator(riid, ppvObject) : E_UNEXPECTED;
}
STDMETHODIMP ClassFactory::LockServer(BOOL) { return S_OK; }
