---
external help file: PowerShell.Map.dll-Help.xml
Module Name: PowerShell.Map
online version: https://github.com/yotsuda/PowerShell.Map
schema: 2.0.0
---

# Show-OpenStreetMap

## SYNOPSIS
Displays an interactive map using OpenStreetMap and Leaflet.js in the default browser.

## SYNTAX

### Location
```
Show-OpenStreetMap [[-Location] <String[]>] [-Marker <String>] [-Zoom <Int32>] [-Duration <Double>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Markers
```
Show-OpenStreetMap -Markers <Object[]> [-Zoom <Int32>] [-Duration <Double>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Pipeline
```
Show-OpenStreetMap -Latitude <String> -Longitude <String> [-Label <String>] [-Color <String>] [-Zoom <Int32>]
 [-Duration <Double>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Show-OpenStreetMap cmdlet displays an interactive map in your default web browser. You can specify locations using coordinates, place names (geocoding), or display multiple markers. The map is powered by OpenStreetMap and Leaflet.js, with a local HTTP server running on port 8765.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Collections.Hashtable[]
You can pipe an array of hashtables containing marker information to this cmdlet.

## OUTPUTS

### None
This cmdlet does not generate any output.

## NOTES
- The map server runs on http://localhost:8765/
- Geocoding uses the Nominatim API (https://nominatim.openstreetmap.org/)
- Nominatim has a usage policy of 1 request per second
- The map updates in real-time when you run the cmdlet multiple times
- The browser tab is automatically opened when the server starts

## RELATED LINKS

[Show-OpenStreetMapRoute](Show-OpenStreetMapRoute.md)
[OpenStreetMap](https://www.openstreetmap.org/)
[Leaflet.js](https://leafletjs.com/)
