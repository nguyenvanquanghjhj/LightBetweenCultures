$ErrorActionPreference = 'Stop'
$auditRoot = 'D:\GAMEKKK\LightBetweenCultures\Docs\Storage\SafeCleanup-20261019'
$gradleRoot = 'D:\GAMEKKK\LightBetweenCultures\Library\Bee\Android\Prj\IL2CPP\Gradle'
$permitted = @(
    "$gradleRoot\unityLibrary\build\intermediates",
    "$gradleRoot\launcher\build\intermediates"
)
$plan = Get-Content -LiteralPath "$auditRoot\cleanup-plan.json" -Raw | ConvertFrom-Json
$processes = @(Get-CimInstance Win32_Process)
$blockers = @($processes | Where-Object {
    ($_.Name -ieq 'Unity.exe' -and $_.ExecutablePath -ine 'C:\Program Files\Unity Hub\resources\unity.exe') -or
    $_.Name -match '^(LightBetweenCultures.*|UnityShaderCompiler|UnityPackageManager|java|javaw|gradle|bee_backend|clang|clang\+\+|il2cpp|UnityLinker|dotnet|MSBuild)\.exe$'
})
$processes | Where-Object { $_.Name -match 'Unity|LightBetweenCultures|java|gradle|bee|clang|dotnet' } |
    Select-Object ProcessId, Name, ExecutablePath |
    ConvertTo-Json -Depth 3 | Set-Content -LiteralPath "$auditRoot\process-check.json" -Encoding UTF8
if ($blockers.Count -gt 0) { throw 'Editor/Player/build process is active or cannot be identified safely. No cleanup.' }
if ($plan.targets.Count -ne 2) { throw 'Unexpected cleanup target count.' }
foreach ($guard in $plan.guards) {
    if ((Get-FileHash -LiteralPath $guard.path -Algorithm SHA256).Hash -ine $guard.sha256) {
        throw "Protected file changed: $($guard.path)"
    }
}
# Validate every target, every ancestor, and every child before deleting anything.
foreach ($target in $plan.targets) {
    $resolved = (Resolve-Path -LiteralPath $target.path).ProviderPath
    if ($permitted -inotcontains $resolved -or -not $resolved.StartsWith($gradleRoot + '\', [StringComparison]::OrdinalIgnoreCase)) {
        throw "Target outside the literal approved list: $resolved"
    }
    $ancestor = Get-Item -LiteralPath $resolved
    while ($null -ne $ancestor) {
        if ($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Reparse point: $($ancestor.FullName)" }
        $ancestor = $ancestor.Parent
    }
    $children = @(Get-ChildItem -LiteralPath $resolved -Recurse -Force)
    if (@($children | Where-Object { $_.Attributes -band [IO.FileAttributes]::ReparsePoint }).Count) { throw 'Child reparse point.' }
    $files = @($children | Where-Object { -not $_.PSIsContainer })
    if ($files.Count -ne $target.files.Count) { throw "Target changed since inventory: $resolved" }
    foreach ($entry in $target.files) {
        $file = Get-Item -LiteralPath $entry.path
        if ($file.Length -ne $entry.bytes -or $file.LastWriteTimeUtc.Ticks -ne $entry.mtimeTicks) {
            throw "File changed since inventory: $($entry.path)"
        }
        if ($file.Extension -in @('.apk', '.aab', '.keystore', '.jks')) { throw 'Protected binary/signing asset in deletion plan.' }
    }
}
$results = @()
foreach ($target in $plan.targets) {
    Remove-Item -LiteralPath $target.path -Recurse -Force
    if (Test-Path -LiteralPath $target.path) { throw "Cleanup incomplete: $($target.path)" }
    $results += [PSCustomObject]@{ path=$target.path; bytes=$target.bytes; files=$target.files.Count; removed=$true }
    $results | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath "$auditRoot\deletion-results.json" -Encoding UTF8
}
foreach ($guard in $plan.guards) {
    if ((Get-FileHash -LiteralPath $guard.path -Algorithm SHA256).Hash -ine $guard.sha256) { throw "Post-cleanup guard failed: $($guard.path)" }
}
[PSCustomObject]@{ removedBytes=$plan.totalBytes; removedFiles=($results | Measure-Object files -Sum).Sum; protectedHashChecks=$plan.guards.Count; status='PASS' } |
    ConvertTo-Json | Tee-Object -FilePath "$auditRoot\verification.json"
