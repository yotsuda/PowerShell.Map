---
Module Name: PowerShell.Map
Module Guid: 00000000-0000-0000-0000-000000000000
Download Help Link: https://github.com/yotsuda/PowerShell.Map
Help Version: 0.1.0
Locale: en-US
---

# PowerShell.Map Module
## Description
Displays interactive 2D/3D maps using OpenStreetMap and MapLibre GL JS in a browser. Key Features: - Display maps with markers (place names or coordinates) - Calculate and display routes (driving/walking/cycling) - Create animated tours visiting multiple locations - 3D terrain and building visualization - Local HTTP server on port 8765 APIs Used: - Geocoding: Nominatim API (1 req/sec limit)
- Routing: OSRM API
- Terrain: AWS Terrarium tiles

## PowerShell.Map Cmdlets
### [Show-OpenStreetMap](Show-OpenStreetMap.md)
Displays an interactive 2D/3D map using OpenStreetMap and MapLibre GL JS in the default browser.

### [Show-OpenStreetMapRoute](Show-OpenStreetMapRoute.md)
Displays a route between two locations on an interactive map.

### [Start-OpenStreetMapTour](Start-OpenStreetMapTour.md)
Creates an animated tour that visits multiple locations sequentially on an interactive map.

