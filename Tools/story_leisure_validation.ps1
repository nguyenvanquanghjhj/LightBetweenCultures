param([string]$Job='pilot',[string]$Run='pilot01',[string]$Seed='',[string]$Order='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$folder=Join-Path $root "Validation\10Story12Leisure-20261019\$Run"
if(Test-Path -LiteralPath $folder){throw "Refuse overwrite $folder"}
New-Item -ItemType Directory -Path $folder | Out-Null
if($Seed){
 $source=Join-Path $root $Seed
 Copy-Item -LiteralPath $source -Destination (Join-Path $folder 'gameplay-save.json')
 @{source=$Seed;sha256=(Get-FileHash -LiteralPath $source).Hash;privateQA=$true}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $folder 'input-provenance.json') -Encoding UTF8
}
$launch=@('--gameplay-smoke',"--story-leisure-$Job",'--audio-preferences=story-leisure-20261019',"--gameplay-output=$folder",'-logFile',"$folder\player.log")
if($Order){$launch+="--phase4-e2e=$Order"}
$player=Start-Process -FilePath (Join-Path $root 'Builds\Windows\LightBetweenCultures.exe') -ArgumentList $launch -WindowStyle Hidden -PassThru -WorkingDirectory $root
@{pid=$player.Id;job=$Job;run=$Run;started=[DateTime]::UtcNow.ToString('o')}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $folder 'process.json') -Encoding UTF8
Write-Output "$Run PID $($player.Id)"
