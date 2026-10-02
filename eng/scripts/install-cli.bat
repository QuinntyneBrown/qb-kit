@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0install-cli.ps1"
exit /b %ERRORLEVEL%
