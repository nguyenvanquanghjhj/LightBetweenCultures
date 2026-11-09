param([string]$Suite='all')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$jobs=@()
if($Suite-eq'final'){
 $jobs+=@{job='retro-claim';run='legacy-claim01'}
 $jobs+=@{job='tail';run='tail-vn01';seed='Validation\Phase5-20261018\e2e-vn01\P4-E2E-VN_FIRST-final.json'}
 $jobs+=@{job='tail';run='tail-korea01';seed='Validation\Phase5-20261018\e2e-korea01\P4-E2E-KOREA_FIRST-final.json'}
 $jobs+=@{job='restart';run='restart-ready02';state='ready';seed='Validation\Phase5-20261018\matrix04\P5-ready-unconfirmed.json'}
 $jobs+=@{job='restart';run='restart-interrupted02';state='interrupted';seed='Validation\Phase5-20261018\matrix04\P5-ceremony-interrupted.json'}
 $jobs+=@{job='restart';run='restart-completed02';state='completed';seed='Validation\Phase5-20261018\tail-vn01\P5-ending-completed.json'}
}
if($Suite-eq'all'){
 $jobs+=@{job='matrix';run='matrix03'}
 $jobs+=@{job='no-relics';run='no-relics02'}
 $jobs+=@{job='restart';run='restart-ready01';state='ready';seed='Validation\Phase5-20261018\matrix03\P5-ready-unconfirmed.json'}
 $jobs+=@{job='restart';run='restart-interrupted01';state='interrupted';seed='Validation\Phase5-20261018\matrix03\P5-ceremony-interrupted.json'}
 $jobs+=@{job='restart';run='restart-completed01';state='completed';seed='Validation\Phase5-20261018\matrix03\P5-ending-completed.json'}
 $jobs+=@{job='restart';run='restart-legacy-zero01';state='legacy-zero';seed='Validation\FullStory-20261017\Fresh04\full-story-final.json'}
 $jobs+=@{job='restart';run='restart-legacy-six01';state='legacy-six';seed='Validation\Phase4-20261018\retro-korea01\P4-RETRO-KOREA_FIRST-final.json'}
 $jobs+=@{job='restart';run='restart-pre-seoul01';state='pre-seoul';seed='Validation\FullStory-20261017\Fresh04\full-gyeongju-gate1.json'}
 $jobs+=@{job='restart';run='restart-relay01';state='relay';seed='Validation\FullStory-20261017\Fresh04\full-seoul-before-relay-B.json'}
 $jobs+=@{job='restart';run='restart-mid-boss01';state='mid-boss';seed='Validation\Phase5-20261018\no-relics02\P5-mid-boss.json'}
 $jobs+=@{job='restart';run='restart-battle01';state='battle';seed='Validation\Phase4-20261018\e2e-korea03\P4-E2E-KOREA_FIRST-after-boss.json'}
 $jobs+=@{job='restart';run='restart-repair01';state='missing';seed='Validation\Phase5-20261018\no-relics02\P5-post-boss-zero-relics.json'}
}
if($Suite-eq'all'-or$Suite-eq'e2e'){
 $jobs+=@{job='vn';run='e2e-vn01'};$jobs+=@{job='korea';run='e2e-korea01'}
}
foreach($job in $jobs){
 & "$PSScriptRoot\phase5_validation.ps1" -Job $job.job -Run $job.run -Seed $job.seed -State $job.state
 $folder=Join-Path $root "Validation\Phase5-20261018\$($job.run)"
 $idPlayer=(Get-Content -LiteralPath (Join-Path $folder 'process.json') -Raw|ConvertFrom-Json).pid
 $deadline=[DateTime]::UtcNow.AddMinutes(18)
 $player=Get-Process -Id $idPlayer -ErrorAction SilentlyContinue
 while($null-ne$player-and-not $player.WaitForExit(45000)){
  Write-Output "Running $($job.run) $([DateTime]::UtcNow.ToString('o'))"
  if([DateTime]::UtcNow-gt$deadline){throw 'Bounded QA timeout; inspect Player before proceeding'}
 }
 $result=Join-Path $folder 'smoke-results.txt'
 if(-not(Test-Path -LiteralPath $result)){throw "No results $($job.run)"}
 Get-Content -LiteralPath $result -Tail 4
 if((Get-Content -LiteralPath $result -Tail 1)-ne'ALL CHECKS PASSED'){throw "Stop on failure $($job.run)"}
}
