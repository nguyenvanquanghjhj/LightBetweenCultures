$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
function Finish-RouteRun([string]$name){
 $folder=Join-Path $root "Validation\AppliedRouteReferences-20261019\$name"
 $taskPid=(Get-Content -LiteralPath (Join-Path $folder 'process.json') -Raw|ConvertFrom-Json).pid
 $process=Get-Process -Id $taskPid -ErrorAction SilentlyContinue
 $deadline=[DateTime]::UtcNow.AddMinutes(25)
 while($null-ne$process-and-not $process.WaitForExit(45000)){
  Write-Output "Running $name $([DateTime]::UtcNow.ToString('o'))"
  if([DateTime]::UtcNow-gt$deadline){throw 'Bounded timeout: inspect Player; do not start another run'}
 }
 $result=Join-Path $folder 'smoke-results.txt'
 if(-not(Test-Path -LiteralPath $result)){throw "Missing results $name"}
 Get-Content -LiteralPath $result -Tail 3
 if((Get-Content -LiteralPath $result -Tail 1)-ne'ALL CHECKS PASSED'){throw "Stop on failure $name"}
}
Finish-RouteRun 'after01'
if(-not(Test-Path -LiteralPath (Join-Path $root 'Validation\AppliedRouteReferences-20261019\fresh02\smoke-results.txt'))){& "$PSScriptRoot\validate_applied_route_references.ps1" -Job fresh -Run fresh02}
Finish-RouteRun 'fresh02'
foreach($city in @('hoi_an','hue','da_lat','jeonju','gyeongju','seoul')){
 $name=if($city-eq'hoi_an'){'restart-hoi_an-02'}else{"restart-$city-01"}
 & "$PSScriptRoot\validate_applied_route_references.ps1" -Job restart -Run $name -Seed "Validation\AppliedRouteReferences-20261019\restart-seeds\$city.json"
 Finish-RouteRun $name
}
& "$PSScriptRoot\validate_applied_route_references.ps1" -Job overview -Run after-overview01
Finish-RouteRun 'after-overview01'
