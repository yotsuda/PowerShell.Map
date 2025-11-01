# PowerShell.Map

Interactive 2D/3D map visualization for PowerShell using MapLibre GL JS and OpenStreetMap. Although usable standalone, this module is primarily designed for Claude Desktop integration via the PowerShell.MCP module, enabling AI-powered map visualization.

## What's New in v1.0.1

Two major features have been added since v1.0.0:

- **🎮 3D Map Visualization** - Interactive 3D terrain and building views with dynamic camera controls (Pitch, Bearing)
- **📝 Rich Marker Descriptions** - Click markers to display detailed multi-line information, perfect for AI-powered context delivery

Additional improvements:
- Enhanced animation with configurable duration
- Stateful design - camera position and 3D mode preserved across commands
- Upgraded from Leaflet.js to MapLibre GL JS for better 3D performance

## Features

- **Interactive 2D/3D Map Display**: View locations on OpenStreetMap with optional 3D terrain and buildings
- **Rich Marker Information**: Click markers to display detailed descriptions with multi-line text and emoji support
- **Route Visualization**: Display routes between two locations with turn-by-turn directions
- **Animated Tours**: Create animated tours that visit multiple locations sequentially
- **Geocoding**: Convert place names to coordinates using Nominatim
- **Reverse Geocoding**: Convert coordinates to place names
- **Multiple Markers**: Display multiple markers with custom colors, labels, and descriptions
- **Stateful Camera**: Pitch, bearing, and 3D mode preserved across commands
- **CSV Pipeline Support**: Import location data from CSV files

## Installation

```powershell
Install-Module -Name PowerShell.Map
```

## Quick Start

### Display a Single Location

```powershell
Show-OpenStreetMap Tokyo
```

### Display with 3D Terrain

```powershell
Show-OpenStreetMap "Mount Fuji" -Enable3D -Zoom 11 -Pitch 60
```

### Display with Description (NEW in v1.0.1)

```powershell
Show-OpenStreetMap "Eiffel Tower" -Description "🗼 Eiffel Tower`nHeight: 330m`nBuilt: 1889`nEntry: €28"
```

### Display Multiple Locations

```powershell
Show-OpenStreetMap Paris, London, Berlin
```

### Show a Route

```powershell
Show-OpenStreetMapRoute -From Paris -To London
```

### Create an Animated Tour

```powershell
Start-OpenStreetMapTour Paris, London, Berlin, Amsterdam -Duration 2.5 -PauseTime 5
```

## Core Cmdlets

- **Show-OpenStreetMap**: Display one or more locations on an interactive 2D/3D map
- **Show-OpenStreetMapRoute**: Display a route between two locations
- **Start-OpenStreetMapTour**: Create an animated tour visiting multiple locations

## Examples

### Using Coordinates

```powershell
Show-OpenStreetMap -Location "48.8584,2.2945" -Zoom 15
```

### 3D Visualization (NEW in v1.0.1)

```powershell
# Mount Everest with 3D terrain
Show-OpenStreetMap "27.9881,86.9250" -Enable3D -Zoom 11 -Pitch 70 -Bearing 45

