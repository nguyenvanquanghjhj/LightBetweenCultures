param([string]$Job='story',[string]$Run='vn01',[string]$Order='VN_FIRST',[string]$Seed='',[string]$Only='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
if(Get-Process LightBetweenCultures -ErrorAction SilentlyContinue){throw 'A Player is active; no concurrent run'}
$folder=Join-Path $root "Validation\Phase7-20261019\$Run"
if(Test-Path -LiteralPath $folder){throw "Never overwrite QA: $folder"}
New-Item -ItemType Directory -Path $folder | Out-Null
$launch=@('--gameplay-smoke','--audio-preferences=phase7-20261019',"--gameplay-output=$folder",'-logFile',"$folder\player.log")
switch($Job){
 'story' {$launch+=@('--final-polish-story','--phase5-e2e',"--phase4-e2e=$Order")}
 'maps' {if(-not $Seed){$Seed='Validation\Phase5-20261018\tail-vn01\P5-ending-completed.json'};$launch+=@('--final-polish-maps','--final-polish-map-label=phase7')}
 'leisure' {if(-not $Seed){$Seed='Validation\FullStory-20261017\Fresh04\full-story-final.json'};$launch+='--leisure-full'}
 'restart' {if(-not $Seed){throw 'Explicit earned save required'};$launch+='--final-polish-restart'}
 'visual' {if(-not $Seed){throw 'Explicit earned22-proof campaign save required'};$launch+='--phase7-visual'}
 'bots-before' {if(-not $Seed){throw 'Explicit earned save required'};$launch+=@('--phase7-bots','--phase7-bot-label=before')}
 'bots-after' {if(-not $Seed){throw 'Explicit earned save required'};$launch+=@('--phase7-bots','--phase7-bot-label=after')}
 'bot-portraits' {if(-not $Seed){throw 'Explicit earned save required'};$launch+='--phase7-bot-portraits'}
 'ending-matrix' {$Seed='Validation\Phase4-20261018\retro-korea01\P4-RETRO-KOREA_FIRST-final.json';$launch+='--phase5-matrix'}
 'ending-denial' {$Seed='Validation\FullStory-20261017\Fresh04\full-seoul-before-boss.json';$launch+=@('--phase5-no-relics','--final-polish-limit-captures')}
 default {throw 'Unknown job'}
}
if($Seed){$source=Join-Path $root $Seed;Copy-Item -LiteralPath $source -Destination (Join-Path $folder 'gameplay-save.json');@{source=$Seed;sha256=(Get-FileHash -LiteralPath $source).Hash;privateQA=$true;fixture=($Job-eq'ending-matrix')}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $folder 'input-provenance.json') -Encoding UTF8}
if($Only){$launch+="--leisure-full-only=$Only"}
$assembly=Join-Path $root 'Builds\Windows\LightBetweenCultures_Data\Managed\Assembly-CSharp.dll'
@{job=$Job;order=$Order;startedUTC=[DateTime]::UtcNow.ToString('o');assemblySHA256=(Get-FileHash -LiteralPath $assembly).Hash;fresh=(!$Seed);flags=$launch}|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $folder 'build-provenance.json') -Encoding UTF8
$p=Start-Process -FilePath (Join-Path $root 'Builds\Windows\LightBetweenCultures.exe') -ArgumentList $launch -WorkingDirectory $root -WindowStyle Hidden -PassThru
@{pid=$p.Id;job=$Job;run=$Run;deadlineUTC=[DateTime]::UtcNow.AddMinutes(12).ToString('o');staleLogSeconds=90}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $folder 'process.json') -Encoding UTF8
Write-Output "Started $Run PID $($p.Id); inspect progress, stop only this PID if no progress for 90 seconds; 12 minute bounded run"
