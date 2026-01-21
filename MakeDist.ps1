$ErrorActionPreference = "Stop"
$Here = Split-Path -Parent $MyInvocation.MyCommand.Path
$Dist = Join-Path $Here "dist"
New-Item -ItemType Directory -Force -Path $Dist | Out-Null

Write-Host "Copy these files into dist\ (same folder):"
Write-Host "  ScjLauncher.exe"
Write-Host "  ScjUninstall.exe"
Write-Host "  ScjTsfEngine.dll"
Write-Host "  ScjWin11UI.exe"
Write-Host "  scj6.cin"
Copy-Item "$Here\PORTABLE_README.txt" $Dist -Force
