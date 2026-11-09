$ErrorActionPreference='Stop'
$taskRoot=[IO.Path]::GetFullPath('D:\GAMEKKK\LightBetweenCultures').TrimEnd('\')
$taskAudit=Join-Path $taskRoot 'Docs\Storage\SafeCleanup-20261023'
if(Test-Path -LiteralPath (Join-Path $taskAudit 'deletion-result.json')){throw 'This one-time cleanup was already applied. Do not rerun historical cleanup.'}
$taskPlan=Get-Content -LiteralPath (Join-Path $taskAudit 'delete-plan.json') -Raw | ConvertFrom-Json
$taskAllow=@(
 'Library\Bee\Android\Prj\IL2CPP\Gradle\unityLibrary\build',
 'Library\Bee\Android\Prj\IL2CPP\Gradle\launcher\build',
 'Library\Bee\artifacts\objcopy_5lmi',
 'Library\PlayerDataCache\Win642',
 'Tools\__pycache__'
)
$taskProcesses=@(Get-CimInstance Win32_Process)
$taskBusy=@($taskProcesses | Where-Object {
 $_.Name -match '^(Unity|UnityShaderCompiler|LightBetweenCultures|bee_backend)\.exe$' -or
 ($_.Name -match '^(java|dotnet)\.exe$' -and $_.CommandLine -and ($_.CommandLine -like '*LightBetweenCultures\Library*' -or $_.CommandLine -like '*AndroidPlayer*gradle*'))
})
if($taskBusy.Count){throw ('Build/Player still active: '+($taskBusy.ProcessId -join ','))}
if($taskPlan.targets.Count -ne $taskAllow.Count){throw 'Unexpected target count'}
$taskApproved=@()
foreach($taskItem in $taskPlan.targets){
 $taskRelative=$taskItem.relativePath.Replace('/','\')
 if($taskAllow -notcontains $taskRelative){throw "Not allowlisted: $taskRelative"}
 $taskAbsolute=[IO.Path]::GetFullPath((Join-Path $taskRoot $taskRelative)).TrimEnd('\')
 if(!$taskAbsolute.StartsWith($taskRoot+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Target escaped workspace'}
 if($taskAbsolute -ne [IO.Path]::GetFullPath($taskItem.absolutePath).TrimEnd('\')){throw 'Resolved path mismatch'}
 $taskLong='\\?\'+$taskAbsolute
 $taskTree=@(Get-Item -LiteralPath $taskLong)+@(Get-ChildItem -LiteralPath $taskLong -Recurse -Force)
 if(@($taskTree | Where-Object {$_.Attributes -band [IO.FileAttributes]::ReparsePoint}).Count){throw 'Reparse point in target'}
 $taskFiles=@($taskTree | Where-Object {!$_.PSIsContainer})
 if($taskFiles.Count -ne $taskItem.files -or ($taskFiles | Measure-Object Length -Sum).Sum -ne $taskItem.bytes){throw 'Cache changed after audit'}
 $taskApproved+=[pscustomobject]@{path=$taskAbsolute;longPath=$taskLong;bytes=[long]$taskItem.bytes;files=$taskItem.files}
}
# Every recursive target has now been resolved, checked and confined to the
# fixed allowlist. No path enumeration is passed to another shell.
$taskDeleted=@()
foreach($taskItem in $taskApproved){
 Remove-Item -LiteralPath $taskItem.longPath -Recurse -Force
 if(Test-Path -LiteralPath $taskItem.longPath){throw 'Cache removal incomplete'}
 $taskDeleted+=[pscustomobject]@{path=$taskItem.path;bytes=$taskItem.bytes;files=$taskItem.files}
}
@{deleted=$taskDeleted;bytes=($taskDeleted | Measure-Object bytes -Sum).Sum;files=($taskDeleted | Measure-Object files -Sum).Sum;noBuildActive=$true} |
 ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $taskAudit 'deletion-result.json') -Encoding UTF8
Get-Content -LiteralPath (Join-Path $taskAudit 'deletion-result.json')
