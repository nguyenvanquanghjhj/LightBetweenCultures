param(
    [Parameter(Mandatory=$true)][string]$Apk,
    [Parameter(Mandatory=$true)][string]$Output,
    [string]$Editor='C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe'
)
$ErrorActionPreference='Stop'
$apkPath=(Resolve-Path -LiteralPath $Apk).Path
if(Test-Path -LiteralPath $Output){throw 'Existing evidence is protected; choose a new output directory'}
$sdk=Join-Path (Split-Path -Parent $Editor) 'Data\PlaybackEngines\AndroidPlayer\SDK'
$env:JAVA_HOME=Join-Path (Split-Path -Parent $Editor) 'Data\PlaybackEngines\AndroidPlayer\OpenJDK'
$buildTools=Get-ChildItem -LiteralPath (Join-Path $sdk 'build-tools') -Directory | Sort-Object Name -Descending | Select-Object -First 1
New-Item -ItemType Directory -Path $Output | Out-Null
$checks=@{apk=$apkPath;sha256=(Get-FileHash -LiteralPath $apkPath -Algorithm SHA256).Hash}
& (Join-Path $buildTools.FullName 'aapt.exe') dump badging $apkPath 2>&1 | Out-File (Join-Path $Output 'badging.txt') -Encoding UTF8
$checks.badgingExit=$LASTEXITCODE
& (Join-Path $buildTools.FullName 'apksigner.bat') verify --verbose --print-certs $apkPath 2>&1 | Out-File (Join-Path $Output 'signature.txt') -Encoding UTF8
$checks.signatureExit=$LASTEXITCODE
& (Join-Path $buildTools.FullName 'zipalign.exe') -c -P 16 4 $apkPath 2>&1 | Out-File (Join-Path $Output 'alignment.txt') -Encoding UTF8
$checks.alignmentExit=$LASTEXITCODE
$checks | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $Output 'result.json') -Encoding UTF8
if($checks.badgingExit -ne 0 -or $checks.signatureExit -ne 0 -or $checks.alignmentExit -ne 0){throw 'APK verification failed; inspect retained evidence'}
