# PowerShell.Map

Interactive map visualization for PowerShell using Leaflet.js and OpenStreetMap. Although usable standalone, this module is primarily designed for Claude Desktop integration via the PowerShell.MCP module, enabling AI-powered map visualization.

## Features

- **Interactive Map Display**: View locations on an interactive OpenStreetMap
- **Route Visualization**: Display routes between two locations with turn-by-turn directions
- **Animated Tours**: Create animated tours that visit multiple locations sequentially
- **Geocoding**: Convert place names to coordinates using Nominatim
- **Reverse Geocoding**: Convert coordinates to place names
- **Multiple Markers**: Display multiple markers with custom colors and labels

## Installation

```powershell
Install-Module -Name PowerShell.Map
```

## Quick Start

### Display a Single Location

```powershell
Show-OpenStreetMap Tokyo
```

### Display Multiple Locations

```powershell
Show-OpenStreetMap Tokyo, Osaka, Kyoto
```

### Show a Route

```powershell
Show-OpenStreetMapRoute -From Tokyo -To Osaka
```

### Create an Animated Tour

```powershell
Start-OpenStreetMapTour Tokyo, Osaka, Kyoto, Fukuoka -Duration 2.5 -PauseTime 2
```

## Cmdlets

- **Show-OpenStreetMap**: Display one or more locations on an interactive map
- **Show-OpenStreetMapRoute**: Display a route between two locations
- **Start-OpenStreetMapTour**: Create an animated tour visiting multiple locations

## Examples

### Using Coordinates

```powershell
Show-OpenStreetMap -Location "35.6762,139.6503" -Marker "Tokyo Tower"
```

### Custom Colors and Animation

```powershell
Show-OpenStreetMap Paris -Duration 2 -Zoom 15
```

### CSV Pipeline

```powershell
Import-Csv locations.csv | Show-OpenStreetMap
```

## Claude Desktop Integration (AI-Powered)

PowerShell.Map integrates with Claude Desktop via [PowerShell.MCP](https://github.com/yotsuda/PowerShell.MCP), enabling natural language map visualization.

### Setup

1. Install PowerShell.MCP:
```powershell
Install-Module PowerShell.MCP
```

2. Configure Claude Desktop to use PowerShell.MCP (see [PowerShell.MCP documentation](https://github.com/yotsuda/PowerShell.MCP))

### Usage

Once configured, simply tell Claude to set up the module:

**"In PowerShell console, run Import-Module PowerShell.Map and check how to use it."**

Then ask for what you want:

- **"Show me a tour of famous temples in Kyoto"**
- **"Create a route from Tokyo Tower to Senso-ji Temple"**
- **"Show me a walking tour of cafes in Paris"**
- **"Plan a hot spring tour across Japan with routes between locations"**

Claude will translate these requests into PowerShell.Map commands and display the interactive maps automatically.

### Links

- [PowerShell.MCP on PowerShell Gallery](https://www.powershellgallery.com/packages/PowerShell.MCP)
- [PowerShell.MCP on GitHub](https://github.com/yotsuda/PowerShell.MCP)

## Requirements

- PowerShell 5.1 or later
- Internet connection (for OpenStreetMap tiles and geocoding)

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Links

- [GitHub Repository](https://github.com/yotsuda/PowerShell.Map)
- [Documentation](https://github.com/yotsuda/PowerShell.Map/tree/main/docs)
