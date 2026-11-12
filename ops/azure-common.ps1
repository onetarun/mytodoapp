# Shared helpers for ops/azure-provision.ps1 and ops/azure-deploy.ps1 (Windows PowerShell 5.1+).
# Dot-source only. Error messages never include command arguments, so secrets are never printed.

function Write-Step([string]$Message) {
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Invoke-Az {
    $output = & az @args
    if ($LASTEXITCODE -ne 0) {
        throw "Azure CLI command 'az $($args[0]) $($args[1])' failed (exit code $LASTEXITCODE). See the message above."
    }
    if ($null -ne $output) { ($output -join [Environment]::NewLine).Trim() }
}

function Test-AzResource([string[]]$Arguments) {
    $previous = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        & az @Arguments -o none 2>$null | Out-Null
        return ($LASTEXITCODE -eq 0)
    }
    finally {
        $ErrorActionPreference = $previous
    }
}

function Assert-AzLoggedIn {
    if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
        throw 'Azure CLI (az) is not installed. Install it: winget install -e --id Microsoft.AzureCLI'
    }
    if (-not (Test-AzResource -Arguments @('account', 'show'))) {
        throw 'Not logged in to Azure. Run: az login'
    }
    $account = Invoke-Az account show --query '{name:name, id:id}' -o tsv
    Write-Host "Azure subscription: $account"
}

function New-RandomSecret([int]$Length) {
    # Letters and digits only: safe through az.cmd / cmd.exe quoting, and meets the
    # Azure SQL password rule (3 of: upper, lower, digit, symbol).
    $alphabet = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789'.ToCharArray()
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        do {
            $bytes = New-Object byte[] $Length
            $rng.GetBytes($bytes)
            $chars = foreach ($b in $bytes) { $alphabet[$b % $alphabet.Length] }
            $secret = -join $chars
        } until ($secret -cmatch '[A-Z]' -and $secret -cmatch '[a-z]' -and $secret -match '[0-9]')
        return $secret
    }
    finally {
        $rng.Dispose()
    }
}

function Set-AzAppSettings([string]$ResourceGroup, [string]$AppName, [System.Collections.IDictionary]$Settings) {
    # Values go through a short-lived temp file, not the command line, because connection
    # strings contain ';' and '=' that cmd.exe (az.cmd) would split.
    $items = foreach ($key in $Settings.Keys) {
        [pscustomobject]@{ name = $key; value = $Settings[$key]; slotSetting = $false }
    }
    $path = Join-Path ([IO.Path]::GetTempPath()) ("todoapp-settings-{0}.json" -f [guid]::NewGuid())
    try {
        [IO.File]::WriteAllText($path, (ConvertTo-Json -InputObject @($items)), (New-Object Text.UTF8Encoding($false)))
        Invoke-Az webapp config appsettings set -g $ResourceGroup -n $AppName --settings "@$path" -o none | Out-Null
    }
    finally {
        Remove-Item -LiteralPath $path -Force -ErrorAction SilentlyContinue
    }
}

function Get-AzAppSetting([string]$ResourceGroup, [string]$AppName, [string]$Name) {
    Invoke-Az webapp config appsettings list -g $ResourceGroup -n $AppName --query "[?name=='$Name'].value | [0]" -o tsv
}
