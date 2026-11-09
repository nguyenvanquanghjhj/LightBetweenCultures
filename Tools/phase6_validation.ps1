param([string]$Job='main',[string]$Run='main01',[string]$Seed='Validation\Phase5-20261018\tail-vn01\P5-ending-completed.json')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$folder=Join-Path $root "Validation\Phase6-20261019\$Run"
if(Test-Path -LiteralPath $folder){throw "Refuse overwrite $folder"}
New-Item -ItemType Directory -Path $folder | Out-Null
$source=Join-Path $root $Seed
Copy-Item -LiteralPath $source -Destination (Join-Path $folder 'gameplay-save.json')
@{source=$Seed;sha256=(Get-FileHash -LiteralPath $source).Hash;privateQA=$true;notes='Own copy only. Presentation fixtures are identified in runner/report; never user save.'}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $folder 'input-provenance.json') -Encoding UTF8
$launch=@('--gameplay-smoke',"--phase6-$Job",'--audio-preferences=phase6-20261019',"--gameplay-output=$folder",'-logFile',"$folder\player.log")
$player=Start-Process -FilePath (Join-Path $root 'Builds\Windows\LightBetweenCultures.exe') -ArgumentList $launch -WindowStyle Hidden -PassThru -WorkingDirectory $root
@{pid=$player.Id;job=$Job;run=$Run;started=[DateTime]::UtcNow.ToString('o')}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $folder 'process.json') -Encoding UTF8
Write-Output "$Run PID $($player.Id)"
