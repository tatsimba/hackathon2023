@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

python -m venv %~dp0..\python\venv
powershell %~dp0..\python\venv\Scripts\Activate.ps1
python %~dp0..\python\app.py