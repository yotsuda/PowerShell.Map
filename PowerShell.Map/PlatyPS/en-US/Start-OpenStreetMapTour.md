---
external help file: PowerShell.Map.dll-Help.xml
Module Name: PowerShell.Map
online version: https://github.com/yotsuda/PowerShell.Map
schema: 2.0.0
---

# Start-OpenStreetMapTour

## SYNOPSIS
Creates an animated tour that visits multiple locations sequentially on an interactive map.

## SYNTAX

### Simple
```
Start-OpenStreetMapTour -Location <String[]> [-Zoom <Int32>] [-PauseTime <Double>] [-Duration <Double>]
 [-Enable3D] [-Disable3D] [-Bearing <Double>] [-Pitch <Double>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### WithDescription
```
Start-OpenStreetMapTour -Locations <Object[]> [-Zoom <Int32>] [-PauseTime <Double>] [-Duration <Double>]
 [-Enable3D] [-Disable3D] [-Bearing <Double>] [-Pitch <Double>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
Creates animated tour smoothly flying between locations. Pauses at each location before continuing. Blocks PowerShell until complete. Perfect for presentations. Use Locations array with Description for detailed information at each stop.

## EXAMPLES

### Example 1: Simple tour

```powershell
Start-OpenStreetMapTour Tokyo, Osaka, Kyoto
```

### Example 2: With timing control

```powershell
Start-OpenStreetMapTour Paris, London, Berlin -Duration 2 -PauseTime 1.5 -Zoom 12
```

### Example 3: Tour with detailed descriptions (important for AI-guided tours)

```powershell
$tourStops = @(
    @{ Location = "Tokyo Tower"; Description = "🗼 Tokyo Tower`nHeight: 332.9m`nBuilt: 1958" }
    @{ Location = "Mount Fuji"; Description = "🗻 Mt. Fuji`nHeight: 3,776m`nUNESCO World Heritage" }
    @{ Location = "Kyoto"; Description = "⛩️ Kyoto`n2000+ temples and shrines" }
)
Start-OpenStreetMapTour -Locations $tourStops -Duration 1.5 -PauseTime 2
```

### Example 4: 3D mountain tour

```powershell
Start-OpenStreetMapTour -Locations $tourStops -Enable3D -Pitch 70 -Zoom 12
```

## PARAMETERS

### -Bearing
Camera rotation in degrees: 0=North, 90=East, 180=South, 270=West. **STATEFUL**: If not specified, the map retains its current bearing from previous commands. Applies to all tour stops.

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

### -Disable3D
Forces 2D flat view for all tour stops. Disables 3D terrain and building rendering, locks pitch to 0. **STATEFUL**: If neither -Enable3D nor -Disable3D is specified, the map retains its current 3D/2D state from previous commands.

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
Specifies the animation duration for transitions between locations in seconds (0.1 to 10.0). This controls how fast the camera flies from one location to the next. Default is 1.5 seconds.

```yaml
Type: Double
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 1.5
Accept pipeline input: False
Accept wildcard characters: False
```

### -Enable3D
Enables 3D buildings (zoom 14+) and terrain with dynamic exaggeration (0.3x-2.0x). Auto-sets pitch=60. Applies to all tour stops. **STATEFUL**: If neither -Enable3D nor -Disable3D is specified, the map retains its current 3D/2D state from previous commands.

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

### -Location
Specifies an array of locations to visit in the tour. Each location can be a place name (e.g., "Tokyo") or a coordinate string in "latitude,longitude" format (e.g., "35.6586,139.7454"). The tour visits locations in the order specified.

```yaml
Type: String[]
Parameter Sets: Simple
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Locations
Specifies an array of structured location objects with optional descriptions. Each element should be a hashtable or PSObject with the following properties:
- Location (required): Place name or coordinate string
- Description (optional): Text to display when visiting this location

Example:
$locations = @(
    @{ Location = "Tokyo"; Description = "🗼 Capital of Japan - Population: 14 million" }
    @{ Location = "Kyoto"; Description = "⛩️ Ancient capital - 2000+ temples" }
)
Start-OpenStreetMapTour -Locations $locations

```yaml
Type: Object[]
Parameter Sets: WithDescription
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -PauseTime
Specifies how long to pause at each location in seconds (0.5 to 30.0). This is the time the map stays still at each location before transitioning to the next. Default is 2.0 seconds.

```yaml
Type: Double
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 2.0
Accept pipeline input: False
Accept wildcard characters: False
```

### -Pitch
Camera tilt in degrees (0-85): 0=top-down, 60=default for 3D, 85=almost horizontal. **STATEFUL**: If not specified, the map retains its current pitch from previous commands. Applies to all tour stops.

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
Specifies the zoom level for each location (1 to 19). Lower numbers show a larger area, higher numbers show more detail. Default is 13.

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

### System.String[]
You can pipe an array of location strings to this cmdlet.

## OUTPUTS

### PowerShell.Map.Server.MapMarker
Returns a MapMarker object for each location visited, including Step, TotalSteps, Location, Latitude, Longitude, Label, Status, and GeocodingSource properties.

## NOTES

**Stateful Behavior**
- If not specified, Bearing, Pitch and 3D mode state is preserved. To reset the view, explicitly specify the desired values.

**Tour Behavior**
- Tours run sequentially and block PowerShell until complete
- Place names are auto-geocoded; coordinates use "latitude,longitude" format
- Total tour time: (Duration + PauseTime) × Number of Locations
- Failed locations are skipped with a warning; tour continues

**Server and API**
- Map server: http://localhost:8765/
- Geocoding: Nominatim API (rate limit: 1 request per second)

## RELATED LINKS

[Show-OpenStreetMap](Show-OpenStreetMap.md)
[Show-OpenStreetMapRoute](Show-OpenStreetMapRoute.md)
[OpenStreetMap](https://www.openstreetmap.org/)
