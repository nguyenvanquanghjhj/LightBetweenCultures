$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$qa=Join-Path $root 'Validation\HoiAnLightingB20-20261018'
$exe=Join-Path $root 'Builds\Windows\LightBetweenCultures.exe'
$jobs=@(
    @{name='after03';source='Validation\Phase4-20261018\e2e-vn01\state-B.json'},
    @{name='restart-B';source='Validation\HoiAnLightingB20-20261018\after03\restart-final.json'},
    @{name='after-H7';source='Validation\Phase4-20261018\e2e-vn01\state-B-H7.json'},
    @{name='check-A';source='Validation\Phase4-20261018\e2e-vn01\state-A.json'},
    @{name='check-C';source='Validation\Phase4-20261018\e2e-vn01\state-C.json'}
)
foreach($job in $jobs) {
    $folder=Join-Path $qa $job.name
    if(Test-Path -LiteralPath $folder){throw "Refuse to overwrite QA evidence: $folder"}
    New-Item -ItemType Directory -Path $folder | Out-Null
    $seed=Join-Path $root $job.source
    Copy-Item -LiteralPath $seed -Destination (Join-Path $folder 'gameplay-save.json')
    @{source=$job.source;sha256=(Get-FileHash -LiteralPath $seed -Algorithm SHA256).Hash;fixture=$false}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $folder 'input-provenance.json') -Encoding UTF8
    $player=Start-Process -FilePath $exe -ArgumentList @('--gameplay-smoke','--hoi-an-lighting-restart',"--gameplay-output=$folder",'-logFile',"$folder\player.log") -WindowStyle Hidden -PassThru
    Write-Output "Started $($job.name) PID $($player.Id)"
    $deadline=[DateTime]::UtcNow.AddMinutes(5)
    while(-not $player.WaitForExit(45000)) {
        Write-Output "Running $($job.name)"
        if([DateTime]::UtcNow -gt $deadline){throw 'Bounded QA timeout; inspect Player before another run'}
    }
    $result=Join-Path $folder 'restart-results.txt'
    $last=Get-Content -LiteralPath $result -Tail 1
    Write-Output "$($job.name): $last"
    if($last -ne 'ALL CHECKS PASSED'){Get-Content -LiteralPath $result -Tail 12;throw 'Stop on failure'}
}
