#pragma once
#include <string>
#include <unordered_map>
#include <vector>

class CinTable {
public:
    bool LoadFromFile(const std::wstring& path);
    std::vector<std::wstring> Lookup(const std::wstring& code, size_t maxCandidates = 10) const;
private:
    std::unordered_map<std::wstring, std::vector<std::wstring>> _map;
};
