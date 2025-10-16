<#
.SYNOPSIS
    Update PlatyPS markdown help files for PowerShell.Map module

.DESCRIPTION
    Updates markdown documentation from the currently loaded PowerShell.Map module.
    Run this after making changes to cmdlet parameters or adding new cmdlets.
    
    Input: Currently loaded PowerShell.Map module
    Output: Updated markdown files in PlatyPS\en-US\

.EXAMPLE
    Import-Module .\bin\Release\net9.0\PowerShell.Map.psd1 -Force
    .\Update-Help.ps1
    Updates all markdown help files

.NOTES
    Requires: PlatyPS module (Install-Module -Name PlatyPS)
#>

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$scriptRoot = $PSScriptRoot
$markdownPath = Join-Path $scriptRoot "en-US"

Write-Host "=== Updating PowerShell.Map Help ===" -ForegroundColor Cyan

# Check if module is loaded
$module = Get-Module PowerShell.Map
if (-not $module) {
    Write-Warning "PowerShell.Map module is not loaded."
    Write-Host "Please import the module first:" -ForegroundColor Yellow
    Write-Host "  Import-Module .\bin\Release\net9.0\PowerShell.Map.psd1 -Force" -ForegroundColor Gray
    exit 1
}

Write-Host "[OK] PowerShell.Map module loaded (v$($module.Version))" -ForegroundColor Green

# Import PlatyPS
if (-not (Get-Module -Name PlatyPS -ListAvailable)) {
    Write-Error "PlatyPS module not found. Install it with: Install-Module -Name PlatyPS"
    exit 1
}

Import-Module PlatyPS -ErrorAction Stop
Write-Host "[OK] PlatyPS module loaded" -ForegroundColor Green

# Update markdown help
Write-Host "Updating markdown files in: $markdownPath" -ForegroundColor Yellow
Update-MarkdownHelpModule -Path $markdownPath -RefreshModulePage -ErrorAction Stop

# Show updated files
Write-Host "`n[OK] Updated files:" -ForegroundColor Green
Get-ChildItem $markdownPath\*.md | Where-Object { $_.Name -ne 'README.md' } | ForEach-Object {
    Write-Host "  - $($_.Name)" -ForegroundColor Gray
}

Write-Host "`n=== Update Complete ===" -ForegroundColor Green
Write-Host "Run Build-Help.ps1 to rebuild the XML help file" -ForegroundColor Yellow
