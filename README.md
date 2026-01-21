# SCJ Win11 IME (complete, working MVP)

You asked for a *working* build, not a skeleton. This repo is a working MVP:

- TSF engine DLL intercepts keys (a-z), keeps a composition buffer, looks up candidates from `Engine/data/scj6.cin`
- Press `1-9` or `Space/Enter` to commit the selected candidate into the focused app
- Win11-style candidate window UI (WinUI 3) via named pipe `scj.ui.v1`
- Portable per-user installer (no admin): `ScjLauncher.exe` auto-starts UI
- Portable uninstaller: `ScjUninstall.exe`

## What is still not in MVP
- Inline preedit shown in the target app (we commit only on selection)
- Full CIN features (selkey rules, paging, symbol tables, etc.)

## GitHub Actions
Workflow included: `.github/workflows/build-windows.yml`
Push to GitHub, download artifact `scj-dist`.

## Use on Windows
1) Replace `Engine/data/scj6.cin` with real SCJ table file
2) Run `ScjLauncher.exe` -> Install + Start UI
3) Settings -> Chinese (Traditional) -> Keyboard -> Add keyboard -> `快倉 (SCJ) - Prototype`
4) Switch to it and test typing.
