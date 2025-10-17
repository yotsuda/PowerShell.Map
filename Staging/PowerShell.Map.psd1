@{
    RootModule = 'PowerShell.Map.psm1'
    ModuleVersion = '0.1.0'
    GUID = 'b8e7c4d1-3f2a-4a9b-8c5d-1e2f3a4b5c6d'
    Author = 'Yoshifumi Tsuda'
    CompanyName = ''
    Copyright = '(c) 2025. All rights reserved.'
    Description = 'Interactive map visualization for PowerShell using Leaflet.js and OpenStreetMap'
    PowerShellVersion = '5.1'
    DotNetFrameworkVersion = '4.7.2'

    FormatsToProcess = @('PowerShell.Map.Format.ps1xml')
    CLRVersion = '4.0'

    FunctionsToExport = @()
    CmdletsToExport = @(
'Show-OpenStreetMap',
'Show-OpenStreetMapRoute',
'Start-OpenStreetMapTour'
)

    VariablesToExport = @()
    AliasesToExport = @()

    HelpInfoURI = 'https://github.com/yotsuda/PowerShell.Map/tree/main/docs'

    PrivateData = @{
        PSData = @{
            Tags = @('Map', 'Visualization', 'GIS', 'Leaflet', 'OpenStreetMap', 'Route')
            LicenseUri = 'https://github.com/yotsuda/PowerShell.Map/blob/master/LICENSE'
            ProjectUri = 'https://github.com/yotsuda/PowerShell.Map'
            ReleaseNotes = 'v0.1.0: Initial release with Show-OpenStreetMap, Show-OpenStreetMapRoute, and Start-OpenStreetMapTour. Interactive maps with smooth animations and route visualization.'
        }
    }
}