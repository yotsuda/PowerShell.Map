---
external help file: PowerShell.Map.dll-Help.xml
Module Name: PowerShell.Map
online version: https://github.com/yotsuda/PowerShell.Map
schema: 2.0.0
---

# Show-OpenStreetMapRoute

## SYNOPSIS
Displays a route between two locations on an interactive map.

## SYNTAX

```
Show-OpenStreetMapRoute [-From] <String> [-To] <String> [-Color <String>] [-Width <Int32>] [-DebugMode]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Show-OpenStreetMapRoute cmdlet displays a calculated route between two locations on an interactive map in your default web browser. Routes are calculated using the OSRM (Open Source Routing Machine) API. Locations can be specified as place names (using geocoding) or as coordinate strings.

## EXAMPLES

### Example 1: Display route between two cities
```powershell
Show-OpenStreetMapRoute -From "Tokyo" -To "Osaka"
```

Displays a route from Tokyo to Osaka using the default blue color.

### Example 2: Display route with custom color and width
```powershell
Show-OpenStreetMapRoute -From "Tokyo" -To "Osaka" -Color "#ff0000" -Width 6
```

Displays a route with red color and thicker line (6 pixels).

### Example 3: Display route using coordinates
```powershell
Show-OpenStreetMapRoute -From "35.6586,139.7454" -To "34.6937,135.5023"
```

Displays a route using coordinate strings (Tokyo Tower to Osaka).

### Example 4: Display multiple routes
```powershell
$routes = @(
    @{From="Tokyo"; To="Nagoya"}
    @{From="Nagoya"; To="Osaka"}
    @{From="Osaka"; To="Hiroshima"}
)
$routes | ForEach-Object {
    Show-OpenStreetMapRoute -From $_.From -To $_.To
    Start-Sleep -Seconds 2
}
```

Displays multiple routes in sequence with a 2-second delay between each.

### Example 5: Enable debug mode
```powershell
Show-OpenStreetMapRoute -From "Tokyo" -To "Kyoto" -DebugMode
```

Displays the route with debug information showing API calls and route details.

## PARAMETERS

### -From
Specifies the starting location. Can be a place name (e.g., "Tokyo") or a coordinate string in "latitude,longitude" format (e.g., "35.6586,139.7454").

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -To
Specifies the destination location. Can be a place name (e.g., "Osaka") or a coordinate string in "latitude,longitude" format (e.g., "34.6937,135.5023").

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Color
Specifies the color of the route line. Can be a color name or hex color code (e.g., "#ff0000" for red). Default is "#0066ff" (blue).

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: #0066ff
Accept pipeline input: False
Accept wildcard characters: False
```

### -Width
Specifies the width of the route line in pixels (1 to 10). Default is 4.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 4
Accept pipeline input: False
Accept wildcard characters: False
```

### -DebugMode
Enables debug mode, which displays detailed logging information on the map interface, including API calls and route calculation details.

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

### None
This cmdlet does not accept pipeline input.

## OUTPUTS

### None
This cmdlet does not generate any output.

## NOTES
- Routes are calculated using the OSRM API (http://project-osrm.org/)
- Geocoding uses the Nominatim API (https://nominatim.openstreetmap.org/)
- The map server runs on http://localhost:8765/
- The map automatically centers and zooms to show the entire route
- Route calculation is performed for driving routes
- The browser tab is automatically opened when the server starts

## RELATED LINKS

[Show-OpenStreetMap](Show-OpenStreetMap.md)
[OSRM API](http://project-osrm.org/)
[OpenStreetMap](https://www.openstreetmap.org/)
