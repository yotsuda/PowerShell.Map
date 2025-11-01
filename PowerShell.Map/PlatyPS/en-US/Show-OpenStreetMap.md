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

### SimpleLocation
```
Show-OpenStreetMap [-Location <String[]>] [-Zoom <Int32>] [-Duration <Double>] [-Enable3D] [-Disable3D]
 [-Bearing <Double>] [-Pitch <Double>] [-Description <String>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### StructuredLocation
```
Show-OpenStreetMap -Locations <Object[]> [-Zoom <Int32>] [-Duration <Double>] [-Enable3D] [-Disable3D]
 [-Bearing <Double>] [-Pitch <Double>] [-Description <String>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### Pipeline
```
Show-OpenStreetMap -Latitude <String> -Longitude <String> [-Label <String>] [-Color <String>] [-Zoom <Int32>]
 [-Duration <Double>] [-Enable3D] [-Disable3D] [-Bearing <Double>] [-Pitch <Double>] [-Description <String>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Opens an interactive map in the default browser (http://localhost:8765/). Supports place names (auto-geocoded), coordinates, multiple markers with metadata (including clickable descriptions), 3D terrain/buildings, and CSV pipeline input. Use the Description parameter or Locations array to provide detailed information that appears when markers are clicked.

## EXAMPLES

### Example 1: Single location with description (for AI-assisted information display)

```powershell
Show-OpenStreetMap "Tokyo Tower" -Description "🗼 Tokyo Tower`nHeight: 332.9m`nBuilt: 1958"
```

### Example 2: Multiple markers

```powershell
Show-OpenStreetMap Tokyo, Osaka, Kyoto
```

### Example 3: Structured locations with detailed descriptions

```powershell
$locations = @(
    @{ Location = "Tokyo Tower"; Description = "🗼 Tokyo Tower`nHeight: 332.9m`nBuilt: 1958"; Label = "東京タワー"; Color = "red" }
    @{ Location = "Mount Fuji"; Description = "🗻 Mt. Fuji`nHeight: 3,776m`nUNESCO World Heritage"; Label = "富士山"; Color = "blue" }
)
Show-OpenStreetMap -Locations $locations
```

### Example 4: 3D visualization

```powershell
Show-OpenStreetMap "35.3606,138.7274" -Enable3D -Zoom 11 -Pitch 70 -Bearing 45
```

## PARAMETERS

### -Bearing
Camera rotation in degrees: 0=North, 90=East, 180=South, 270=West. **STATEFUL**: If not specified, the map retains its current bearing from previous commands.

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
Marker color: color name or hex code (e.g., "red" or "#FF5733"). **Supported colors**: red, blue, green, orange, yellow, violet, purple, indigo, pink, cyan, teal, black, grey, gray, white, silver, darkred, darkgreen, darkblue, lightred, lightgreen, lightblue, navy, lime, magenta, maroon, olive, brown, gold, crimson, coral, turquoise, skyblue, lavender, plum, salmon, khaki **Hex codes**: #rgb (e.g., #F00) or #rrggbb (e.g., #FF0000)

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

### -Description
Text displayed when marker is clicked. Supports emoji and multi-line text (use `n for newlines). Critical for AI-assisted information display where detailed context is provided to users. For single location only (SimpleLocation parameter set).


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

### -Location
Place name(s) or "lat,lon" coordinate string(s). Auto-geocoded via Nominatim API. Multiple locations shown as markers with auto-fit bounds.


```yaml
Type: String[]
Parameter Sets: SimpleLocation
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Locations
Array of hashtables with properties: Location (required), Description, Label, Color (optional). For structured markers with metadata.


```yaml
Type: Object[]
Parameter Sets: StructuredLocation
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Collections.Hashtable[]
You can pipe an array of hashtables containing marker information to this cmdlet.

## OUTPUTS

### PowerShell.Map.Server.MapMarker
This cmdlet outputs MapMarker objects containing information about the displayed locations, including coordinates, labels, geocoding status, and source information.

## NOTES

**Stateful Behavior**
- If not specified, Bearing, Pitch and 3D mode state is preserved. To reset the view, explicitly specify the desired values.

**Server and API**
- Map server runs on http://localhost:8765/
- Geocoding uses Nominatim API (https://nominatim.openstreetmap.org/)
- Nominatim rate limit: 1 request per second
- Browser tab opens automatically when server starts
- Map updates in real-time when you run commands multiple times

**3D Features**
- 3D terrain data: AWS Terrarium tiles (https://registry.opendata.aws/terrain-tiles/)
- 3D buildings: Available at zoom level 14+ in supported areas
- Terrain exaggeration: Dynamic scaling from 0.3x (urban) to 2.0x (mountains)
- Browser controls: 3D toggle and camera reset available

## RELATED LINKS

[Show-OpenStreetMapRoute](Show-OpenStreetMapRoute.md)
[Start-OpenStreetMapTour](Start-OpenStreetMapTour.md)
[OpenStreetMap](https://www.openstreetmap.org/)
[MapLibre GL JS](https://maplibre.org/)
