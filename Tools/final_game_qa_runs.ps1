param([switch]$FollowUp,[switch]$Retention)
$ErrorActionPreference = 'Stop'
$qaProject = 'D:\GAMEKKK\LightBetweenCultures'
$qaOutput = Join-Path $qaProject 'Validation\FinalGameplayAndroid-20261020'
$qaExe = Join-Path $qaProject 'Builds\Windows-FinalGameplayQA-20261020\LightBetweenCultures.exe'
$qaSeed = Join-Path $qaProject 'Validation\OwnerRoutePhaseB-20261019\story-korea-seoul02\OwnerRoutes-story-final.json'
$qaEarnedLane = Join-Path $qaOutput 'leisure12\Casual-earned-optional-lane.json'
$qaReceipt = Join-Path $qaOutput $(if ($FollowUp) {'followup-results.json'} else {'queue-results.json'})
if ($Retention) {
    $qaExe=Join-Path $qaProject 'Builds\Windows-FinalGameplayRetention-20261020\LightBetweenCultures.exe'
    $qaReceipt=Join-Path $qaOutput 'retention-results.json'
}
$qaResults = @()

function Invoke-GameQa($qaName, $qaArguments, $qaInput, $qaLimit = 900) {
    $qaFolder = Join-Path $qaOutput $qaName
    if (Test-Path -LiteralPath $qaFolder) { throw "Refuse existing QA output: $qaFolder" }
    New-Item -ItemType Directory -Path $qaFolder | Out-Null
    if ($qaInput) { Copy-Item -LiteralPath $qaInput -Destination (Join-Path $qaFolder 'gameplay-save.json') }
    $qaStarted = Get-Date
    $qaProcess = Start-Process -FilePath $qaExe -ArgumentList (@('--gameplay-smoke',"--gameplay-output=$qaFolder",'-logFile',"$qaFolder\player.log") + $qaArguments) -WindowStyle Hidden -PassThru
    $qaPeak = 0L
    $qaTimedOut = $false
    Write-Output "START $qaName PID=$($qaProcess.Id)"
    while (-not $qaProcess.HasExited) {
        $qaProcess.Refresh()
        if (-not $qaProcess.HasExited) { $qaPeak = [Math]::Max($qaPeak, $qaProcess.WorkingSet64) }
        if (((Get-Date) - $qaStarted).TotalSeconds -gt $qaLimit) {
            # Only the private QA process created above, never another Player/Editor.
            $qaProcess.Kill()
            $qaTimedOut = $true
            break
        }
        Start-Sleep -Seconds 2
    }
    $qaResultFile = Join-Path $qaFolder 'smoke-results.txt'
    $qaStatus = 'BLOCKED_NO_RESULT'
    if ($qaTimedOut) { $qaStatus = 'BLOCKED_TIMEOUT' }
    elseif (Test-Path -LiteralPath $qaResultFile) {
        $qaText = Get-Content -LiteralPath $qaResultFile -Raw
        if ($qaText.TrimEnd().EndsWith('ALL CHECKS PASSED')) { $qaStatus = 'PASS' } else { $qaStatus = 'FAIL' }
    }
    $qaRecord = [pscustomobject]@{run=$qaName;status=$qaStatus;pid=$qaProcess.Id;started=$qaStarted.ToString('o');finished=(Get-Date).ToString('o');peakWindowsWorkingSetBytes=$qaPeak;seed=$qaInput;arguments=$qaArguments;resultFile=$qaResultFile;scope='Automated Windows UI with isolated save; not a blind human test'}
    $script:qaResults += $qaRecord
    $script:qaResults | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $qaReceipt -Encoding UTF8
    Write-Output "DONE $qaName $qaStatus"
}

if ($Retention) {
    if (Get-Process -Name LightBetweenCultures -ErrorAction SilentlyContinue) { throw 'Another Player is active' }
    $qaEnding=Join-Path $qaOutput 'fresh-vietnam\P5-ending-completed.json'
    Invoke-GameQa 'retention-journal' @('--contest-resume','--retention-journal') $qaEnding
    Invoke-GameQa 'retention-menu-restart' @('--contest-resume') $qaEnding
    Invoke-GameQa 'retention-fresh-core' @('--casual-runtime','--casual-core') $null
    exit
}
if ($FollowUp) {
    if (Get-Process -Name LightBetweenCultures -ErrorAction SilentlyContinue) { throw 'Another Player is active; do not overlap input tests' }
    Invoke-GameQa 'owner-fresh' @('--owner-confusion') $null
    Invoke-GameQa 'owner-four-restart' @('--owner-confusion','--owner-resume') (Join-Path $qaOutput 'owner-fresh\Owner-four-fragments-earned.json')
    Invoke-GameQa 'owner-seal' @('--owner-confusion','--owner-seal') (Join-Path $qaOutput 'fresh-vietnam\P5-ending-completed.json')
    Invoke-GameQa 'npc-actors' @('--map-npc-markers') $qaEarnedLane
    Invoke-GameQa 'photo-album18' @('--checkin-art-restart','--checkin-all-city-album') (Join-Path $qaProject 'Validation\CheckInArt-20261018\Journey02\gameplay-save.json')
    Invoke-GameQa 'boss-zero-relics' @('--phase5-no-relics') (Join-Path $qaProject 'Validation\Phase5-20261018\no-relics02\P5-boss-phase1.json')
    exit
}

# Let the already running leisure-help review finish before starting another Player.
$qaExisting = Get-Process -Id 28936 -ErrorAction SilentlyContinue
if ($qaExisting) {
    if (-not $qaExisting.WaitForExit(420000)) { throw 'Existing private leisure QA has not finished; do not start overlapping Players' }
}
if (-not (Test-Path -LiteralPath $qaEarnedLane)) { throw 'Earned optional-lane save absent; never inject progression flags' }
Invoke-GameQa 'leisure12-native' @('--leisure-full') $qaEarnedLane
Invoke-GameQa 'routes-boats-junctions' @('--owner-route-b','--owner-route-b-720',"--owner-route-cases=$qaProject\Docs\Maps\OwnerRoutePhaseB-20261019\ferry-test-cases.json") $qaSeed
Invoke-GameQa 'fresh-vietnam' @('--owner-route-b','--owner-route-b-story','--phase5-e2e','--phase4-e2e=VN_FIRST') $null 1200
Invoke-GameQa 'fresh-korea' @('--owner-route-b','--owner-route-b-story','--phase5-e2e','--phase4-e2e=KOREA_FIRST') $null 1200
Invoke-GameQa 'ending-restart' @('--phase5-restart=completed') $qaSeed
