@echo off
title Factory Orders Live Server
echo ===================================================
echo     STARTING FACTORY ORDERS & PRIORITY LIVE
echo ===================================================
echo.
echo Preview on PC:       http://localhost:5000
echo Open on Android:     http://[YOUR-PC-IP]:5000 (connected to same Wi-Fi)
echo.
echo Press Ctrl+C in this window to stop the server.
echo ===================================================
echo.

set "PATH=D:\dotnet;%PATH%"
cd /d "%~dp0"
dotnet run --urls "http://0.0.0.0:5000"
pause
