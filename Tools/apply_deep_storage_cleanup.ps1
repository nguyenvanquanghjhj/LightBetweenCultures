param([ValidateSet('OutputsAndCaches','ApprovedCheckpoints')][string]$Scope='OutputsAndCaches')
$ErrorActionPreference='Stop'
$taskRoot='D:\GAMEKKK\LightBetweenCultures'
$taskAudit=Join-Path $taskRoot 'Docs\Storage\DeepCleanup-20261020'
$taskPlan=Get-Content -LiteralPath (Join-Path $taskAudit 'delete-plan.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$taskItems=@($taskPlan.items | Where-Object {
    $taskIsBackup=$_.path.StartsWith('D:\GAMEKKK\Backups\',[StringComparison]::OrdinalIgnoreCase)
    if($Scope -eq 'ApprovedCheckpoints'){$taskIsBackup}else{-not $taskIsBackup}
})
$taskBackup=[IO.Path]::GetFullPath($taskPlan.sourceBackup.path).TrimEnd('\')
if((Get-FileHash -LiteralPath (Join-Path $taskBackup 'SOURCE_MANIFEST.json') -Algorithm SHA256).Hash.ToLowerInvariant() -ne $taskPlan.sourceBackup.manifestSHA256){throw 'Source backup manifest changed'}
$taskBusy=Get-CimInstance Win32_Process | Where-Object { $_.Name -match '^(Unity(?:\.exe|Hub\.exe|ShaderCompiler\.exe|\.ILPP)|LightBetweenCultures\.exe|bee_backend\.exe|gradle\.exe|msbuild\.exe|robocopy\.exe|7z\.exe)$' }
if($taskBusy){throw ('Relevant process active; no cleanup: '+(($taskBusy | Select-Object -ExpandProperty Name)-join ','))}
$taskProtected=@($taskBackup,[IO.Path]::GetFullPath($taskPlan.keepWindows),[IO.Path]::GetFullPath($taskPlan.keepAPK),[IO.Path]::GetFullPath($taskPlan.retainedLegacyToolDependency))
$taskAllowed=@((Join-Path $taskRoot 'Validation'),(Join-Path $taskRoot 'Docs\QA'),(Join-Path $taskRoot 'Logs'),(Join-Path $taskRoot 'Builds'),'D:\GAMEKKK\Backups',(Join-Path $taskRoot 'Library\Bee'),(Join-Path $taskRoot 'Library\PlayerDataCache'),(Join-Path $taskRoot 'Library\BurstCache'),(Join-Path $taskRoot 'Library\BuildHistory'),(Join-Path $taskRoot 'Library\BuildPlayerData'),(Join-Path $taskRoot 'Temp'),(Join-Path $taskRoot 'obj'),(Join-Path $taskRoot 'build'),(Join-Path $taskRoot '.utmp'),(Join-Path $taskRoot 'Tools\__pycache__'))
$taskTemp=[IO.Path]::GetFullPath((Join-Path $env:LOCALAPPDATA 'Temp')).TrimEnd('\')
$taskCrash=[IO.Path]::GetFullPath((Join-Path $env:LOCALAPPDATA 'CrashDumps')).TrimEnd('\')
function Test-TaskWithin([string]$path,[string]$root){return $path.Equals($root,[StringComparison]::OrdinalIgnoreCase) -or $path.StartsWith($root.TrimEnd('\')+'\',[StringComparison]::OrdinalIgnoreCase)}
# Validate every fully resolved target before the first recursive removal.
foreach($taskEntry in $taskItems){
    $taskPath=[IO.Path]::GetFullPath($taskEntry.path).TrimEnd('\')
    foreach($taskKeep in $taskProtected){if((Test-TaskWithin $taskPath $taskKeep) -or (Test-TaskWithin $taskKeep $taskPath)){throw ('Protected path in cleanup plan: '+$taskPath)}}
    $taskOk=$false
    foreach($taskBase in $taskAllowed){if((Test-TaskWithin $taskPath $taskBase) -and $taskPath -ne 'D:\GAMEKKK\Backups'){$taskOk=$true;break}}
    if($taskPath -eq (Join-Path $taskRoot 'Submission\KGW2026\LightBetweenCultures_Windows_20261020.zip')){$taskOk=$true}
    if([IO.Path]::GetDirectoryName($taskPath) -eq $taskRoot -and ([IO.Path]::GetFileName($taskPath) -match '(Report\.md|Review\.html|^AUDIT_VI\.md)$')){$taskOk=$true}
    if([IO.Path]::GetDirectoryName($taskPath) -eq $taskTemp -and [IO.Path]::GetFileName($taskPath) -like 'UnityVBCSPreWarm_*'){$taskOk=$true}
    if(Test-TaskWithin $taskPath (Join-Path $taskTemp 'UnityCBMCrashes')){$taskOk=$true}
    if([IO.Path]::GetDirectoryName($taskPath) -eq $taskCrash -and [IO.Path]::GetFileName($taskPath) -like 'Unity.ILPP.Trigger.exe.*.dmp'){$taskOk=$true}
    if(-not $taskOk){throw ('Target outside exact cleanup scope: '+$taskPath)}
    if(-not (Test-Path -LiteralPath $taskPath)){throw ('Target disappeared: '+$taskPath)}
    $taskInfo=Get-Item -LiteralPath $taskPath -Force
    if(($taskInfo.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0){throw ('Reparse target: '+$taskPath)}
    if($taskEntry.directory){
        $taskLinks=Get-ChildItem -LiteralPath $taskPath -Force -Recurse -Attributes ReparsePoint -ErrorAction Stop | Select-Object -First 1
        if($taskLinks){throw ('Nested reparse point: '+$taskPath)}
    }elseif($taskInfo.Length -ne $taskEntry.bytes){throw ('File changed since planning: '+$taskPath)}
}
$taskStart=[DateTime]::UtcNow.ToString('O')
$taskBefore=@{};foreach($taskDisk in [IO.DriveInfo]::GetDrives()){if($taskDisk.Name -in @('C:\','D:\')){$taskBefore[$taskDisk.Name]=$taskDisk.AvailableFreeSpace}}
$taskDone=0;$taskBytes=[long]0;$taskFailures=New-Object System.Collections.Generic.List[object]
foreach($taskEntry in $taskItems){
    try{
        if(-not $taskEntry.directory){$taskLock=[IO.File]::Open($taskEntry.path,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::None);$taskLock.Dispose()}
        if($taskEntry.directory){Remove-Item -LiteralPath $taskEntry.path -Recurse -Force -ErrorAction Stop}else{Remove-Item -LiteralPath $taskEntry.path -Force -ErrorAction Stop}
        $taskDone++;$taskBytes+=[long]$taskEntry.bytes
    }catch{$taskFailures.Add(@{path=$taskEntry.path;reason=$_.Exception.Message})}
}
$taskAfter=@{};foreach($taskDisk in [IO.DriveInfo]::GetDrives()){if($taskDisk.Name -in @('C:\','D:\')){$taskAfter[$taskDisk.Name]=$taskDisk.AvailableFreeSpace}}
@{scope=$Scope;startedUTC=$taskStart;finishedUTC=[DateTime]::UtcNow.ToString('O');deletedEntries=$taskDone;deletedLogicalBytes=$taskBytes;skipped=$taskFailures;freeBeforeApply=$taskBefore;freeAfterApply=$taskAfter} | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $taskAudit ('deletion-result-'+$Scope+'.json')) -Encoding UTF8
Write-Output ('Deleted entries: '+$taskDone+'; skipped: '+$taskFailures.Count+'; logical GiB: '+[Math]::Round($taskBytes/1GB,3))
foreach($taskName in @('C:\','D:\')){Write-Output ($taskName+' actual free delta during deletion: '+[Math]::Round(($taskAfter[$taskName]-$taskBefore[$taskName])/1GB,3)+' GiB')}
