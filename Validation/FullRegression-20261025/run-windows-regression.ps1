param([switch]$AfterAdapter,[switch]$IsolateVietnam)
$ErrorActionPreference='Stop'
$qaRoot='D:\GAMEKKK\LightBetweenCultures'
$qaOutput=Join-Path $qaRoot 'Validation\FullRegression-20261025'
if($AfterAdapter){$qaOutput=Join-Path $qaOutput 'after-adapter';New-Item -ItemType Directory -Force -Path $qaOutput|Out-Null}
if($IsolateVietnam){$qaOutput=Join-Path $qaOutput 'vietnam-isolation';New-Item -ItemType Directory -Force -Path $qaOutput|Out-Null}
$qaExe=Join-Path $qaRoot 'Builds\Windows-ThreeNpcQA-20261025\LightBetweenCultures.exe'
$qaEarned=Join-Path $qaRoot 'Validation\ThreeNpc-20261024\legacy-source.json'
$qaRows=[System.Collections.Generic.List[object]]::new()
if(Get-Process LightBetweenCultures,Unity -ErrorAction SilentlyContinue){throw 'Another Player/Editor is active; no overlapping input tests'}
$qaOwner='C:\Users\admin\AppData\LocalLow\DefaultCompany\LightBetweenCultures\hoi-an-v1.json'
$qaOwnerHash=if(Test-Path -LiteralPath $qaOwner){(Get-FileHash -LiteralPath $qaOwner -Algorithm SHA256).Hash}else{$null}
@{path=$qaOwner;hash=$qaOwnerHash;player=$qaExe;assembly=(Get-FileHash -LiteralPath (Join-Path (Split-Path $qaExe) 'LightBetweenCultures_Data\Managed\Assembly-CSharp.dll')).Hash}|ConvertTo-Json|Set-Content -Encoding UTF8 (Join-Path $qaOutput 'before.json')
function Invoke-ExistingRunner([string]$Name,[string[]]$Mode,[string]$Seed,[int]$Limit=300,[string]$SharedFolder=''){
    $qaFolder=if($SharedFolder){Join-Path $qaOutput $SharedFolder}else{Join-Path $qaOutput $Name}
    if(-not(Test-Path -LiteralPath $qaFolder)){New-Item -ItemType Directory -Path $qaFolder|Out-Null}
    if($Seed){Copy-Item -LiteralPath $Seed -Destination (Join-Path $qaFolder 'gameplay-save.json')}
    if($Name -eq 'three-npc'){Copy-Item -LiteralPath $qaEarned -Destination (Join-Path $qaFolder 'legacy-source.json')}
    $qaStart=[DateTime]::UtcNow
    $qaRecord=[pscustomobject]@{run=$Name;status='RUNNING';started=$qaStart.ToString('o');finished=$null;elapsedSeconds=0;peakWorkingSetBytes=0;assertions=0;seed=$Seed;arguments=$Mode;folder=$qaFolder;exitCode=$null}
    $qaRows.Add($qaRecord)
    $qaRows|ConvertTo-Json -Depth 6|Set-Content -Encoding UTF8 (Join-Path $qaOutput 'queue-results.json')
    $qaProcess=Start-Process -FilePath $qaExe -ArgumentList (@('--gameplay-smoke',"--gameplay-output=$qaFolder",'--audio-preferences=fullregression20261025','-logFile',(Join-Path $qaFolder ($Name+'-player.log')))+$Mode) -WorkingDirectory $qaRoot -WindowStyle Hidden -PassThru
    Write-Output "START $Name PID=$($qaProcess.Id)"
    $qaTimedOut=$false
    while(-not $qaProcess.HasExited){
        $qaProcess.Refresh()
        if(-not $qaProcess.HasExited){$qaRecord.peakWorkingSetBytes=[Math]::Max($qaRecord.peakWorkingSetBytes,$qaProcess.WorkingSet64)}
        if(([DateTime]::UtcNow-$qaStart).TotalSeconds -gt $Limit){$qaProcess.Kill();$qaTimedOut=$true;break}
        Start-Sleep -Seconds 2
    }
    $qaResult=Join-Path $qaFolder 'smoke-results.txt'
    $qaRecord.finished=[DateTime]::UtcNow.ToString('o');$qaRecord.elapsedSeconds=[int]([DateTime]::UtcNow-$qaStart).TotalSeconds
    $qaRecord.exitCode=$qaProcess.ExitCode
    if($qaTimedOut){$qaRecord.status='BLOCKED_TIMEOUT'}
    elseif(-not(Test-Path -LiteralPath $qaResult)){$qaRecord.status='BLOCKED_NO_RESULT'}
    elseif((Get-Item -LiteralPath $qaResult).LastWriteTimeUtc -lt $qaStart.AddSeconds(-1)){$qaRecord.status='BLOCKED_STALE_RESULT'}
    else{
        $qaText=Get-Content -Raw -Encoding UTF8 -LiteralPath $qaResult
        $qaRecord.status=if($qaText.TrimEnd().EndsWith('ALL CHECKS PASSED')){'PASS'}else{'FAIL'}
        $qaRecord.assertions=(Select-String -LiteralPath $qaResult -Pattern '^PASS:').Count
        Copy-Item -LiteralPath $qaResult -Destination (Join-Path $qaOutput ($Name+'-results.txt'))
        $qaCaptures=Join-Path $qaFolder 'capture-manifest.txt'
        if(Test-Path -LiteralPath $qaCaptures){Copy-Item -LiteralPath $qaCaptures -Destination (Join-Path $qaOutput ($Name+'-captures.txt'))}
    }
    $qaRows|ConvertTo-Json -Depth 6|Set-Content -Encoding UTF8 (Join-Path $qaOutput 'queue-results.json')
    Write-Output "DONE $Name $($qaRecord.status) checks=$($qaRecord.assertions) seconds=$($qaRecord.elapsedSeconds)"
}
try{
    if($IsolateVietnam){
        Invoke-ExistingRunner 'seoul-assembly-isolated' @('--phase4-city=seoul','--phase4-restore-only') (Join-Path $qaRoot 'Validation\FullRegression-20261025\after-adapter\campaign-vietnam\gameplay-save.json') 180
        if($qaRows[-1].status -eq 'PASS'){
            $qaContinuation=Join-Path $qaOutput 'vietnam-earned-continuation';New-Item -ItemType Directory -Force -Path $qaContinuation|Out-Null
            Copy-Item -LiteralPath (Join-Path $qaRoot 'Validation\FullRegression-20261025\after-adapter\campaign-vietnam-results.txt') -Destination (Join-Path $qaContinuation 'opening-new-game-results.txt')
            Invoke-ExistingRunner 'vietnam-earned-continuation' @('--pc-full-gameplay','--pc-resume-opening','--phase4-e2e=VN_FIRST') (Join-Path $qaOutput 'seoul-assembly-isolated\gameplay-save.json') 300
            if($qaRows[-1].status -eq 'PASS'){Invoke-ExistingRunner 'vietnam-earned-restart' @('--pc-gameplay-restart') '' 120 'vietnam-earned-continuation'}
        }
        return
    }
    if($AfterAdapter){
        Invoke-ExistingRunner 'campaign-vietnam' @('--pc-full-gameplay','--phase4-e2e=VN_FIRST') '' 1200
        if($qaRows[-1].status -eq 'PASS'){Invoke-ExistingRunner 'campaign-vietnam-restart' @('--pc-gameplay-restart') '' 120 'campaign-vietnam'}
        Invoke-ExistingRunner 'campaign-korea' @('--pc-full-gameplay','--phase4-e2e=KOREA_FIRST') '' 1200
        if($qaRows[-1].status -eq 'PASS'){Invoke-ExistingRunner 'campaign-korea-restart' @('--pc-gameplay-restart') '' 120 'campaign-korea'}
        Invoke-ExistingRunner 'dialogue-complete-fixtures' @('--dialogue-release-qa',"--buttons-partial-save=$qaRoot\Validation\ReleaseCandidate-20261024\shop-partial-hoian.json","--buttons-world-save=$qaRoot\Validation\ReleaseCandidate-20261024\focused\RC-earned.json") $qaEarned 240
        Invoke-ExistingRunner 'final-ux-latest-source' @('--final-ux-qa') $qaEarned 360
        Invoke-ExistingRunner 'leisure-twelve-latest-source' @('--casual-runtime') (Join-Path $qaRoot 'Validation\FinalGameplayAndroid-20261020\leisure12\Casual-earned-optional-lane.json') 360
        Invoke-ExistingRunner 'all-npc-approaches' @('--npc-standing-qa') $qaEarned 300
        Invoke-ExistingRunner 'three-npc' @('--three-npc-qa') '' 180
        if($qaRows[-1].status -eq 'PASS'){Invoke-ExistingRunner 'three-npc-restart' @('--three-npc-qa','--three-npc-restart') '' 180 'three-npc'}
        return
    }
    Invoke-ExistingRunner 'three-npc' @('--three-npc-qa') '' 180
    if($qaRows[-1].status -eq 'PASS'){Invoke-ExistingRunner 'three-npc-restart' @('--three-npc-qa','--three-npc-restart') '' 180 'three-npc'}
    Invoke-ExistingRunner 'campaign-vietnam' @('--pc-full-gameplay','--phase4-e2e=VN_FIRST') '' 1200
    if($qaRows[-1].status -eq 'PASS'){Invoke-ExistingRunner 'campaign-vietnam-restart' @('--pc-gameplay-restart') '' 120 'campaign-vietnam'}
    Invoke-ExistingRunner 'save-recovery-six-cities' @('--release-candidate-qa') $qaEarned 240
    Invoke-ExistingRunner 'final-ux-puzzles-relics-audio' @('--final-ux-qa') $qaEarned 360
    Invoke-ExistingRunner 'dialogue-npc-buttons' @('--dialogue-release-qa') $qaEarned 240
    Invoke-ExistingRunner 'node-panel-two-resolutions' @('--node-panel-qa') $qaEarned 240
    Invoke-ExistingRunner 'leisure-twelve' @('--casual-runtime') (Join-Path $qaRoot 'Validation\FinalGameplayAndroid-20261020\leisure12\Casual-earned-optional-lane.json') 360
    Invoke-ExistingRunner 'boats-junctions-six-city-graph' @('--owner-route-b',"--owner-route-cases=$qaRoot\Docs\Maps\OwnerRoutePhaseB-20261019\ferry-test-cases.json") $qaEarned 420
    Invoke-ExistingRunner 'campaign-korea' @('--pc-full-gameplay','--phase4-e2e=KOREA_FIRST') '' 1200
    if($qaRows[-1].status -eq 'PASS'){Invoke-ExistingRunner 'campaign-korea-restart' @('--pc-gameplay-restart') '' 120 'campaign-korea'}
}
finally{
    $qaAfter=if(Test-Path -LiteralPath $qaOwner){(Get-FileHash -LiteralPath $qaOwner -Algorithm SHA256).Hash}else{$null}
    @{path=$qaOwner;before=$qaOwnerHash;after=$qaAfter;unchanged=($qaAfter -eq $qaOwnerHash)}|ConvertTo-Json|Set-Content -Encoding UTF8 (Join-Path $qaOutput 'owner-save-verified.json')
    $qaRows|ConvertTo-Json -Depth 6|Set-Content -Encoding UTF8 (Join-Path $qaOutput 'queue-results.json')
    Write-Output "QUEUE_FINISHED ownerSaveUnchanged=$($qaAfter -eq $qaOwnerHash)"
}
