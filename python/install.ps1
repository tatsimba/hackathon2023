# Run script as administrator

$pwd = $PSCommandPath | Split-Path -Parent
python -m venv $pwd\venv
powershell $pwd\venv\Scripts\Activate.ps1
pip install -r $pwd\requirements.txt