# New York cityscape with 3D buildings
Show-OpenStreetMap "Empire State Building" -Enable3D -Zoom 16 -Pitch 60
```

### Structured Locations with Rich Descriptions (NEW in v1.0.1)

```powershell
$landmarks = @(
    @{ 
        Location = "Eiffel Tower"
        Description = "🗼 Eiffel Tower`nHeight: 330m (1,083 ft)`nBuilt: 1889`nEntry: €28 summit`nHours: 9:00-00:45`nBest: Sunset or night"
        Label = "Tour Eiffel"
        Color = "red"
    },
    @{ 
        Location = "Louvre Museum"
        Description = "🎨 Louvre Museum`nWorld's largest art museum`nOpened: 1793`nEntry: €17`nHours: 9:00-18:00`nClosed: Tuesdays"
        Label = "Musée du Louvre"
        Color = "blue"
    },
    @{
        Location = "Arc de Triomphe"
        Description = "🏛️ Arc de Triomphe`nHeight: 50m`nBuilt: 1836`nEntry: €13`nHours: 10:00-23:00`nBest: Sunset from top"
        Label = "Arc de Triomphe"
        Color = "orange"
    }
)
Show-OpenStreetMap -Locations $landmarks -Enable3D -Zoom 13 -Pitch 50
```

### Historical Tour Example with Descriptions

```powershell
# WWII D-Day landing beaches with detailed information
$beaches = @(
    @{ Location = "Utah Beach, France"; Label = "Utah Beach"; Color = "green"
       Description = "🏖️ Utah Beach`nLanding: June 6, 1944, 6:30 AM`nForces: US 4th Infantry Division`nCasualties: ~200`nSuccess: Lightest casualties of D-Day" },
    @{ Location = "Omaha Beach, France"; Label = "Omaha Beach"; Color = "red"
       Description = "🏖️ Omaha Beach`nLanding: June 6, 1944, 6:30 AM`nForces: US 1st & 29th Infantry`nCasualties: ~2,000`nDifficulty: Bloodiest landing" },
    @{ Location = "Gold Beach, France"; Label = "Gold Beach"; Color = "gold"
       Description = "🏖️ Gold Beach`nLanding: June 6, 1944, 7:25 AM`nForces: British 50th Infantry`nCasualties: ~1,000`nObjective: Link with Omaha" },
    @{ Location = "Juno Beach, France"; Label = "Juno Beach"; Color = "blue"
       Description = "🏖️ Juno Beach`nLanding: June 6, 1944, 7:45 AM`nForces: Canadian 3rd Infantry`nCasualties: ~1,000`nProgress: Deepest advance of D-Day" },
    @{ Location = "Sword Beach, France"; Label = "Sword Beach"; Color = "navy"
       Description = "🏖️ Sword Beach`nLanding: June 6, 1944, 7:25 AM`nForces: British 3rd Infantry`nCasualties: ~700`nObjective: Link with British airborne" }
)

Show-OpenStreetMap -Locations $beaches -Enable3D -Zoom 10 -Pitch 45
Start-OpenStreetMapTour -Locations $beaches -Duration 2.0 -PauseTime 8 -Enable3D -Pitch 50
```

### Custom Colors and Animation

```powershell
Show-OpenStreetMap "Big Ben" -Duration 2 -Zoom 15
```

### CSV Pipeline

```powershell
# CSV format: Location,Description,Color,Label
Import-Csv locations.csv | Show-OpenStreetMap
```

### Stateful Camera Control

```powershell
# Set 3D view once
Show-OpenStreetMap "Grand Canyon" -Enable3D -Pitch 60 -Bearing 45

# Subsequent commands preserve 3D state
Show-OpenStreetMap "Yosemite"  # Still in 3D mode with Pitch 60

# Reset to top-down view
Show-OpenStreetMap "Yellowstone" -Pitch 0 -Bearing 0

