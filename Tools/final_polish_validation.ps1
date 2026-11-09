param([string]$Job='leisure',[string]$Run='leisure01',[string]$Order='VN_FIRST',[string]$Seed='',[string]$Label='before',[string]$Only='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
if(Get-Process LightBetweenCultures -ErrorAction SilentlyContinue){throw 'A Windows Player is active; no concurrent run'}
$folder=Join-Path $root "Validation\FinalPolish-20261019\$Run"
if(Test-Path -LiteralPath $folder){throw "Never overwrite previous QA: $folder"}
New-Item -ItemType Directory -Path $folder | Out-Null
$launch=@('--gameplay-smoke','--audio-preferences=final-polish-20261019',"--gameplay-output=$folder",'-logFile',"$folder\player.log")
switch($Job){
 'leisure-before' {$Seed='Validation\FullStory-20261017\Fresh04\full-story-final.json';$launch+=@('--leisure-full','--leisure-full-only=D13,U8,J15,S1')}
 'leisure' {if(-not $Seed){$Seed='Validation\FullStory-20261017\Fresh04\full-story-final.json'};$launch+='--leisure-full'}
 'leisure-restart' {if(-not $Seed){throw 'The new two-activity QA save is required'};$launch+='--leisure-full-restart'}
 'maps' {if(-not $Seed){$Seed='Validation\Phase5-20261018\tail-vn01\P5-ending-completed.json'};$launch+=@('--final-polish-maps',"--final-polish-map-label=$Label")}
 'story' {$launch+=@('--final-polish-story','--phase5-e2e',"--phase4-e2e=$Order")}
 'restart' {if(-not $Seed){throw 'A new earned final QA save is required'};$launch+='--final-polish-restart'}
 'ending-denial' {$Seed='Validation\FullStory-20261017\Fresh04\full-seoul-before-boss.json';$launch+=@('--phase5-no-relics','--final-polish-limit-captures')}
 default {throw 'Unknown job'}
}
if($Seed){$source=Join-Path $root $Seed;Copy-Item -LiteralPath $source -Destination (Join-Path $folder 'gameplay-save.json');@{source=$Seed;sha256=(Get-FileHash -LiteralPath $source).Hash;privateQA=$true;injectedProgression=$false}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $folder 'input-provenance.json') -Encoding UTF8}
if($Only){$launch+="--leisure-full-only=$Only"}
$assembly=Join-Path $root 'Builds\Windows\LightBetweenCultures_Data\Managed\Assembly-CSharp.dll'
@{job=$Job;order=$Order;startedUTC=[DateTime]::UtcNow.ToString('o');assemblySHA256=(Get-FileHash -LiteralPath $assembly).Hash}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $folder 'build-provenance.json') -Encoding UTF8
$p=Start-Process -FilePath (Join-Path $root 'Builds\Windows\LightBetweenCultures.exe') -ArgumentList $launch -WorkingDirectory $root -WindowStyle Hidden -PassThru
@{pid=$p.Id;job=$Job;run=$Run}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $folder 'process.json') -Encoding UTF8
Write-Output "Started $Run PID $($p.Id)"
