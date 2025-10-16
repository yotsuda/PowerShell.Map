---
external help file: PowerShell.Map.dll-Help.xml
Module Name: PowerShell.Map
online version: https://github.com/yoshifumi-tsuda/PowerShell.Map
schema: 2.0.0
---

# Show-OpenStreetMap

## SYNOPSIS
Displays an interactive map using OpenStreetMap and Leaflet.js in the default browser.

## SYNTAX

```
Show-OpenStreetMap [[-Latitude] <Double>] [[-Longitude] <Double>] [[-Location] <String>] [-Zoom <Int32>]
 [-Marker <String>] [-Markers <Hashtable[]>] [-DebugMode] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
The Show-OpenStreetMap cmdlet displays an interactive map in your default web browser. You can specify locations using coordinates, place names (geocoding), or display multiple markers. The map is powered by OpenStreetMap and Leaflet.js, with a local HTTP server running on port 8765.

## EXAMPLES

### Example 1: Display map by coordinates
```powershell
Show-OpenStreetMap -Latitude 35.6586 -Longitude 139.7454 -Zoom 15
```

Displays a map centered on Tokyo Tower with zoom level 15.

### Example 2: Display map by place name
```powershell
Show-OpenStreetMap -Location "Osaka Castle"
```

Uses geocoding to find "Osaka Castle" and displays the map at that location.

### Example 3: Display map with a marker
```powershell
Show-OpenStreetMap -Location "Tokyo Tower" -Marker "東京タワー"
```

Displays a map with a marker labeled "東京タワー" at Tokyo Tower.

### Example 4: Display multiple markers
```powershell
$markers = @(
    @{Location="Tokyo"; Label="Tokyo"; Color="red"}
    @{Location="Osaka"; Label="Osaka"; Color="blue"}
    @{Location="Kyoto"; Label="Kyoto"; Color="green"}
)
Show-OpenStreetMap -Markers $markers
```

Displays a map with three colored markers at different cities. The map automatically centers and zooms to show all markers.

### Example 5: Display markers from CSV
```powershell
$locations = Import-Csv locations.csv
$markers = $locations | ForEach-Object {
    @{Location="$($_.Lat),$($_.Lon)"; Label=$_.Name; Color=$_.Color}
}
Show-OpenStreetMap -Markers $markers
```

Imports locations from a CSV file and displays them as markers on the map.

## PARAMETERS

### -Latitude
Specifies the latitude coordinate (-90 to 90 degrees). Must be used together with -Longitude parameter.

```yaml
Type: Double
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Longitude
Specifies the longitude coordinate (-180 to 180 degrees). Must be used together with -Latitude parameter.

```yaml
Type: Double
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Location
Specifies the location as a place name or coordinate string. Place names are geocoded using the Nominatim API. Coordinate strings should be in "latitude,longitude" format.

```yaml
Type: String
Parameter Sets: (All)
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
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Markers
Specifies an array of hashtables containing marker information. Each hashtable should have Location (required), Label (optional), and Color (optional) properties. Available colors: red, blue, green, orange, violet, yellow, grey, black, gold.

```yaml
Type: Hashtable[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -DebugMode
Enables debug mode, which displays detailed logging information on the map interface.

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

### -ProgressAction
{{ Fill ProgressAction Description }}

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
