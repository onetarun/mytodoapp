<#
.SYNOPSIS
    Daily full backup of TodoAppDb (NFR-5): backup with CHECKSUM, verify, keep the last 7 days, log the result.

.DESCRIPTION
    1. BACKUP DATABASE to <BackupFolder>\<Database>_<yyyyMMdd_HHmm>.bak WITH INIT, CHECKSUM.
    2. RESTORE VERIFYONLY on the new file.
    3. Only after a verified backup: delete <Database>_*.bak files in <BackupFolder> older than RetentionDays.
    4. Append one line to the log file (success or failure). Exit code 0 on success, 1 on failure.

    Uses Windows authentication (sqlcmd -E): the account that runs this script needs the BACKUP DATABASE
    permission, and the SQL Server service account needs write access to BackupFolder
    (ops/setup-backup-schedule.ps1 grants it). No passwords are used or logged.

.EXAMPLE
    powershell -NoProfile -ExecutionPolicy Bypass -File ops\backup-todoapp.ps1
#>
[CmdletBinding()]
param(
    [string]$Instance = '.\SQLEXPRESS',
    [string]$Database = 'TodoAppDb',
    [string]$BackupFolder = 'C:\Backups\TodoAppDb',
    [ValidateRange(1, 365)]
    [int]$RetentionDays = 7,
    [string]$LogFile = (Join-Path $PSScriptRoot 'backup.log')
)

$ErrorActionPreference = 'Stop'

function Write-BackupLog([string]$Status, [string]$Message) {
    $line = '{0} {1} {2}' -f (Get-Date -Format 'yyyy-MM-ddTHH:mm:ssK'), $Status, $Message
    Add-Content -Path $LogFile -Value $line -Encoding utf8
    Write-Output $line
}

# Runs one T-SQL batch; -b makes sqlcmd return a non-zero exit code on any SQL error.
function Invoke-Sql([string]$Query) {
    $output = & sqlcmd -S $Instance -E -C -b -h -1 -W -Q $Query 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw (($output | Out-String).Trim() -replace '\s+', ' ')
    }
}

try {
    if (-not (Test-Path -LiteralPath $BackupFolder -PathType Container)) {
        throw "Backup folder '$BackupFolder' does not exist. Run ops\setup-backup-schedule.ps1 first."
    }

    $fileName = '{0}_{1}.bak' -f $Database, (Get-Date -Format 'yyyyMMdd_HHmm')
    $filePath = Join-Path $BackupFolder $fileName
    # Names come from the parameters above, never from user input; ']' and "'" are escaped anyway.
    $db = $Database.Replace(']', ']]')
    $disk = $filePath.Replace("'", "''")

    Invoke-Sql "BACKUP DATABASE [$db] TO DISK = N'$disk' WITH INIT, CHECKSUM, NAME = N'$db daily full backup';"
    Invoke-Sql "RESTORE VERIFYONLY FROM DISK = N'$disk' WITH CHECKSUM;"

    $sizeMb = [math]::Round((Get-Item -LiteralPath $filePath).Length / 1MB, 2)

    # Retention: only this database's .bak files in the backup folder, and only after a verified backup.
    $cutoff = (Get-Date).AddDays(-$RetentionDays)
    $old = Get-ChildItem -LiteralPath $BackupFolder -Filter "$Database`_*.bak" -File |
        Where-Object { $_.LastWriteTime -lt $cutoff }
    $old | Remove-Item -Force
    $removed = @($old).Count
    $removedNames = if ($removed -gt 0) { ' (' + (($old | ForEach-Object Name) -join ', ') + ')' } else { '' }

    Write-BackupLog 'OK' "$fileName $sizeMb MB, verified; removed $removed backup(s) older than $RetentionDays days$removedNames"
    exit 0
}
catch {
    Write-BackupLog 'FAILED' $_.Exception.Message
    exit 1
}
