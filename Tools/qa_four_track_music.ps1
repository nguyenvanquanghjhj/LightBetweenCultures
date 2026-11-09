param([string]$Run='fresh01',[switch]$Restart,[switch]$Battles)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$folder=Join-Path $root "Validation\FourTrackMusic-20261018\$Run"
if(Test-Path -LiteralPath $folder){throw 'Refuse overwrite QA evidence'}
New-Item -ItemType Directory -Path $folder | Out-Null
$seed=Join-Path $root 'Validation\Phase4-20261018\retro-korea01\P4-RETRO-KOREA_FIRST-final.json'
Copy-Item -LiteralPath $seed -Destination (Join-Path $folder 'gameplay-save.json')
@{source=$seed;sha256=(Get-FileHash -LiteralPath $seed).Hash;combatFixture=$true;note='Earned postgame seed; reopen encounter only inside private QA save to test music outcomes, not story progression.'}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $folder 'input-provenance.json') -Encoding UTF8
$argsMusic=@('--gameplay-smoke','--four-track-music','--audio-preferences=fourtracks-20261018',"--gameplay-output=$folder",'-logFile',"$folder\player.log")
if($Restart){$argsMusic+='--music-restart'}
if($Battles){$argsMusic+='--music-battles'}
$p=Start-Process -FilePath (Join-Path $root 'Builds\Windows\LightBetweenCultures.exe') -ArgumentList $argsMusic -WindowStyle Hidden -PassThru
Write-Output "Started $Run PID $($p.Id)"
