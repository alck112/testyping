#pragma once
#include <windows.h>
#include <string>
#include <functional>
#include <thread>
#include <atomic>

class PipeClient {
public:
    using OnLine = std::function<void(const std::string&)>;

    PipeClient();
    ~PipeClient();

    void Start(const std::wstring& pipeName, OnLine onLine);
    void Stop();
    void SendLine(const std::string& line);

private:
    void ThreadMain();

    std::wstring _pipeName;
    OnLine _onLine;
    std::thread _th;
    std::atomic<bool> _running{false};
    HANDLE _hPipe{INVALID_HANDLE_VALUE};
    CRITICAL_SECTION _sendCs;
};
