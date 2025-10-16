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
    Requires: PlatyPS module (Install-Module -Name PlatyPS)
#>

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$scriptRoot = $PSScriptRoot
$mdPath = Join-Path $scriptRoot "en-US"
$xmlOutputPath = Join-Path $scriptRoot "xml-US"
$projectRoot = Split-Path $scriptRoot -Parent
$binPath = Join-Path $projectRoot "bin"
$helpFileName = "PowerShell.Map.dll-Help.xml"
$sourceHelpFile = Join-Path $xmlOutputPath $helpFileName

Write-Host "=== PowerShell.Map Help Build ===" -ForegroundColor Cyan
Write-Host ""

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

Write-Host "[✓] Found $($mdFiles.Count) markdown files:" -ForegroundColor Green
foreach ($file in $mdFiles) {
    Write-Host "    - $($file.Name)" -ForegroundColor Gray
}
Write-Host ""

# Check for PlatyPS module
if (-not (Get-Module -Name PlatyPS -ListAvailable)) {
    Write-Error "PlatyPS module not found. Install it with: Install-Module -Name PlatyPS"
    exit 1
}

Import-Module PlatyPS -ErrorAction Stop
Write-Host "[✓] PlatyPS module loaded" -ForegroundColor Green
Write-Host ""

# Generate help XML
Write-Host "Building help file..." -ForegroundColor Yellow

try {
    # Create output directory
    if (-not (Test-Path $xmlOutputPath)) {
        New-Item -Path $xmlOutputPath -ItemType Directory -Force | Out-Null
    }
    
    # Generate help XML to xml-US directory
    New-ExternalHelp -Path $mdPath -OutputPath $xmlOutputPath -Force -ErrorAction Stop | Out-Null
    
    if (-not (Test-Path $sourceHelpFile)) {
        Write-Error "Help file was not created: $sourceHelpFile"
        exit 1
    }
    
    $xmlFile = Get-Item $sourceHelpFile
    Write-Host "[✓] Help file generated:" -ForegroundColor Green
    Write-Host "    File: $helpFileName" -ForegroundColor Gray
    Write-Host "    Path: PlatyPS\xml-US\" -ForegroundColor Gray
    Write-Host "    Size: $([math]::Round($xmlFile.Length / 1KB, 2)) KB" -ForegroundColor Gray
    Write-Host ""
} catch {
    Write-Error "Failed to build help file: $_"
    exit 1
}

# Copy to bin directories
Write-Host "Copying to build outputs..." -ForegroundColor Yellow
$copiedCount = 0

foreach ($config in @("Debug", "Release")) {
    $targetPath = Join-Path $binPath "$config\net9.0\en-US"
    
    if (Test-Path (Split-Path $targetPath -Parent)) {
        if (-not (Test-Path $targetPath)) {
            New-Item -Path $targetPath -ItemType Directory -Force | Out-Null
        }
        
        $destination = Join-Path $targetPath $helpFileName
        Copy-Item -Path $sourceHelpFile -Destination $destination -Force
        $copiedCount++
        Write-Host "    [✓] $config/net9.0/en-US/$helpFileName" -ForegroundColor Gray
    } else {
        Write-Host "    [SKIP] $config build not found" -ForegroundColor DarkGray
    }
}

if ($copiedCount -eq 0) {
    Write-Warning "No build outputs found. Build the project first."
}
Write-Host ""

# Deploy help file to PowerShell modules directory (if module is installed)
Write-Host "Deploying to installed module..." -ForegroundColor Yellow

$module = Get-Module -Name PowerShell.Map -ListAvailable | Select-Object -First 1
if ($module) {
    $moduleBasePath = Split-Path $module.Path -Parent
    $moduleHelpPath = Join-Path $moduleBasePath "en-US"
    
    if (-not (Test-Path $moduleHelpPath)) {
        New-Item -Path $moduleHelpPath -ItemType Directory -Force | Out-Null
    }

    $destinationHelpFile = Join-Path $moduleHelpPath $helpFileName
    Copy-Item -Path $sourceHelpFile -Destination $destinationHelpFile -Force
    Write-Host "    [✓] Deployed to: $moduleHelpPath" -ForegroundColor Gray
} else {
    Write-Host "    [SKIP] PowerShell.Map module not installed" -ForegroundColor DarkGray
}

Write-Host ""
Write-Host "=== Build Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  - Source: PlatyPS\xml-US\$helpFileName" -ForegroundColor Gray
Write-Host "  - Copied to $copiedCount build location(s)" -ForegroundColor Gray
if ($module) {
    Write-Host "  - Deployed to installed module" -ForegroundColor Gray
}
Write-Host ""
Write-Host "Next step: Import the module to test the help" -ForegroundColor Cyan
Write-Host "  Import-Module PowerShell.Map -Force" -ForegroundColor Gray
Write-Host "  Get-Help Show-OpenStreetMap -Full" -ForegroundColor Gray
Write-Host ""
