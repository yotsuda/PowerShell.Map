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
Show-OpenStreetMapRoute -From <Object> -To <Object> [-Color <String>] [-Width <Int32>] [-Zoom <Int32>]
 [-Duration <Double>] [-Profile <String>] [-Enable3D] [-Disable3D] [-Bearing <Double>] [-Pitch <Double>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Calculates and displays route between two locations using OSRM API. Locations: place names (auto-geocoded) or coordinates. Auto-fits bounds unless -Zoom specified. Use Description in From/To hashtables to provide context at start/end points.

## EXAMPLES

### Example 1: Basic route

```powershell
Show-OpenStreetMapRoute -From Tokyo -To Osaka
```

### Example 2: With routing profile and styling

```powershell
Show-OpenStreetMapRoute -From Tokyo -To Osaka -Profile walking -Color "#ff0000" -Width 6
```

### Example 3: With descriptions (important for AI-assisted display)

```powershell
Show-OpenStreetMapRoute `
    -From @{ Location = "Tokyo Station"; Description = "🚉 Start point`nMajor railway hub" } `
    -To @{ Location = "Kyoto Station"; Description = "🎯 Destination`nGateway to temples" } `
    -Profile driving
```

## PARAMETERS

### -Bearing
Camera rotation in degrees: 0=North, 90=East, 180=South, 270=West. STATEFUL: If not specified, the map retains its current bearing from previous commands.

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

### -Color
Route line color: color name or hex code (e.g., "red" or "#FF0000"). Default: #0066ff. Supported colors: red, blue, green, orange, yellow, violet, purple, indigo, pink, cyan, teal, black, grey, gray, white, silver, darkred, darkgreen, darkblue, lightred, lightgreen, lightblue, navy, lime, magenta, maroon, olive, brown, gold, crimson, coral, turquoise, skyblue, lavender, plum, salmon, khaki. Hex codes: #rgb (e.g., #F00) or #rrggbb (e.g., #FF0000).

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

### -Disable3D
Forces 2D flat view. Disables 3D terrain and building rendering, locks pitch to 0. **STATEFUL**: If neither -Enable3D nor -Disable3D is specified, the map retains its current 3D/2D state from previous commands.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
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

### -Enable3D
Enables 3D buildings (zoom 14+) and terrain with dynamic exaggeration (0.3x-2.0x). Auto-sets pitch=60. **STATEFUL**: If neither -Enable3D nor -Disable3D is specified, the map retains its current 3D/2D state from previous commands.

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

### -From
Specifies the starting location for the route. This parameter accepts two formats:

1. Simple string: Place name (e.g., "Tokyo") or coordinate string (e.g., "35.6586,139.7454")
2. Hashtable with optional metadata:
   - Location (required): Place name or coordinates
   - Description (optional): Text to display when the marker is clicked

Example with description:
Show-OpenStreetMapRoute -From @{ Location = "Tokyo"; Description = "?? Starting point" } -To "Osaka"

```yaml
Type: Object
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Pitch
Camera tilt in degrees (0-85): 0=top-down, 60=default for 3D, 85=almost horizontal. **STATEFUL**: If not specified, the map retains its current pitch from previous commands.

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

### -Profile
Routing profile for route calculation. Default: driving. **Valid values**: driving, walking, cycling (case-insensitive). Different profiles produce different routes based on allowed roads and paths.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: driving
Accept pipeline input: False
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

### -To
Specifies the destination location for the route. This parameter accepts two formats:

1. Simple string: Place name (e.g., "Osaka") or coordinate string (e.g., "34.6937,135.5023")
2. Hashtable with optional metadata:
   - Location (required): Place name or coordinates
   - Description (optional): Text to display when the marker is clicked

Example with description:
Show-OpenStreetMapRoute -From "Tokyo" -To @{ Location = "Osaka"; Description = "?? Destination" }

```yaml
Type: Object
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
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

### -Zoom
Specifies the zoom level (1 to 19) for the map view. When specified, displays the start point at the requested zoom level. When omitted, automatically fits bounds to show the entire route. Lower numbers show a larger area, higher numbers show more detail.

```yaml
Type: Int32
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

### None
This cmdlet does not accept pipeline input.

## OUTPUTS

### None
This cmdlet does not generate any output.

## NOTES

**Stateful Behavior**
- If not specified, Bearing, Pitch and 3D mode state is preserved. To reset the view, explicitly specify the desired values.

**Routing and APIs**
- Routes: Calculated using OSRM API (http://project-osrm.org/)
- Geocoding: Nominatim API (https://nominatim.openstreetmap.org/)
- Map server: http://localhost:8765/
- Auto-fit: When -Zoom omitted, map fits bounds to show entire route
- Browser: Tab opens automatically when server starts

**3D Features**
- 3D terrain data: AWS Terrarium tiles (when -Enable3D used)
- 3D buildings: Available at zoom level 14+ in supported areas

## RELATED LINKS

[Show-OpenStreetMap](Show-OpenStreetMap.md)
[Start-OpenStreetMapTour](Start-OpenStreetMapTour.md)
[OSRM API](http://project-osrm.org/)
[OpenStreetMap](https://www.openstreetmap.org/)
