---
external help file: PowerShell.Map.dll-Help.xml
Module Name: PowerShell.Map
online version: https://github.com/yotsuda/PowerShell.Map
schema: 2.0.0
---

# Show-OpenStreetMap

## SYNOPSIS
Displays an interactive 2D/3D map using OpenStreetMap and MapLibre GL JS in the default browser.

## SYNTAX

### Location
```
Show-OpenStreetMap [[-Location] <String[]>] [-Marker <String>] [-Zoom <Int32>] [-Duration <Double>] [-Enable3D]
 [-Bearing <Double>] [-Pitch <Double>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Markers
```
Show-OpenStreetMap -Markers <Object[]> [-Zoom <Int32>] [-Duration <Double>] [-Enable3D] [-Bearing <Double>]
 [-Pitch <Double>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Pipeline
```
Show-OpenStreetMap -Latitude <String> -Longitude <String> [-Label <String>] [-Color <String>] [-Zoom <Int32>]
 [-Duration <Double>] [-Enable3D] [-Bearing <Double>] [-Pitch <Double>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
The Show-OpenStreetMap cmdlet displays an interactive map in your default web browser. You can specify locations using coordinates, place names (geocoding), or display multiple markers. The map is powered by OpenStreetMap and MapLibre GL JS, with a local HTTP server running on port 8765.

The cmdlet supports 3D visualization of buildings and terrain when using the -Enable3D parameter. The 3D view includes dynamically scaled terrain exaggeration (0.3x to 2.0x based on zoom level) for natural-looking mountain visualization, and extruded building shapes in urban areas.

## EXAMPLES

### Example 1: Display map by place name
```powershell
Show-OpenStreetMap "Tokyo Tower"
```

Uses geocoding to find "Tokyo Tower" and displays the map at that location.

### Example 2: Display map with coordinates
```powershell
Show-OpenStreetMap -Location "35.6586,139.7454" -Zoom 15
```

Displays a map centered on Tokyo Tower (using coordinate string) with zoom level 15.

### Example 3: Display multiple locations as markers
```powershell
Show-OpenStreetMap -Location Tokyo, Osaka, Kyoto
```

Displays a map with three markers at different cities. The map automatically centers and zooms to show all markers.

### Example 4: Display map with a labeled marker
```powershell
Show-OpenStreetMap "Tokyo Tower" -Marker "🗼 東京タワー"
```

Displays a map with a marker labeled "🗼 東京タワー" at Tokyo Tower.

### Example 5: Display markers using simple string array
```powershell
Show-OpenStreetMap -Markers Tokyo, Osaka, Kyoto, Hiroshima
```

Displays markers at multiple cities using a simple string array.

### Example 6: Display markers with labels and colors (pipe-delimited format)
```powershell
Show-OpenStreetMap -Markers "Tokyo|🗼 東京タワー|red", "Osaka|🏯 大阪城|blue", "Kyoto|⛩️ 清水寺|gold"
```

Displays colored markers with labels using "Location|Label|Color" format.

### Example 7: Display markers using hashtable array
```powershell
$markers = @(
    @{Location="Tokyo"; Label="Tokyo"; Color="red"}
    @{Location="Osaka"; Label="Osaka"; Color="blue"}
    @{Location="Kyoto"; Label="Kyoto"; Color="green"}
)
Show-OpenStreetMap -Markers $markers
```

Displays a map with three colored markers at different cities using hashtable format.

### Example 8: Display markers from CSV using pipeline
```powershell
Import-Csv locations.csv | Show-OpenStreetMap
```

Imports locations from a CSV file (with Latitude, Longitude, Label, Color columns) and displays them as markers on the map using pipeline.

### Example 9: Create CSV data and display markers
```powershell
$locations = Import-Csv locations.csv
$markers = $locations | ForEach-Object {
    @{Location="$($_.Latitude),$($_.Longitude)"; Label=$_.Name; Color=$_.Color}
}
Show-OpenStreetMap -Markers $markers
```

Imports locations from a CSV file, converts to hashtable format, and displays as markers.
Imports locations from a CSV file, converts to hashtable format, and displays as markers.

### Example 10: Display 3D map with buildings and terrain
```powershell
Show-OpenStreetMap "Tokyo" -Enable3D
```

Displays Tokyo in 3D mode with buildings and terrain elevation. The camera is automatically tilted to 60 degrees for optimal 3D viewing.

### Example 11: Display mountains with custom camera angle
```powershell
Show-OpenStreetMap "35.3606,138.7274" -Marker "Mt. Fuji" -Enable3D -Zoom 11 -Pitch 70 -Bearing 45
```

Displays Mt. Fuji in 3D with a 70-degree pitch (tilt) and 45-degree bearing (rotation) for dramatic terrain visualization.

### Example 12: Display famous mountains in 3D
```powershell
Show-OpenStreetMap "45.9763,7.6586" -Marker "Matterhorn" -Enable3D -Zoom 12 -Pitch 70
```

Displays the Matterhorn mountain in the Alps with 3D terrain visualization.

## PARAMETERS

### -Location
Specifies one or more locations as place names or coordinate strings. Place names are geocoded using the Nominatim API. Coordinate strings should be in "latitude,longitude" format.

When multiple locations are provided, they are displayed as markers on the map, and the map automatically centers and zooms to show all locations.

```yaml
Type: String[]
Parameter Sets: Location
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Zoom
Specifies the zoom level (1 to 19). Lower numbers show a larger area, higher numbers show more detail. Default is 13.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 13
Accept pipeline input: False
Accept wildcard characters: False
```

### -Marker
Specifies a label for a single marker to be displayed at the specified location.

```yaml
Type: String
Parameter Sets: Location
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Markers
Specifies an array of markers to display on the map. Accepts multiple formats:

- **String array**: Simple location names (e.g., "Tokyo", "Osaka")
- **Pipe-delimited strings**: "Location|Label|Color" format (e.g., "Tokyo|東京|red")
- **Hashtable array**: Each hashtable should have Location (required), Label (optional), and Color (optional) properties
- **MapMarker objects**: Strongly-typed MapMarker objects

Available colors: red, blue, green, orange, violet, yellow, grey, black, gold.

```yaml
Type: Object[]
Parameter Sets: Markers
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -ProgressAction
Specifies how PowerShell responds to progress updates generated by the cmdlet. Valid values are: Continue, Ignore, Inquire, SilentlyContinue, Stop, Suspend.

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Color
Specifies the marker color when using pipeline input (e.g., from Import-Csv). Available colors: red, blue, green, orange, violet, yellow, grey, black, gold.

```yaml
Type: String
Parameter Sets: Pipeline
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Label
Specifies the marker label when using pipeline input (e.g., from Import-Csv). The label will be displayed when clicking on the marker.

```yaml
Type: String
Parameter Sets: Pipeline
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Latitude
Specifies the latitude coordinate when using pipeline input (e.g., from Import-Csv). Must be used together with -Longitude parameter. Valid range: -90 to 90 degrees.

```yaml
Type: String
Parameter Sets: Pipeline
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Longitude
Specifies the longitude coordinate when using pipeline input (e.g., from Import-Csv). Must be used together with -Latitude parameter. Valid range: -180 to 180 degrees.

```yaml
Type: String
Parameter Sets: Pipeline
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Duration
Specifies the animation duration in seconds (0.0 to 10.0) for map transitions. Set to 0 for instant movement without animation. Default is 1.0 second.

```yaml
Type: Double
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Bearing
Specifies the camera bearing (rotation) in degrees (0-360). The bearing represents the compass direction the camera is pointing:
- 0 degrees = North (default)
- 90 degrees = East
- 180 degrees = South
- 270 degrees = West

This parameter is useful for orienting the view of 3D terrain features.

```yaml
Type: Double
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -Enable3D
Enables 3D visualization of buildings and terrain. When enabled:
- Buildings are rendered as 3D extruded shapes (at zoom level 14 and above)
- Terrain elevation data is displayed with appropriate exaggeration for visibility
- Default pitch angle is set to 60 degrees for optimal 3D viewing
- Terrain exaggeration scales dynamically based on zoom level (0.3x to 2.0x)

The 3D view can be toggled on/off using the browser interface, and the camera view can be reset using the "Reset View" button.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -Pitch
Specifies the camera pitch (tilt angle) in degrees (0-85). The pitch controls how much the camera is tilted:
- 0 degrees = Top-down view (default for 2D maps)
- 60 degrees = Default for 3D view (automatically set when -Enable3D is used without explicit -Pitch)
- 85 degrees = Almost horizontal view

Higher pitch values provide a more dramatic 3D perspective, especially for mountainous terrain. This parameter is most useful when -Enable3D is enabled.

```yaml
Type: Double
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Collections.Hashtable[]
You can pipe an array of hashtables containing marker information to this cmdlet.

## OUTPUTS

### PowerShell.Map.Server.MapMarker
This cmdlet outputs MapMarker objects containing information about the displayed locations, including coordinates, labels, geocoding status, and source information.

## NOTES
- The map server runs on http://localhost:8765/
- Geocoding uses the Nominatim API (https://nominatim.openstreetmap.org/)
- Nominatim has a usage policy of 1 request per second
- The map updates in real-time when you run the cmdlet multiple times
- The browser tab is automatically opened when the server starts
- 3D terrain data is sourced from AWS Terrarium tiles (https://registry.opendata.aws/terrain-tiles/)
- 3D buildings are available at zoom level 14 and above in supported areas
- Terrain exaggeration scales dynamically: 0.3x (urban areas) to 2.0x (mountain regions)
- The 3D toggle and camera reset controls are available in the browser interface

## RELATED LINKS

[Show-OpenStreetMapRoute](Show-OpenStreetMapRoute.md)
[Start-OpenStreetMapTour](Start-OpenStreetMapTour.md)
[OpenStreetMap](https://www.openstreetmap.org/)
[MapLibre GL JS](https://maplibre.org/)
