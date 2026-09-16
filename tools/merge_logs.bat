@echo off
chcp 65001 >nul
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0merge_logs.ps1" -Root "%~dp0."
echo.
pause
