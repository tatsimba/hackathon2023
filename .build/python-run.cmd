@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

python -m venv %~dp0..\python\venv
powershell %~dp0..\python\venv\Scripts\Activate.ps1
pip install -r %~dp0..\python\requirements.txt
python %~dp0..\python\app.py