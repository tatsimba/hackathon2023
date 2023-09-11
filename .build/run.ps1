$currentDir = $PSCommandPath | Split-Path -Parent

Start-Process -FilePath "$currentDir\dotnet-run.cmd"

Start-Process -FilePath "$currentDir\python-run.cmd"

Start-Process -FilePath "$currentDir\ui-run.cmd"
