$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$qa=Join-Path $root 'Validation\10Story12Leisure-20261019'
function Wait-Result([string]$name) {
    $folder=Join-Path $qa $name
    $deadline=[DateTime]::UtcNow.AddMinutes(25)
    while(-not(Test-Path -LiteralPath (Join-Path $folder 'smoke-results.txt'))) {
        if([DateTime]::UtcNow -gt $deadline){throw "Bounded QA timeout: $name"}
        Start-Sleep -Seconds 4
    }
    $result=Get-Content -LiteralPath (Join-Path $folder 'smoke-results.txt') -Tail 1
    Write-Output "$name : $result"
    if($result -ne 'ALL CHECKS PASSED'){throw "Stop dependent QA after failure: $name"}
    $state=Get-Content -LiteralPath (Join-Path $folder 'process.json') -Raw | ConvertFrom-Json
    $player=Get-Process -Id $state.pid -ErrorAction SilentlyContinue
    if($player){$null=$player.WaitForExit(15000)}
}
Wait-Result 'recheck01'
foreach($order in @('VN_FIRST','KOREA_FIRST')) {
    $name=if($order -eq 'VN_FIRST'){'e2e-vn01'}else{'e2e-korea01'}
    & "$PSScriptRoot\story_leisure_validation.ps1" -Job e2e -Run $name -Order $order
    Wait-Result $name
}
& "$PSScriptRoot\story_leisure_validation.ps1" -Job restart -Run restart-partial01 -Seed 'Validation/10Story12Leisure-20261019/pilot02/SL-pilot-complete.json'
Wait-Result 'restart-partial01'
& "$PSScriptRoot\story_leisure_validation.ps1" -Job restart -Run restart-all12-01 -Seed 'Validation/10Story12Leisure-20261019/all12-01/SL-all12-optional.json'
Wait-Result 'restart-all12-01'
Write-Output 'STORY_LEISURE_SUITE_PASS'
