param([string]$Run='player01',[switch]$Restart,[string]$Only='', [string]$Seed='Validation\FullStory-20261017\Fresh04\full-story-final.json')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
if(Get-Process LightBetweenCultures -ErrorAction SilentlyContinue){throw 'A Player is active; no concurrent run'}
$folder=Join-Path $root "Validation\LeisureActivitiesFullReview-20261019\$Run"
if(Test-Path -LiteralPath $folder){throw "Never overwrite an earlier QA run: $folder"}
New-Item -ItemType Directory -Path $folder | Out-Null
$source=Join-Path $root $Seed
Copy-Item -LiteralPath $source -Destination (Join-Path $folder 'gameplay-save.json')
@{source=$Seed;sha256=(Get-FileHash -LiteralPath $source).Hash;privateQA=$true;scope='Earned save; no injected quest/gate/artifact flags'} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $folder 'input-provenance.json') -Encoding UTF8
$mode=if($Restart){'--leisure-full-restart'}else{'--leisure-full'}
$launch=@('--gameplay-smoke',$mode,'--audio-preferences=leisure-full-20261019',"--gameplay-output=$folder",'-logFile',"$folder\player.log")
if($Only){$launch+="--leisure-full-only=$Only"}
$player=Start-Process -FilePath (Join-Path $root 'Builds\Windows\LightBetweenCultures.exe') -ArgumentList $launch -WindowStyle Hidden -WorkingDirectory $root -PassThru
@{pid=$player.Id;job=$mode;run=$Run;startedUTC=[DateTime]::UtcNow.ToString('o')} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $folder 'process.json') -Encoding UTF8
Write-Output "Started $Run PID $($player.Id)"
