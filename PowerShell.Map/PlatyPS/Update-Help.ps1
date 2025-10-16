<#
.SYNOPSIS
    Update PlatyPS markdown help files for PowerShell.Map module

.DESCRIPTION
    Updates markdown documentation from the currently loaded PowerShell.Map module.
    Run this after making changes to cmdlet parameters or adding new cmdlets.

    Input: Currently loaded PowerShell.Map module
    Output: Updated markdown files in PlatyPS\en-US\

    NOTE: This script updates SYNTAX and PARAMETERS sections automatically,
    but EXAMPLES section must be updated manually in the markdown files.

.EXAMPLE
    Import-Module PowerShell.Map -Force
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
Write-Host ""

# Check if module is loaded
$module = Get-Module PowerShell.Map
if (-not $module) {
    Write-Warning "PowerShell.Map module is not loaded."
    Write-Host ""
    Write-Host "Please import the module first:" -ForegroundColor Yellow
    Write-Host "  Import-Module PowerShell.Map -Force" -ForegroundColor Gray
    Write-Host ""
    exit 1
}

Write-Host "[✓] PowerShell.Map module loaded (v$($module.Version))" -ForegroundColor Green

# Show loaded cmdlets
$cmdlets = Get-Command -Module PowerShell.Map
Write-Host "[✓] Cmdlets found: $($cmdlets.Count)" -ForegroundColor Green
foreach ($cmdlet in $cmdlets) {
    Write-Host "    - $($cmdlet.Name)" -ForegroundColor Gray
}
Write-Host ""

# Import PlatyPS
if (-not (Get-Module -Name PlatyPS -ListAvailable)) {
    Write-Error "PlatyPS module not found. Install it with: Install-Module -Name PlatyPS"
    exit 1
}

Import-Module PlatyPS -ErrorAction Stop
Write-Host "[✓] PlatyPS module loaded" -ForegroundColor Green
Write-Host ""

# Update markdown help
Write-Host "Updating markdown files in:" -ForegroundColor Yellow
Write-Host "  $markdownPath" -ForegroundColor Gray
Write-Host ""

$result = Update-MarkdownHelpModule -Path $markdownPath -RefreshModulePage -ErrorAction Stop

# Show updated files with timestamps
Write-Host "[✓] Updated files:" -ForegroundColor Green
$mdFiles = Get-ChildItem $markdownPath\*.md | Where-Object { $_.Name -ne 'README.md' }
foreach ($file in $mdFiles) {
    $lastWrite = $file.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
    Write-Host "    [$lastWrite] $($file.Name)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "=== Update Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "IMPORTANT: Remember to manually update EXAMPLES section if needed!" -ForegroundColor Yellow
Write-Host "Next step: Run Build-Help.ps1 to rebuild the XML help file" -ForegroundColor Cyan
Write-Host ""
