@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

cd %~dp0..\UI\ColorCode
call npm install
call npm run dev