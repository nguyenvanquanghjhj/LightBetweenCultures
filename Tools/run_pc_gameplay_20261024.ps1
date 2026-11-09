param([switch]$Resume)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$qa=Join-Path $root 'Validation\PcGameplayFix-20261024'
$folder=Join-Path $qa 'campaign'
$exe=Join-Path $root 'Builds\Windows-PcGameplayQA-20261024\LightBetweenCultures.exe'
$buildPid=[int](Get-Content -LiteralPath (Join-Path $qa 'windows-build.pid'))
$deadline=[DateTime]::UtcNow.AddMinutes(10)
while(Get-Process -Id $buildPid -ErrorAction SilentlyContinue){
    if([DateTime]::UtcNow -gt $deadline){throw 'BLOCKED_BUILD_TIMEOUT: no overlapping retry launched'}
    Start-Sleep -Seconds 2
}
if(-not(Select-String -LiteralPath (Join-Path $qa 'windows-build.log') -SimpleMatch 'PHASE5 WINDOWS BUILD: Succeeded' -Quiet)){throw 'BLOCKED_WINDOWS_BUILD: inspect windows-build.log'}
if(Get-Process LightBetweenCultures -ErrorAction SilentlyContinue){throw 'BLOCKED_CONCURRENT_PLAYER'}
if((Test-Path -LiteralPath (Join-Path $folder 'gameplay-save.json')) -and -not $Resume){throw 'Refusing to reuse a previous campaign save as New Game'}
function Run-Player([string[]]$Mode,[string]$Log,[int]$Seconds){
    $args=@('--gameplay-smoke',"--gameplay-output=$folder",'-logFile',(Join-Path $folder $Log))+$Mode
    $start=[DateTime]::UtcNow
    $process=Start-Process -FilePath $exe -ArgumentList $args -WorkingDirectory $root -WindowStyle Hidden -PassThru
    Write-Host "START $($Mode -join ' ') PID=$($process.Id)"
    while(-not $process.HasExited){
        if(([DateTime]::UtcNow-$start).TotalSeconds -gt $Seconds){$process.Kill();throw "BLOCKED_PLAYER_TIMEOUT: $Log"}
        Start-Sleep -Seconds 2;$process.Refresh()
    }
    $elapsed=([DateTime]::UtcNow-$start).TotalSeconds
    $resultFile=Join-Path $folder 'smoke-results.txt'
    if(-not(Test-Path -LiteralPath $resultFile)){throw "BLOCKED_NO_RESULT: $Log"}
    if((Get-Item -LiteralPath $resultFile).LastWriteTimeUtc -lt $start.AddSeconds(-1)){throw "BLOCKED_STALE_RESULT: $Log"}
    $last=Get-Content -LiteralPath $resultFile -Tail 1
    Write-Host "END $Log elapsed=$([int]$elapsed)s status=$last"
    if($last -ne 'ALL CHECKS PASSED'){Get-Content -LiteralPath $resultFile -Tail 12;throw "PLAYER_FAILED: $Log"}
    return $elapsed
}
try{
    $mode=@('--pc-full-gameplay','--phase4-e2e=VN_FIRST');if($Resume){$mode+='--pc-resume-opening'}
    $campaign=Run-Player $mode 'player.log' 1200
    Copy-Item -LiteralPath (Join-Path $folder 'smoke-results.txt') -Destination (Join-Path $folder 'pc-campaign-results.txt')
    $restart=Run-Player @('--pc-gameplay-restart') 'restart.log' 90
    $playthrough=Get-Content -LiteralPath (Join-Path $folder 'pc-playthrough.json') -Raw|ConvertFrom-Json
    $playthrough.saveLoad='PASS'
    $playthrough|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $folder 'pc-playthrough.json') -Encoding UTF8
    @{status='PC_FULL_PLAYTHROUGH_PASS';campaign=$campaign;restart=$restart;isolatedSave=(Join-Path $folder 'gameplay-save.json');player=$exe}|ConvertTo-Json -Depth 3|Set-Content -LiteralPath (Join-Path $qa 'result.json') -Encoding UTF8
    Write-Output 'PC_FULL_PLAYTHROUGH_PASS + SEPARATE_PROCESS_SAVE_LOAD_PASS'
}catch{
    @{status='BLOCKED_OR_FAILED';reason=$_.Exception.Message;player=$exe}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $qa 'result.json') -Encoding UTF8
    throw
}
