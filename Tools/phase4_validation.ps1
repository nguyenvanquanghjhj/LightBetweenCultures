param([string]$Suite='final')
$ErrorActionPreference='Stop'
$project=Split-Path -Parent $PSScriptRoot
$qa=Join-Path $project 'Validation\Phase4-20261018'
$exe=Join-Path $project 'Builds\Windows\LightBetweenCultures.exe'
$jobs=@()
if($Suite -eq 'final') {
    $jobs+=@{name='e2e-korea03';mode='--phase4-e2e=KOREA_FIRST'}
    $jobs+=@{name='retro-vn01';mode='--phase4-retro=VN_FIRST';seed='Validation\FullStory-20261017\Fresh04\full-story-final.json'}
    $jobs+=@{name='retro-korea01';mode='--phase4-retro=KOREA_FIRST';seed='Validation\FullStory-20261017\Reverse03\full-story-final.json'}
    foreach($branch in @('battle','hack')) {
        foreach($state in @('before-H5','after-H5','after-H7')) {
            $jobs+=@{name="key-restart-$branch-$state";mode='--phase4-key-restart';seed="Validation\Phase4-20261018\key-$($branch)01\key-$state.json"}
        }
    }
    $jobs+=@{name='key-retained-fixture';mode='--phase4-key-restart';seed='Validation\Phase4-20261018\key-battle01\key-after-H7.json';retainedFixture=$true}
    $jobs+=@{name='presentation01';mode='--phase4-presentation';seed='Validation\Phase4-20261018\e2e-vn01\P4-E2E-VN_FIRST-final.json'}
} elseif($Suite -eq 'culture') {
    $jobs+=@{name='hue-culture02';mode='--phase4-city=hue';extra='--phase4-restore-only';seed='Validation\Phase4-20261018\retro-korea01\phase4-hue-assembled.json'}
} else {throw 'Unknown explicit suite'}
foreach($job in $jobs) {
    $folder=Join-Path $qa $job.name
    if(Test-Path -LiteralPath $folder) {throw "Refuse to overwrite QA evidence: $folder"}
    New-Item -ItemType Directory -Path $folder | Out-Null
    if($job.seed) {
        $seed=Join-Path $project $job.seed
        Copy-Item -LiteralPath $seed -Destination (Join-Path $folder 'gameplay-save.json')
        @{seed=$job.seed;sha256=(Get-FileHash -LiteralPath $seed -Algorithm SHA256).Hash;fixture=[bool]$job.retainedFixture} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $folder 'input-provenance.json') -Encoding UTF8
    }
    if($job.retainedFixture) {
        # Isolated compatibility counterexample, NOT an earned source/reward.
        $save=Join-Path $folder 'gameplay-save.json'
        $data=Get-Content -LiteralPath $save -Raw -Encoding UTF8 | ConvertFrom-Json
        $data.items+= 'link_key'
        [IO.File]::WriteAllText($save,($data|ConvertTo-Json -Depth 60),[Text.UTF8Encoding]::new($false))
    }
    $args=@('--gameplay-smoke',"--gameplay-output=$folder",$job.mode,'-screen-width','540','-screen-height','960','-logFile',"$folder\player.log")
    if($job.extra) {$args+=$job.extra}
    $player=Start-Process -FilePath $exe -ArgumentList $args -WindowStyle Hidden -PassThru -WorkingDirectory $project
    Write-Output "Started $($job.name) PID $($player.Id)"
    $deadline=[DateTime]::UtcNow.AddMinutes(18)
    while(-not $player.WaitForExit(45000)) {
        Write-Output "Running $($job.name)"
        if([DateTime]::UtcNow -gt $deadline) {throw 'Bounded QA timeout; inspect this Player before further runs'}
    }
    $result=Join-Path $folder $(if($job.mode -eq '--phase4-key-restart') {'restart-results.txt'} else {'smoke-results.txt'})
    if(-not(Test-Path -LiteralPath $result)) {throw "No result: $($job.name)"}
    $last=Get-Content -LiteralPath $result -Tail 1
    Write-Output "$($job.name): $last"
    if($last -ne 'ALL CHECKS PASSED') {Get-Content -LiteralPath $result -Tail 12;throw 'Stop on failure; do not skip dependent QA'}
}
