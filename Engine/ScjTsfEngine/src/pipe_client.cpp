\
#include "pipe_client.h"
#include <vector>

PipeClient::PipeClient() { InitializeCriticalSection(&_sendCs); }
PipeClient::~PipeClient() { Stop(); DeleteCriticalSection(&_sendCs); }

void PipeClient::Start(const std::wstring& pipeName, OnLine onLine) {
    Stop();
    _pipeName = pipeName;
    _onLine = std::move(onLine);
    _running = true;
    _th = std::thread([this]{ ThreadMain(); });
}

void PipeClient::Stop() {
    _running = false;
    if (_hPipe != INVALID_HANDLE_VALUE) {
        CloseHandle(_hPipe);
        _hPipe = INVALID_HANDLE_VALUE;
    }
    if (_th.joinable()) _th.join();
}

void PipeClient::SendLine(const std::string& line) {
    EnterCriticalSection(&_sendCs);
    if (_hPipe == INVALID_HANDLE_VALUE) { LeaveCriticalSection(&_sendCs); return; }
    std::string out = line;
    out.push_back('\n');
    DWORD written = 0;
    WriteFile(_hPipe, out.data(), (DWORD)out.size(), &written, nullptr);
    LeaveCriticalSection(&_sendCs);
}

void PipeClient::ThreadMain() {
    while (_running) {
        _hPipe = CreateFileW(_pipeName.c_str(), GENERIC_READ|GENERIC_WRITE, 0, nullptr, OPEN_EXISTING, 0, nullptr);
        if (_hPipe != INVALID_HANDLE_VALUE) break;
        Sleep(300);
    }
    if (_hPipe == INVALID_HANDLE_VALUE) return;

    std::vector<char> buf(4096);
    std::string acc;
    while (_running) {
        DWORD read = 0;
        BOOL ok = ReadFile(_hPipe, buf.data(), (DWORD)buf.size(), &read, nullptr);
        if (!ok || read == 0) break;
        acc.append(buf.data(), buf.data()+read);
        size_t pos = 0;
        while (true) {
            auto nl = acc.find('\n', pos);
            if (nl == std::string::npos) break;
            auto line = acc.substr(pos, nl-pos);
            if (_onLine) _onLine(line);
            pos = nl + 1;
        }
        if (pos) acc.erase(0, pos);
    }
}
