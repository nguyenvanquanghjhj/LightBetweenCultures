$ErrorActionPreference='Stop'
$qaProject='D:\GAMEKKK\LightBetweenCultures'
$qaRoot=Join-Path $qaProject 'Validation\GenuinelyFun-20261020'
$qaExe=Join-Path $qaProject 'Builds\Windows-GameFeel-20261020\LightBetweenCultures.exe'
$qaRows=@()
function Invoke-FeelQa($qaName,$qaArguments,$qaSeed) {
    $qaFolder=Join-Path $qaRoot $qaName
    if(Test-Path -LiteralPath $qaFolder){throw "Refuse existing run $qaName"}
    New-Item -ItemType Directory -Path $qaFolder | Out-Null
    if($qaSeed){Copy-Item -LiteralPath $qaSeed -Destination "$qaFolder\gameplay-save.json"}
    $qaStart=Get-Date
    $qaProcess=Start-Process -FilePath $qaExe -ArgumentList (@('--gameplay-smoke',"--gameplay-output=$qaFolder",'-logFile',"$qaFolder\player.log")+$qaArguments) -WindowStyle Hidden -PassThru
    Write-Output "START $qaName PID=$($qaProcess.Id)"
    $qaPeak=0L;$qaBlocked=$false
    while(-not $qaProcess.HasExited){$qaProcess.Refresh();if(-not $qaProcess.HasExited){$qaPeak=[Math]::Max($qaPeak,$qaProcess.WorkingSet64)};if(((Get-Date)-$qaStart).TotalSeconds -gt 480){$qaProcess.Kill();$qaBlocked=$true;break};Start-Sleep -Seconds 2}
    $qaFile=Join-Path $qaFolder 'smoke-results.txt'
    $qaResult=if($qaBlocked){'BLOCKED_TIMEOUT'}elseif(-not(Test-Path -LiteralPath $qaFile)){'BLOCKED_NO_RESULT'}elseif((Get-Content -LiteralPath $qaFile -Raw).TrimEnd().EndsWith('ALL CHECKS PASSED')){'PASS'}else{'FAIL'}
    $script:qaRows+=[pscustomobject]@{run=$qaName;result=$qaResult;seed=$qaSeed;started=$qaStart.ToString('o');finished=(Get-Date).ToString('o');peakWindowsWorkingSetBytes=$qaPeak;arguments=$qaArguments;build='game-feel-windows02'}
    $script:qaRows | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath "$qaRoot\runs.json" -Encoding UTF8
    Write-Output "DONE $qaName $qaResult"
}
if(Get-Process -Name LightBetweenCultures -ErrorAction SilentlyContinue){throw 'Another Player is running; do not overlap UI input'}
Invoke-FeelQa 'cultural-actions01' @('--game-feel','--game-feel-more') "$qaProject\Validation\FinalGameplayAndroid-20261020\leisure12\Casual-earned-optional-lane.json"
Invoke-FeelQa 'hoi-an02' @('--game-feel','--game-feel-fresh','--casual-core') $null
Invoke-FeelQa 'fallback-four01' @('--leisure-full','--leisure-full-only=H16,D9,G10,G11') "$qaProject\Validation\FinalGameplayAndroid-20261020\leisure12\Casual-earned-optional-lane.json"
Invoke-FeelQa 'star-shield01' @('--game-feel','--game-feel-star') "$qaProject\Validation\FinalGameplayAndroid-20261020\fresh-korea\phase4-gyeongju-before-guard.json"
Invoke-FeelQa 'boss-two-phase01' @('--game-feel','--game-feel-boss') "$qaProject\Validation\Phase5-20261018\no-relics02\P5-boss-phase1.json"
Invoke-FeelQa 'activity-restart01' @('--leisure-full-restart') "$qaRoot\cultural-actions01\GF-cultural-actions-earned.json"
