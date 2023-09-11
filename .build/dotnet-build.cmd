@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

set sln_name=%1
IF "%~1" EQU "" set sln_name=ColorCodeHackathon2023
set BuildType=Release
IF "%~2" EQU "Debug" set BuildType=Debug
set BuildArchitecture=amd64

pushd %~dp0
call dotnet build %~dp0..\dotnet\%sln_name%\%sln_name%.sln --configuration Release --no-restore 