# Disable 3D explicitly
Show-OpenStreetMap "Death Valley" -Disable3D
```

## 3D Features

### 3D Terrain
- **Data Source**: AWS Terrarium RGB tiles
- **Coverage**: Global elevation data
- **Exaggeration**: Dynamic scaling from 0.3x (urban) to 2.0x (mountains)
- **Best Use**: Mountain ranges, valleys, coastal areas

### 3D Buildings
- **Availability**: Zoom level 14+ in supported areas
- **Data Source**: OpenStreetMap building footprints with height data
- **Best Use**: City centers, downtown areas, skyscrapers

### Camera Controls
- **Pitch**: 0° (top-down) to 85° (almost horizontal)
- **Bearing**: 0-360° (North=0, East=90, South=180, West=270)
- **Duration**: 0-10 seconds for smooth transitions
- **Browser Controls**: 3D toggle button and camera reset available in UI

## Claude Desktop Integration (AI-Powered)

PowerShell.Map integrates with Claude Desktop via [PowerShell.MCP](https://github.com/yotsuda/PowerShell.MCP), enabling natural language map visualization.

### Setup

1. Install PowerShell.MCP:
```powershell
Install-Module PowerShell.MCP
```

2. Configure Claude Desktop to use PowerShell.MCP (see [PowerShell.MCP documentation](https://github.com/yotsuda/PowerShell.MCP))

### Usage

Once configured, simply ask Claude:

- **"Show me a 3D tour of famous landmarks in Paris with detailed information"**
- **"Create a route from Big Ben to Buckingham Palace"**
- **"Display the Alps mountain range in 3D"**
- **"Show me the Normandy D-Day beaches with historical details for each"**
- **"Plan a tour of Italian Renaissance cities with descriptions"**

Claude will automatically:
- Research location details
- Create rich descriptions with relevant information
- Generate maps with appropriate 3D settings
- Organize tours in logical sequences

### Example: AI-Generated Historical Map

**User**: "Show me the major battles of World War I on the Western Front with details"

**Claude creates**:
- Chronological markers (1914-1918)
- Detailed battle information (dates, forces, casualties, outcomes)
- 3D terrain showing strategic geography
- Animated tour following the timeline
- Clickable descriptions for each battle

### Links

- [PowerShell.MCP on PowerShell Gallery](https://www.powershellgallery.com/packages/PowerShell.MCP)
- [PowerShell.MCP on GitHub](https://github.com/yotsuda/PowerShell.MCP)

## Requirements

- **PowerShell**: 7.2 or later
- **Platform**: Windows 10/11
- **Internet**: Required for OpenStreetMap tiles and geocoding
- **Browser**: Modern browser with WebGL support (Chrome, Edge, Firefox)

## Technical Details

- **Map Engine**: MapLibre GL JS (upgraded from Leaflet.js in v1.0.0)
- **3D Terrain**: AWS Terrarium RGB elevation tiles
- **3D Buildings**: OpenStreetMap building data with height tags
- **Geocoding**: Nominatim API (rate limit: 1 request/second)
- **Server**: Local HTTP server on http://localhost:8765/
- **Projection**: Web Mercator (EPSG:3857)

## Performance Tips

- **Large Datasets**: Use `-Duration 0` to disable animation for instant rendering
- **3D Mode**: Disable 3D (`-Disable3D`) if performance is slow on older hardware
- **Geocoding**: Cache results to avoid repeated API calls
- **Zoom Level**: Use appropriate zoom (8-10 for regions, 15+ for landmarks)
- **Multiple Markers**: Consider grouping nearby locations to reduce clutter

## Troubleshooting

### Map doesn't open
- Check if port 8765 is available
- Try closing and reopening PowerShell
- Verify internet connection
- Check Windows Firewall settings

### 3D not working
- Ensure browser supports WebGL
- Check zoom level (3D buildings require zoom 14+)
- Try updating your browser
- Verify graphics drivers are up to date

### Geocoding fails
- Verify internet connection
- Check Nominatim API status at https://nominatim.openstreetmap.org/
- Respect rate limit (1 request/second)
- Use coordinates directly if name lookup fails

### Description not showing
- Click directly on the marker (not the label)
- Ensure Description parameter was provided
- Check browser console for errors

## Version History

### v1.0.1 (Current)
- Added 3D terrain and building visualization
- Added Description parameter for rich marker information
- Upgraded from Leaflet.js to MapLibre GL JS
- Added stateful camera controls (Pitch, Bearing)
- Enhanced animation with Duration parameter
- Improved error handling and performance

### v1.0.0
- Initial release
- Basic 2D map display
- Route visualization
- Animated tours
- Geocoding support
- CSV pipeline support

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Links

- [GitHub Repository](https://github.com/yotsuda/PowerShell.Map)
- [PowerShell Gallery](https://www.powershellgallery.com/packages/PowerShell.Map)
- [Issue Tracker](https://github.com/yotsuda/PowerShell.Map/issues)
- [Changelog](https://github.com/yotsuda/PowerShell.Map/blob/main/CHANGELOG.md)

## Credits

- **MapLibre GL JS**: https://maplibre.org/
- **OpenStreetMap**: https://www.openstreetmap.org/
- **Nominatim**: https://nominatim.openstreetmap.org/
- **AWS Terrain Tiles**: https://registry.opendata.aws/terrain-tiles/

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
