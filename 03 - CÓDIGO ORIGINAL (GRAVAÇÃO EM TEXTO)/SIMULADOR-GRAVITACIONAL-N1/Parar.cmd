@echo off
chcp 65001 >nul
title Orbita - Encerrar servidor local
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Parar.ps1"
if errorlevel 1 pause
