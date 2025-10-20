---
Module Name: PowerShell.Map
Module Guid: 00000000-0000-0000-0000-000000000000
Download Help Link: https://github.com/yotsuda/PowerShell.Map
Help Version: 0.1.0
Locale: en-US
---

# PowerShell.Map Module
## Description
PowerShell.Map is a module for displaying interactive maps using OpenStreetMap and Leaflet.js.  This module provides cmdlets to: - Display interactive maps in your web browser - Add markers to maps with customizable labels and colors - Calculate and display routes between locations - Support multiple input formats (place names, coordinates, CSV files) - Geocode location names using the Nominatim API - Route calculation using the OSRM API The module runs a local HTTP server on port 8765 to serve the interactive map interface.

## PowerShell.Map Cmdlets
### [Show-OpenStreetMap](Show-OpenStreetMap.md)
Displays an interactive 2D/3D map using OpenStreetMap and MapLibre GL JS in the default browser.

### [Show-OpenStreetMapRoute](Show-OpenStreetMapRoute.md)
Displays a route between two locations on an interactive map.

### [Start-OpenStreetMapTour](Start-OpenStreetMapTour.md)
Creates an animated tour that visits multiple locations sequentially on an interactive map.

