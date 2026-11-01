@echo off
chcp 65001 >nul
title Órbita - Simulador gravitacional
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Iniciar.ps1"
if errorlevel 1 pause
