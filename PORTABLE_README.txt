SCJ Portable (One-click)

Goal: feel like "no-install" on Windows.
Reality: TSF IME must be registered, but we do it per-user (HKCU) without admin.

dist folder should contain:
  ScjLauncher.exe
  ScjUninstall.exe
  ScjTsfEngine.dll
  ScjWin11UI.exe
  scj6.cin

How to use:
1) Run ScjLauncher.exe
2) Click "Install + Start UI"  (UI will auto-start)
3) Click "Open Settings" and add keyboard: 快倉 (SCJ) - Prototype
4) Switch to that keyboard and start typing

Uninstall:
- Run ScjUninstall.exe

Note:
- Some Windows builds may require logoff/restart to refresh the TSF keyboard list.
