#include "cin_table.h"
#include <fstream>

static bool is_empty_or_comment(const std::wstring& s) {
    for (wchar_t ch : s) {
        if (ch == L' ' || ch == L'\t' || ch == L'\r' || ch == L'\n') continue;
        return ch == L'#';
    }
    return true;
}

bool CinTable::LoadFromFile(const std::wstring& path) {
    _map.clear();
    std::wifstream in(path);
    in.imbue(std::locale(""));
    if (!in.is_open()) return false;

    std::wstring line;
    while (std::getline(in, line)) {
        if (line.empty() || is_empty_or_comment(line)) continue;
        if (!line.empty() && line[0] == L'%') continue;

        size_t sep = line.find(L'\t');
        if (sep == std::wstring::npos) sep = line.find(L' ');
        if (sep == std::wstring::npos) continue;

        std::wstring code = line.substr(0, sep);
        while (sep < line.size() && (line[sep] == L'\t' || line[sep] == L' ')) sep++;
        if (sep >= line.size()) continue;

        std::wstring word = line.substr(sep);
        if (code.empty() || word.empty()) continue;

        _map[code].push_back(word);
    }
    return true;
}

std::vector<std::wstring> CinTable::Lookup(const std::wstring& code, size_t maxCandidates) const {
    std::vector<std::wstring> out;
    auto it = _map.find(code);
    if (it == _map.end()) return out;
    for (size_t i=0;i<it->second.size() && out.size()<maxCandidates;i++) out.push_back(it->second[i]);
    return out;
}
