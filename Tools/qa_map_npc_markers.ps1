param([switch]$FinalSeoulOnly,[switch]$FinalHoiAnOnly)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$qa=Join-Path $root 'Validation\MapNpcMarkers-20261018'
$jobs=@(
 @{name='six-cities01';mode='--map-npc-markers';source='Validation\Phase4-20261018\retro-korea01\P4-RETRO-KOREA_FIRST-final.json'},
 @{name='fresh01';mode='--map-npc-markers-fresh';source='Validation\Phase4-20261018\e2e-vn01\state-B-H7.json'},
 @{name='restart01';mode='--map-npc-markers-restart';source='Validation\MapNpcMarkers-20261018\fresh01\npc-fresh-final.json'},
 @{name='hidden01';mode='--map-npc-markers-hidden';source='Validation\Phase4-20261018\e2e-vn01\state-A.json'}
)
if($FinalSeoulOnly){$jobs=@(@{name='seoul03';mode='--map-npc-markers';extra='--map-npc-markers-city=seoul';source='Validation\MapNpcMarkers-20261018\six-cities02\npc-audit-final.json'})}
if($FinalHoiAnOnly){$jobs=@(
 @{name='fresh02';mode='--map-npc-markers-fresh';source='Validation\Phase4-20261018\e2e-vn01\state-B-H7.json'},
 @{name='restart02';mode='--map-npc-markers-restart';source='Validation\MapNpcMarkers-20261018\fresh02\npc-fresh-final.json'}
)}
foreach($job in $jobs) {
 $folder=Join-Path $qa $job.name
 if(Test-Path -LiteralPath $folder){throw "Refuse to overwrite QA: $folder"}
 New-Item -ItemType Directory -Path $folder | Out-Null
 $seed=Join-Path $root $job.source
 Copy-Item -LiteralPath $seed -Destination (Join-Path $folder 'gameplay-save.json')
 @{source=$job.source;sha256=(Get-FileHash -LiteralPath $seed -Algorithm SHA256).Hash;fixture=$false}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $folder 'input-provenance.json') -Encoding UTF8
 $playerArgs=@('--gameplay-smoke',$job.mode,"--gameplay-output=$folder",'-logFile',"$folder\player.log")
 if($job.extra){$playerArgs+=$job.extra}
 $p=Start-Process -FilePath (Join-Path $root 'Builds\Windows\LightBetweenCultures.exe') -ArgumentList $playerArgs -WindowStyle Hidden -PassThru
 Write-Output "Started $($job.name) PID $($p.Id)"
 $deadline=[DateTime]::UtcNow.AddMinutes(10)
 while(-not $p.WaitForExit(45000)){Write-Output "Running $($job.name)";if([DateTime]::UtcNow -gt $deadline){throw 'Bounded QA timeout; inspect current Player before proceeding'}}
 $result=Join-Path $folder 'smoke-results.txt';$last=Get-Content -LiteralPath $result -Tail 1
 Write-Output "$($job.name): $last"
 if($last -ne 'ALL CHECKS PASSED'){Get-Content -LiteralPath $result -Tail 12;throw 'Stop on failure'}
}
