<#
.SYNOPSIS
    Build PowerShell help files

.DESCRIPTION
    Generates MAML help files from markdown documentation using PlatyPS.
    Output: PlatyPS\xml-US\PowerShell.Map.dll-Help.xml
    Copies to: bin output and module directory

.EXAMPLE
    .\Build-Help.ps1
    Builds and deploys help files

.NOTES
    Requires: platyPS module (Install-Module -Name platyPS)
#>

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$scriptRoot = $PSScriptRoot
$mdPath = Join-Path $scriptRoot "en-US"
$xmlOutputPath = Join-Path $scriptRoot "xml-US"
$projectRoot = Split-Path $scriptRoot -Parent
$binPath = Join-Path $projectRoot "bin"
$moduleInstallPath = "C:\Program Files\PowerShell\7\Modules\PowerShell.Map"
$helpFileName = "PowerShell.Map.dll-Help.xml"
$sourceHelpFile = Join-Path $xmlOutputPath $helpFileName

Write-Host "=== PowerShell.Map Help Build ===" -ForegroundColor Cyan

# Check if markdown files exist
if (-not (Test-Path $mdPath)) {
    Write-Error "Markdown directory not found: $mdPath"
    exit 1
}

$mdFiles = Get-ChildItem -Path $mdPath -Filter "*.md" | Where-Object { $_.Name -ne 'README.md' }
if ($mdFiles.Count -eq 0) {
    Write-Error "No markdown files found in: $mdPath"
    exit 1
}

Write-Host "[OK] Found $($mdFiles.Count) markdown files" -ForegroundColor Green

# Check for platyPS module
if (-not (Get-Module -Name platyPS -ListAvailable)) {
    Write-Error "platyPS module not found. Install it with: Install-Module -Name platyPS"
    exit 1
}

Import-Module platyPS -ErrorAction Stop

# Generate help XML
Write-Host "`nBuilding help file..." -ForegroundColor Cyan

try {
    # Create output directory
    if (-not (Test-Path $xmlOutputPath)) {
        New-Item -Path $xmlOutputPath -ItemType Directory -Force | Out-Null
    }
    
    # Generate help XML to xml-US directory
    New-ExternalHelp -Path $mdPath -OutputPath $xmlOutputPath -Force -ErrorAction SilentlyContinue | Out-Null
    
    if (-not (Test-Path $sourceHelpFile)) {
        Write-Error "Help file was not created: $sourceHelpFile"
        exit 1
    }
    
    $xmlFile = Get-Item $sourceHelpFile
    Write-Host "[OK] Help file generated: $helpFileName" -ForegroundColor Green
    Write-Host "     Output: PlatyPS\xml-US\$helpFileName" -ForegroundColor Gray
    Write-Host "     Size: $([math]::Round($xmlFile.Length / 1KB, 2)) KB" -ForegroundColor Gray
} catch {
    Write-Error "Failed to build help file: $_"
    exit 1
}

# Copy to bin directories
Write-Host "`nCopying to build output..." -ForegroundColor Cyan
$copiedCount = 0

foreach ($config in @("Debug", "Release")) {
    $targetPath = Join-Path $binPath "$config\net9.0\en-US"
    
    if (-not (Test-Path $targetPath)) {
        New-Item -Path $targetPath -ItemType Directory -Force | Out-Null
    }
    
    $destination = Join-Path $targetPath $helpFileName
    Copy-Item -Path $sourceHelpFile -Destination $destination -Force
    $copiedCount++
    Write-Host "  [COPY] $config/net9.0/en-US/$helpFileName" -ForegroundColor Gray
}

# Deploy help file to PowerShell modules directory
Write-Host "`nDeploying to module directory..." -ForegroundColor Cyan

$moduleHelpPath = Join-Path $moduleInstallPath "en-US"
if (-not (Test-Path $moduleHelpPath)) {
    New-Item -Path $moduleHelpPath -ItemType Directory -Force | Out-Null
}

$destinationHelpFile = Join-Path $moduleHelpPath $helpFileName
Copy-Item -Path $sourceHelpFile -Destination $destinationHelpFile -Force
Write-Host "[OK] Help file deployed to: $moduleHelpPath" -ForegroundColor Green

Write-Host ""
Write-Host "=== Build Complete ===" -ForegroundColor Green
Write-Host "Source: PlatyPS\xml-US\$helpFileName" -ForegroundColor Gray
Write-Host "Copied to $copiedCount build locations" -ForegroundColor Gray
Write-Host "Deployed to: $moduleInstallPath\en-US\" -ForegroundColor Gray
