<#
.SYNOPSIS
    One-time setup for the daily TodoAppDb backup (NFR-5). Run in an elevated PowerShell ("Run as administrator").

.DESCRIPTION
    1. Creates the backup folder (outside the project and the git repository).
    2. Gives the SQL Server service account Modify permission on that folder, so BACKUP DATABASE can write there.
    3. Copies ops\backup-todoapp.ps1 into the backup folder, so the schedule does not depend on a git checkout,
       and registers a Windows Task Scheduler task that runs that copy every day at -Time,
       "whether the user is logged on or not", and also runs a missed backup as soon as possible.
       Windows asks for your password in its own prompt; this script does not store or print it.
    4. With -RunNow, starts the task once so the first backup and the log line can be checked.

.EXAMPLE
    # From the folder that holds this script, in an elevated PowerShell:
    powershell -NoProfile -ExecutionPolicy Bypass -File .\setup-backup-schedule.ps1 -RunNow
#>
#Requires -RunAsAdministrator
[CmdletBinding()]
param(
    [string]$BackupFolder = 'C:\Backups\TodoAppDb',
    [string]$SqlServiceAccount = 'NT Service\MSSQL$SQLEXPRESS',
    [string]$TaskName = 'TodoAppDb daily backup',
    [string]$Time = '02:00',
    [switch]$RunNow
)

$ErrorActionPreference = 'Stop'
$sourceScript = Join-Path $PSScriptRoot 'backup-todoapp.ps1'
if (-not (Test-Path -LiteralPath $sourceScript)) {
    throw "backup-todoapp.ps1 not found next to this script ($PSScriptRoot)."
}

# 1. Backup folder.
New-Item -ItemType Directory -Path $BackupFolder -Force | Out-Null
Write-Output "Backup folder: $BackupFolder"

# 2. SQL Server service account: Modify on the folder (inherited by new .bak files).
& icacls $BackupFolder /grant "${SqlServiceAccount}:(OI)(CI)M" | Out-Null
if ($LASTEXITCODE -ne 0) { throw "icacls failed for $SqlServiceAccount" }
Write-Output "Granted Modify on the folder to $SqlServiceAccount"

# The scheduled copy writes its log next to itself: <BackupFolder>\backup.log.
$backupScript = Join-Path $BackupFolder 'backup-todoapp.ps1'
Copy-Item -LiteralPath $sourceScript -Destination $backupScript -Force
Write-Output "Installed $backupScript"

# 3. Scheduled task: daily, run whether logged on or not, run if missed, one hour limit.
$user = "$env:USERDOMAIN\$env:USERNAME"
$credential = Get-Credential -UserName $user -Message "Windows password for $user (the task runs as this account; SQL Server uses Windows authentication)"
$action = New-ScheduledTaskAction -Execute 'powershell.exe' `
    -Argument "-NoProfile -NonInteractive -ExecutionPolicy Bypass -File `"$backupScript`""
$trigger = New-ScheduledTaskTrigger -Daily -At $Time
$settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -ExecutionTimeLimit (New-TimeSpan -Hours 1) `
    -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries
Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Settings $settings `
    -User $user -Password $credential.GetNetworkCredential().Password -RunLevel Limited `
    -Description 'Daily full backup of TodoAppDb with CHECKSUM and 7-day retention (NFR-5). See ops/RESTORE.md.' `
    -Force | Out-Null
Write-Output "Scheduled task '$TaskName' registered: daily at $Time, runs $backupScript"

# 4. Optional first run.
if ($RunNow) {
    Start-ScheduledTask -TaskName $TaskName
    Write-Output "Started '$TaskName'. Check $(Join-Path $BackupFolder 'backup.log') and the .bak file in a few seconds."
}
