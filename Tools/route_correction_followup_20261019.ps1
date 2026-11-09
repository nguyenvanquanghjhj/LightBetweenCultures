$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$qa=Join-Path $root 'Validation\RouteShorteningProposal-20261019'
function Invoke-RouteCheck([string]$name,[string]$mode,[string]$seed='') {
    if(Get-Process LightBetweenCultures -ErrorAction SilentlyContinue){throw 'A Player is already active'}
    $folder=Join-Path $qa $name
    if(Test-Path -LiteralPath $folder){throw "Refuse overwrite: $folder"}
    New-Item -ItemType Directory -Path $folder | Out-Null
    if($seed){
        Copy-Item -LiteralPath $seed -Destination (Join-Path $folder 'gameplay-save.json')
        @{source=$seed;sha256=(Get-FileHash -LiteralPath $seed).Hash;scope='Private QA save; explicit camera/location fixtures only'} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $folder 'input-provenance.json') -Encoding UTF8
    }
    $launch=@('--gameplay-smoke',$mode,'--audio-preferences=route-correction-20261019',"--gameplay-output=$folder",'-logFile',"$folder\player.log")
    $player=Start-Process -FilePath (Join-Path $root 'Builds\Windows\LightBetweenCultures.exe') -ArgumentList $launch -WorkingDirectory $root -WindowStyle Hidden -PassThru
    $deadline=[DateTime]::UtcNow.AddMinutes(4)
    while(-not $player.WaitForExit(10000)) {
        Write-Output "Running $name"
        if([DateTime]::UtcNow-gt$deadline){throw "Bounded timeout $name; inspect, do not start another Player"}
    }
    $result=Join-Path $folder 'smoke-results.txt'
    if(-not(Test-Path -LiteralPath $result)){throw "Missing result $name"}
    if((Get-Content -LiteralPath $result -Tail 1)-ne'ALL CHECKS PASSED'){throw "FAIL $name"}
    Write-Output "PASS $name"
}
Invoke-RouteCheck 'Runtime-AfterOverview01' '--route35-before' (Join-Path $root 'Validation\Phase5-20261018\tail-vn01\P5-ending-completed.json')
Invoke-RouteCheck 'Runtime-Fresh01' '--approved-routes-fresh'
foreach($city in @('hoi_an','hue','da_lat','jeonju','gyeongju','seoul')) {
    Invoke-RouteCheck "Runtime-Restart-$city-01" '--approved-routes-restart' (Join-Path $qa "restart-seeds\$city.json")
}
Write-Output 'ROUTE_FOLLOWUP_PASS'
