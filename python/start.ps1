# Run as administrator

$pwd = $PSCommandPath | Split-Path -Parent
powershell $pwd\venv\Scripts\Activate.ps1
python $pwd\app.py