param([string]$Apk='Builds\Android\LightBetweenCultures-Test.apk')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$out=Join-Path $root 'Docs\QA\AndroidApk-20261019'
$sdk='C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK'
$env:JAVA_HOME='C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK'
$apkPath=(Resolve-Path -LiteralPath (Join-Path $root $Apk)).Path
$build=Get-Content -LiteralPath (Join-Path $out 'build-result.json') -Raw | ConvertFrom-Json
if($build.result -ne 'Succeeded'){throw 'Unity did not report a successful build'}
$exit=Get-Content -LiteralPath (Join-Path $out 'editor-exit.json') -Raw | ConvertFrom-Json
if($exit.exitCode -ne 0){throw 'Unity Editor did not exit successfully'}
$checks=@{}
& "$sdk\build-tools\36.0.0\aapt.exe" dump badging $apkPath 2>&1 | Out-File (Join-Path $out 'apk-badging.txt') -Encoding UTF8
$checks.aaptExit=$LASTEXITCODE
& "$sdk\build-tools\36.0.0\aapt.exe" dump permissions $apkPath 2>&1 | Out-File (Join-Path $out 'apk-permissions.txt') -Encoding UTF8
$checks.permissionsExit=$LASTEXITCODE
& "$sdk\build-tools\36.0.0\apksigner.bat" verify --verbose --print-certs $apkPath 2>&1 | Out-File (Join-Path $out 'apk-signature.txt') -Encoding UTF8
$checks.signatureExit=$LASTEXITCODE
& "$sdk\build-tools\36.0.0\zipalign.exe" -c -P 16 4 $apkPath 2>&1 | Out-File (Join-Path $out 'apk-alignment.txt') -Encoding UTF8
$checks.alignmentExit=$LASTEXITCODE
$checks.apk=$apkPath
$checks.bytes=(Get-Item -LiteralPath $apkPath).Length
$checks.sha256=(Get-FileHash -LiteralPath $apkPath -Algorithm SHA256).Hash.ToLowerInvariant()
$checks.verifiedUTC=[DateTime]::UtcNow.ToString('o')
$checks.device='ANDROID_DEVICE_NOT_TESTED'
$checks | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $out 'apk-tool-verification.json') -Encoding UTF8
$checks | ConvertTo-Json
if($checks.aaptExit -ne 0 -or $checks.permissionsExit -ne 0 -or $checks.signatureExit -ne 0 -or $checks.alignmentExit -ne 0){throw 'APK verification tool failed; inspect retained outputs'}
