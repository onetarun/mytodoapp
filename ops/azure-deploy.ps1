<#
.SYNOPSIS
    Builds and deploys the Todo Application to the Azure web app created by ops/azure-provision.ps1.

.DESCRIPTION
    1. Angular production build (frontend/todo-app).
    2. dotnet publish of TodoApp.Api, with the Angular build copied into wwwroot (AD-6).
    3. EF Core migrations applied to Azure SQL (NFR-30). This machine's public IP is allowed on the
       SQL firewall only while the migration runs. Secrets are read from the App Service settings
       into this process's environment and removed afterwards; they are never printed.
    4. Zip deploy to App Service, then a smoke check.

    Run `az login` first. Run from any folder; paths are relative to this script.

.EXAMPLE
    .\ops\azure-deploy.ps1 -AppName mytodoapp-tarun
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[a-z0-9][a-z0-9-]{1,48}[a-z0-9]$')]
    [string]$AppName,

    [string]$ResourceGroup = 'rg-todoapp',

    [switch]$SkipMigrations
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'azure-common.ps1')

$repoRoot = Split-Path -Parent $PSScriptRoot
$apiProject = Join-Path $repoRoot 'backend\TodoApp.Api\TodoApp.Api.csproj'
$frontendDir = Join-Path $repoRoot 'frontend\todo-app'
$angularOutput = Join-Path $frontendDir 'dist\todo-app\browser'
$sqlServerName = "$AppName-sql"
$firewallRuleName = 'deploy-client'
$workDir = Join-Path ([IO.Path]::GetTempPath()) ("todoapp-deploy-{0:yyyyMMddHHmmss}" -f (Get-Date))
$publishDir = Join-Path $workDir 'publish'
$zipPath = Join-Path $workDir 'todoapp.zip'

function Invoke-Native([string]$Description, [scriptblock]$Command) {
    & $Command
    if ($LASTEXITCODE -ne 0) { throw "$Description failed (exit code $LASTEXITCODE)." }
}

function Get-HttpStatus([string]$Url, [string]$Method = 'GET', [string]$Body = $null) {
    try {
        $params = @{ Uri = $Url; Method = $Method; UseBasicParsing = $true; TimeoutSec = 120 }
        if ($Body) { $params.Body = $Body; $params.ContentType = 'application/json' }
        return [int](Invoke-WebRequest @params).StatusCode
    }
    catch {
        # 4xx/5xx: WebException in Windows PowerShell 5.1, HttpResponseException in PowerShell 7.
        # Both expose the status on Exception.Response; no response means a network error.
        if (-not ($_.Exception.PSObject.Properties['Response']) -or $null -eq $_.Exception.Response) { throw }
        return [int]$_.Exception.Response.StatusCode
    }
}

foreach ($tool in 'dotnet', 'npm') {
    if (-not (Get-Command $tool -ErrorAction SilentlyContinue)) { throw "'$tool' is not installed (see README §1)." }
}
Assert-AzLoggedIn
$hostName = Invoke-Az webapp show -g $ResourceGroup -n $AppName --query defaultHostName -o tsv

New-Item -ItemType Directory -Path $workDir | Out-Null
try {
    Write-Step 'Angular production build'
    Push-Location $frontendDir
    try {
        Invoke-Native 'npm ci' { npm ci --no-audit --no-fund }
        Invoke-Native 'npm run build' { npm run build }
    }
    finally {
        Pop-Location
    }
    if (-not (Test-Path (Join-Path $angularOutput 'index.html'))) { throw "Angular build output not found at $angularOutput." }

    Write-Step 'dotnet publish (Release)'
    Invoke-Native 'dotnet publish' { dotnet publish $apiProject -c Release -o $publishDir --nologo }
    $wwwroot = Join-Path $publishDir 'wwwroot'
    New-Item -ItemType Directory -Path $wwwroot -Force | Out-Null
    Copy-Item -Path (Join-Path $angularOutput '*') -Destination $wwwroot -Recurse -Force

    if (-not $SkipMigrations) {
        Write-Step 'EF Core migrations on Azure SQL'
        $clientIp = (Invoke-RestMethod -Uri 'https://api.ipify.org' -UseBasicParsing -TimeoutSec 30).ToString().Trim()
        Invoke-Az sql server firewall-rule create -g $ResourceGroup -s $sqlServerName -n $firewallRuleName `
            --start-ip-address $clientIp --end-ip-address $clientIp -o none | Out-Null
        try {
            $env:ConnectionStrings__DefaultConnection = Get-AzAppSetting $ResourceGroup $AppName 'ConnectionStrings__DefaultConnection'
            $env:Jwt__Key = Get-AzAppSetting $ResourceGroup $AppName 'Jwt__Key'
            if (-not $env:ConnectionStrings__DefaultConnection -or -not $env:Jwt__Key) {
                throw 'App settings are missing. Run ops/azure-provision.ps1 first.'
            }
            Invoke-Native 'dotnet ef database update' { dotnet ef database update --project $apiProject --no-build --configuration Release }
        }
        finally {
            Remove-Item Env:\ConnectionStrings__DefaultConnection -ErrorAction SilentlyContinue
            Remove-Item Env:\Jwt__Key -ErrorAction SilentlyContinue
            Invoke-Az sql server firewall-rule delete -g $ResourceGroup -s $sqlServerName -n $firewallRuleName -o none | Out-Null
        }
    }

    Write-Step 'Zip deploy to App Service'
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [IO.Compression.ZipFile]::CreateFromDirectory($publishDir, $zipPath)
    Invoke-Az webapp deploy -g $ResourceGroup -n $AppName --src-path $zipPath --type zip -o none | Out-Null

    Write-Step 'Smoke check'
    $baseUrl = "https://$hostName"
    $rootStatus = Get-HttpStatus "$baseUrl/"
    $deepLink = Get-HttpStatus "$baseUrl/tasks"
    $login = Get-HttpStatus "$baseUrl/api/v1/auth/login" 'POST' '{}'
    Write-Host "GET / -> $rootStatus, GET /tasks -> $deepLink, POST /api/v1/auth/login {} -> $login (expected 200, 200, 400)"
    if ($rootStatus -ne 200 -or $deepLink -ne 200 -or $login -ne 400) {
        throw "Smoke check failed. Logs: az webapp log tail -g $ResourceGroup -n $AppName"
    }

    Write-Host ''
    Write-Host "Deployed: $baseUrl" -ForegroundColor Green
}
finally {
    Remove-Item -LiteralPath $workDir -Recurse -Force -ErrorAction SilentlyContinue
}
