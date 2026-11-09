param([string]$FreshName='fresh02',[string]$RestartName='restart02')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$jobs=@(
 @{name=$FreshName;mode='--soft-sfx';source='Validation\Phase4-20261018\e2e-vn01\state-A.json'},
 @{name=$RestartName;mode='--soft-sfx-restart';source="Validation\SoftSfx-20261018\$FreshName\soft-sfx-final.json"}
)
foreach($job in $jobs){
 $folder=Join-Path $root "Validation\SoftSfx-20261018\$($job.name)"
 if(Test-Path -LiteralPath $folder){throw "Refuse overwrite $folder"}
 New-Item -ItemType Directory -Path $folder | Out-Null
 $seed=Join-Path $root $job.source
 Copy-Item -LiteralPath $seed -Destination (Join-Path $folder 'gameplay-save.json')
 @{source=$job.source;sha256=(Get-FileHash -LiteralPath $seed).Hash;fixture=$false}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $folder 'input-provenance.json') -Encoding UTF8
 $p=Start-Process -FilePath (Join-Path $root 'Builds\Windows\LightBetweenCultures.exe') -ArgumentList @('--gameplay-smoke',$job.mode,'--audio-preferences=softsfx-20261018',"--gameplay-output=$folder",'-logFile',"$folder\player.log") -WindowStyle Hidden -PassThru
 Write-Output "Started $($job.name) PID $($p.Id)";$limit=[DateTime]::UtcNow.AddMinutes(8)
 while(-not $p.WaitForExit(45000)){Write-Output "Running $($job.name)";if([DateTime]::UtcNow -gt $limit){throw 'Bounded test timeout'}}
 $result=Join-Path $folder 'smoke-results.txt';Get-Content -LiteralPath $result -Tail 5
 if((Get-Content -LiteralPath $result -Tail 1)-ne 'ALL CHECKS PASSED'){throw 'Stop on QA failure'}
}
