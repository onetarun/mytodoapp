<#
.SYNOPSIS
    One-time creation of the Azure resources for the Todo Application (ARCHITECTURE §13, AD-6).

.DESCRIPTION
    Creates a resource group, a Linux App Service plan + web app (.NET 10), an Azure SQL server
    and database, firewall rules and the app settings the API needs. The SQL admin password and
    the JWT signing key are generated here, stored only in the App Service settings, and never
    printed or written to the repository (NFR-31, G-SEC-1).

    Run `az login` first. Afterwards deploy the code with ops/azure-deploy.ps1.

.PARAMETER AppName
    Globally unique web app name (3-50 chars, lowercase letters, digits, hyphens), e.g. mytodoapp-tarun.

.PARAMETER UseFreeTier
    App Service F1 + the Azure SQL free offer (one per subscription) instead of B1 + Basic.

.EXAMPLE
    .\ops\azure-provision.ps1 -AppName mytodoapp-tarun
    .\ops\azure-provision.ps1 -AppName mytodoapp-tarun -UseFreeTier -Location southindia
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[a-z0-9][a-z0-9-]{1,48}[a-z0-9]$')]
    [string]$AppName,

    [string]$ResourceGroup = 'rg-todoapp',

    [string]$Location = 'centralindia',

    [switch]$UseFreeTier,

    [string]$DatabaseName = 'TodoAppDb',

    [string]$SqlAdminUser = 'todoappadmin'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'azure-common.ps1')

$sqlServerName = "$AppName-sql"
$planName = "$AppName-plan"
$appSku = if ($UseFreeTier) { 'F1' } else { 'B1' }
$expectedBackupRetentionDays = 7

Assert-AzLoggedIn

if (Test-AzResource -Arguments @('sql', 'server', 'show', '-g', $ResourceGroup, '-n', $sqlServerName)) {
    throw "SQL server '$sqlServerName' already exists. Provisioning runs only once; use ops/azure-deploy.ps1 to deploy."
}

Write-Step "Resource group $ResourceGroup ($Location)"
Invoke-Az group create -n $ResourceGroup -l $Location -o none

Write-Step "App Service plan $planName (Linux, $appSku)"
Invoke-Az appservice plan create -g $ResourceGroup -n $planName -l $Location --is-linux --sku $appSku -o none

Write-Step "Web app $AppName (.NET 10)"
Invoke-Az webapp create -g $ResourceGroup -p $planName -n $AppName --runtime 'DOTNETCORE:10.0' -o none
Invoke-Az webapp update -g $ResourceGroup -n $AppName --https-only true -o none
$configArgs = @('webapp', 'config', 'set', '-g', $ResourceGroup, '-n', $AppName,
    '--min-tls-version', '1.2', '--ftps-state', 'Disabled', '--startup-file', 'dotnet TodoApp.Api.dll', '-o', 'none')
if (-not $UseFreeTier) { $configArgs += @('--always-on', 'true') }
Invoke-Az @configArgs
$hostName = Invoke-Az webapp show -g $ResourceGroup -n $AppName --query defaultHostName -o tsv

Write-Step "Azure SQL server $sqlServerName"
$sqlPassword = New-RandomSecret -Length 32
Invoke-Az sql server create -g $ResourceGroup -n $sqlServerName -l $Location -u $SqlAdminUser -p $sqlPassword --minimal-tls-version 1.2 -o none
Invoke-Az sql server firewall-rule create -g $ResourceGroup -s $sqlServerName -n AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0 -o none

Write-Step "Azure SQL database $DatabaseName"
if ($UseFreeTier) {
    Invoke-Az sql db create -g $ResourceGroup -s $sqlServerName -n $DatabaseName -e GeneralPurpose -f Gen5 -c 2 `
        --compute-model Serverless --use-free-limit --free-limit-exhaustion-behavior AutoPause --backup-storage-redundancy Local -o none
}
else {
    Invoke-Az sql db create -g $ResourceGroup -s $sqlServerName -n $DatabaseName --service-objective Basic --backup-storage-redundancy Local -o none
}

$retention = Invoke-Az sql db str-policy show -g $ResourceGroup -s $sqlServerName -n $DatabaseName --query retentionDays -o tsv
if ([int]$retention -ne $expectedBackupRetentionDays) {
    Write-Warning "Point-in-time backup retention is $retention days (NFR-5a expects $expectedBackupRetentionDays)."
}

Write-Step 'App settings (secrets are not shown)'
$connectionString = "Server=tcp:$sqlServerName.database.windows.net,1433;Initial Catalog=$DatabaseName;User ID=$SqlAdminUser;Password=$sqlPassword;Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;"
$settings = [ordered]@{
    'ASPNETCORE_ENVIRONMENT'               = 'Production'
    'ASPNETCORE_FORWARDEDHEADERS_ENABLED'  = 'true'
    'SCM_DO_BUILD_DURING_DEPLOYMENT'       = 'false'
    'ConnectionStrings__DefaultConnection' = $connectionString
    'Jwt__Key'                             = (New-RandomSecret -Length 64)
    'Cors__AllowedOrigins__0'              = "https://$hostName"
    'Serilog__WriteTo__0__Args__path'      = '/home/LogFiles/todoapp-.log'
}
Set-AzAppSettings -ResourceGroup $ResourceGroup -AppName $AppName -Settings $settings

$sqlPassword = $null
$connectionString = $null
$settings = $null

Write-Host ''
Write-Host "Done. App URL: https://$hostName" -ForegroundColor Green
Write-Host "Next: .\ops\azure-deploy.ps1 -AppName $AppName -ResourceGroup $ResourceGroup"
