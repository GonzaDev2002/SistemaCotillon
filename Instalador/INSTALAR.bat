@echo off
title Cotillon - Instalacion

REM Doble clic y listo. Este archivo existe para que el cliente no tenga que
REM saber nada de PowerShell ni de permisos: pide administrador solo, y saltea
REM la politica de ejecucion, que de fabrica bloquea los scripts descargados.

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "Start-Process powershell -Verb RunAs -ArgumentList '-NoProfile','-ExecutionPolicy','Bypass','-File','\"%~dp0Instalar.ps1\"'"
