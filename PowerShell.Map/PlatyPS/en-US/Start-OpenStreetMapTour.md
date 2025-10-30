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
Start-OpenStreetMapTour [-Location] <String[]> [-Zoom <Int32>] [-PauseTime <Double>] [-Duration <Double>]
 [-Enable3D] [-Bearing <Double>] [-Pitch <Double>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### WithDescription
```
Start-OpenStreetMapTour -Locations <Object[]> [-Zoom <Int32>] [-PauseTime <Double>] [-Duration <Double>]
 [-Enable3D] [-Bearing <Double>] [-Pitch <Double>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Start-OpenStreetMapTour cmdlet creates an animated tour that smoothly transitions between multiple locations on an interactive map. The map camera flies from one location to the next with smooth zoom animations, pausing at each location before continuing to the next. This cmdlet is perfect for creating visual presentations or exploring multiple locations in sequence.

## EXAMPLES

### Example 1: Simple tour of Japanese cities
```powershell
Start-OpenStreetMapTour Tokyo, Osaka, Kyoto
```

Creates an animated tour visiting Tokyo, Osaka, and Kyoto with default settings (1.5s animation, 2s pause at each location).

### Example 2: Tour with custom timing
```powershell
Start-OpenStreetMapTour Paris, London, Berlin -Duration 3 -PauseTime 2.5
```

Creates a tour with slower animations (3 seconds) and longer pauses (2.5 seconds) at each location.

### Example 3: Tour of coordinates
```powershell
Start-OpenStreetMapTour -Location "35.6762,139.6503", "34.6937,135.5023", "35.0116,135.7681"
```

Tours through specific coordinates (Tokyo Tower, Osaka, Kyoto) using coordinate strings.

### Example 4: Fast-paced tour with many locations
```powershell
$cities = "Tokyo", "Yokohama", "Nagoya", "Kyoto", "Osaka", "Kobe", "Hiroshima", "Fukuoka"
Start-OpenStreetMapTour -Location $cities -Duration 1 -PauseTime 1.5 -Zoom 12
```

Creates a fast-paced tour through 8 Japanese cities with quick transitions.

### Example 5: Tour from pipeline
```powershell
"New York", "Chicago", "Los Angeles", "Miami" | Start-OpenStreetMapTour -Duration 2 -PauseTime 2.5
```

Pipes location names to create a tour of major US cities.

### Example 6: Tour with detailed descriptions
```powershell
$tourStops = @(
    @{ Location = "Tokyo"; Description = "🗼 Tokyo - Capital of Japan with 14 million people. Home to Tokyo Tower and Shibuya Crossing." }
    @{ Location = "Mount Fuji"; Description = "🗻 Mount Fuji - Japan's tallest mountain at 3,776m. Sacred symbol of Japan." }
    @{ Location = "Kyoto"; Description = "⛩️ Kyoto - Ancient capital with over 2,000 temples, shrines, and traditional gardens." }
    @{ Location = "Osaka"; Description = "🏯 Osaka - Japan's kitchen. Famous for street food and Osaka Castle." }
)
Start-OpenStreetMapTour -Locations $tourStops -Duration 2 -PauseTime 3
```

Creates an informative tour with detailed descriptions displayed at each stop.

### Example 7: 3D mountain tour
```powershell
$mountains = @(
    @{ Location = "Mount Fuji"; Description = "🗻 Mount Fuji - 3,776m" }
    @{ Location = "45.9763,7.6586"; Description = "🏔️ Matterhorn - 4,478m" }
    @{ Location = "27.9881,86.9250"; Description = "⛰️ Mount Everest - 8,849m" }
)
Start-OpenStreetMapTour -Locations $mountains -Enable3D -Pitch 70 -Zoom 12 -Duration 3 -PauseTime 4
```

Creates a dramatic 3D tour of famous mountains with elevation visualization.

## PARAMETERS

### -Location
Specifies an array of locations to visit in the tour. Each location can be a place name (e.g., "Tokyo") or a coordinate string in "latitude,longitude" format (e.g., "35.6586,139.7454"). The tour visits locations in the order specified.

```yaml
Type: String[]
Parameter Sets: Simple
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
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

### -Bearing
Specifies the camera bearing (rotation) in degrees (0-360) for all locations in the tour. The bearing represents the compass direction the camera is pointing:
- 0 degrees = North (default)
- 90 degrees = East
- 180 degrees = South
- 270 degrees = West

This parameter applies to all tour stops. Combining this with -Pitch creates a consistent viewing angle throughout the tour.

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
Enables 3D visualization of buildings and terrain for all locations in the tour. When enabled:
- Buildings are rendered as 3D extruded shapes (at zoom level 14 and above)
- Terrain elevation data is displayed with appropriate exaggeration for visibility
- Default pitch angle is set to 60 degrees for optimal 3D viewing
- All tour stops maintain the same 3D perspective

The 3D view can be toggled on/off using the browser interface during the tour.

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

### -Pitch
Specifies the camera pitch (tilt angle) in degrees (0-85) for all locations in the tour. The pitch controls how much the camera is tilted:
- 0 degrees = Top-down view (default for 2D maps)
- 60 degrees = Default for 3D view (automatically set when -Enable3D is used without explicit -Pitch)
- 85 degrees = Almost horizontal view

This parameter applies to all tour stops, creating a consistent viewing angle. Higher pitch values provide a more dramatic 3D perspective.

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

### System.String[]
You can pipe an array of location strings to this cmdlet.

## OUTPUTS

### PowerShell.Map.Server.MapMarker
Returns a MapMarker object for each location visited, including Step, TotalSteps, Location, Latitude, Longitude, Label, Status, and GeocodingSource properties.

## NOTES
- The tour runs sequentially and blocks until all locations have been visited
- Each location is geocoded if it's a place name
- Coordinates can be specified in "latitude,longitude" format
- The total time for the tour is: (Duration + PauseTime) × Number of Locations
- The map server runs on http://localhost:8765/
- Geocoding uses the Nominatim API with a 1 request per second rate limit
- Failed locations are skipped with a warning, and the tour continues

## RELATED LINKS

[Show-OpenStreetMap](Show-OpenStreetMap.md)
[Show-OpenStreetMapRoute](Show-OpenStreetMapRoute.md)
[OpenStreetMap](https://www.openstreetmap.org/)
