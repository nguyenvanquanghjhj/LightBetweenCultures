param([string]$Job='matrix',[string]$Run='matrix01',[string]$Seed='',[string]$State='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$folder=Join-Path $root "Validation\Phase5-20261018\$Run"
if(Test-Path -LiteralPath $folder){throw "Refuse overwrite $folder"}
New-Item -ItemType Directory -Path $folder | Out-Null
$launchArgs=@('--gameplay-smoke','--audio-preferences=phase5-20261018',"--gameplay-output=$folder",'-logFile',"$folder\player.log")
switch($Job){
 'matrix' {$Seed='Validation\Phase4-20261018\retro-korea01\P4-RETRO-KOREA_FIRST-final.json';$launchArgs+='--phase5-matrix'}
 'retro-claim' {$Seed='Validation\FullStory-20261017\Fresh04\full-story-final.json';$launchArgs+='--phase5-retro-claim'}
 'tail' {if(-not $Seed){throw 'Explicit earned source required'};$launchArgs+='--phase5-tail'}
 'no-relics' {$Seed='Validation\FullStory-20261017\Fresh04\full-seoul-before-boss.json';$launchArgs+='--phase5-no-relics'}
 'vn' {$launchArgs+=@('--phase5-e2e','--phase4-e2e=VN_FIRST')}
 'korea' {$launchArgs+=@('--phase5-e2e','--phase4-e2e=KOREA_FIRST')}
 'restart' {if(-not $Seed -or -not $State){throw 'Explicit seed and state required'};$launchArgs+="--phase5-restart=$State"}
 default {throw 'Unknown bounded QA job'}
}
if($Seed){
 $source=Join-Path $root $Seed
 Copy-Item -LiteralPath $source -Destination (Join-Path $folder 'gameplay-save.json')
 @{source=$Seed;sha256=(Get-FileHash -LiteralPath $source).Hash;fixture=($Job-eq'matrix'-or $Seed-like'*matrix*');notes='Matrix states are private fixtures; fresh VN/Korea runs inject no progression flags.'}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $folder 'input-provenance.json') -Encoding UTF8
}
$player=Start-Process -FilePath (Join-Path $root 'Builds\Windows\LightBetweenCultures.exe') -ArgumentList $launchArgs -WindowStyle Hidden -PassThru -WorkingDirectory $root
@{pid=$player.Id;job=$Job;run=$Run;started=[DateTime]::UtcNow.ToString('o')}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $folder 'process.json') -Encoding UTF8
Write-Output "Started $Run PID $($player.Id)"
