param([string]$Job='after',[string]$Run='after01',[string]$Seed='Validation\FullStory-20261017\Fresh04\full-story-final.json')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$folder=Join-Path $root "Validation\AppliedRouteReferences-20261019\$Run"
if(Test-Path -LiteralPath $folder){throw "Refuse overwrite $folder"}
New-Item -ItemType Directory -Path $folder | Out-Null
$arguments=@('--gameplay-smoke','--audio-preferences=routes-ref-20261019',"--gameplay-output=$folder",'-logFile',"$folder\player.log")
if($Job-eq'fresh'){$arguments+='--approved-routes-fresh'}
else {
 $source=Join-Path $root $Seed
 Copy-Item -LiteralPath $source -Destination (Join-Path $folder 'gameplay-save.json')
 @{source=$Seed;sha256=(Get-FileHash -LiteralPath $source).Hash;scope='Private QA copy; edge-isolation/location fixtures are not fresh story proof'}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $folder 'input-provenance.json') -Encoding UTF8
 if($Job-eq'restart'){$arguments+='--approved-routes-restart'}elseif($Job-eq'overview'){$arguments+='--route35-before'}elseif($Job-eq'refine'){$arguments+=@('--route35-after',"--route35-cases=$root\Docs\Maps\AppliedRouteReferences-20261019\refinement-cases.json")}else{$arguments+='--approved-routes'}
}
$player=Start-Process -FilePath (Join-Path $root 'Builds\Windows\LightBetweenCultures.exe') -ArgumentList $arguments -WindowStyle Hidden -PassThru -WorkingDirectory $root
@{pid=$player.Id;job=$Job;run=$Run;startedUTC=[DateTime]::UtcNow.ToString('o');contentSHA256=(Get-FileHash -LiteralPath (Join-Path $root 'Assets\_Game\Resources\Content\GameplayContent.json')).Hash}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $folder 'process.json') -Encoding UTF8
Write-Output "Started $Run PID $($player.Id)"
