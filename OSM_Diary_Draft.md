# PowerShell.Map: OSM Visualization for PowerShell

I've released PowerShell.Map - a module for displaying OpenStreetMap in your browser from PowerShell.

## What it does

Opens interactive maps in your browser from PowerShell commands:

```powershell
Show-OpenStreetMap Paris, London, Rome
Show-OpenStreetMapRoute -From Tokyo -To Osaka
Start-OpenStreetMapTour Tokyo, Kyoto, Osaka -Duration 2
```

## Technical stack

- Leaflet.js with OSM tiles
- Nominatim for geocoding
- OSRM for routing
- Built-in HTTP server for interactive display

## AI integration

This module integrates with Claude Desktop via PowerShell.MCP:
- PowerShell Gallery: https://www.powershellgallery.com/packages/PowerShell.MCP
- GitHub: https://github.com/yotsuda/PowerShell.MCP

## Installation

Requires PowerShell 7+: https://learn.microsoft.com/powershell/scripting/install/installing-powershell

```powershell
Install-Module PowerShell.Map
Import-Module PowerShell.Map
```

**Links**:
- GitHub: https://github.com/yotsuda/PowerShell.Map
- PowerShell Gallery: https://www.powershellgallery.com/packages/PowerShell.Map
- License: MIT

Feedback and contributions welcome!
