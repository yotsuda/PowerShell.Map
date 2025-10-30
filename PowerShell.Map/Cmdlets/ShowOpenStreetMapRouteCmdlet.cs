using System.Collections;
using System.Management.Automation;
using System.Text.Json;
using PowerShell.Map.Helpers;
using PowerShell.Map.Server;

namespace PowerShell.Map.Cmdlets;

/// <summary>
/// Displays a route between two locations on an interactive map.
/// </summary>
[Cmdlet(VerbsCommon.Show, "OpenStreetMapRoute")]
[OutputType(typeof(MapMarker))]
public class ShowOpenStreetMapRouteCmdlet : MapCmdletBase
{
    /// <summary>
    /// Starting location (place name, "latitude,longitude" format, or hashtable with Location and Description)
    /// Example: "Tokyo" or @{ Location = "Tokyo"; Description = "Capital of Japan" }
    /// </summary>
    [Parameter(Position = 0, Mandatory = true)]
    public object? From { get; set; }

    /// <summary>
    /// Destination location (place name, "latitude,longitude" format, or hashtable with Location and Description)
    /// Example: "Osaka" or @{ Location = "Osaka"; Description = "Second largest city" }
    /// </summary>
    [Parameter(Position = 1, Mandatory = true)]
    public object? To { get; set; }

    /// <summary>
    /// Route line color (color name or hex code, default: #0066ff)
    /// </summary>
    [Parameter]
    public string Color { get; set; } = "#0066ff";

    /// <summary>
    /// Route line width in pixels (1-10, default: 4)
    /// </summary>
    [Parameter]
    [ValidateRange(1, 10)]
    public int Width { get; set; } = 4;

    /// <summary>
    /// Zoom level (1-19). If not specified, automatically calculated based on route distance
    /// </summary>
    [Parameter]
    [ValidateRange(1, 19)]
    public int? Zoom { get; set; }

    /// <summary>
    /// Animation duration in seconds (0.0-10.0, default: 1.0)
    /// Set to 0 for instant movement without animation
    /// </summary>
    [Parameter]
    [ValidateRange(0.0, 10.0)]
    public double Duration { get; set; } = 1.0;

    /// <summary>
    /// Routing profile (driving, walking, cycling, default: driving)
    /// </summary>
    [Parameter]
    [ValidateSet("driving", "walking", "cycling", IgnoreCase = true)]
    public string Profile { get; set; } = "driving";

    /// <summary>
    /// Enable 3D display (buildings and terrain)
    /// </summary>
    [Parameter]
    public SwitchParameter Enable3D { get; set; }

    /// <summary>
    /// Camera bearing in degrees (0-360, 0=North, 90=East, 180=South, 270=West)
    /// </summary>
    [Parameter]
    [ValidateRange(0, 360)]
    public double Bearing { get; set; } = 0;

    /// <summary>
    /// Camera pitch in degrees (0-85, 0=top-down view, 60=default for 3D, 85=almost horizontal)
    /// </summary>
    [Parameter]
    [ValidateRange(0, 85)]
    public double Pitch { get; set; } = 0;
    // /// <summary>
    // /// Enable debug mode to show detailed logging
    // /// </summary>
    // [Parameter]
    private SwitchParameter DebugMode { get; set; }

    protected override void ProcessRecord()
    {
        try
        {
            var server = MapServer.Instance;

            // Parse From parameter (string or hashtable)
            string fromLocation;
            string? fromDescription = null;
            string? fromLabel = null;
            string? fromColor = null;
            
            if (From is System.Collections.Hashtable fromHt)
            {
                fromLocation = fromHt["Location"]?.ToString() ?? string.Empty;
                fromDescription = fromHt["Description"]?.ToString();
                fromLabel = fromHt["Label"]?.ToString();
                fromColor = fromHt["Color"]?.ToString();
            }
            else if (From is PSObject fromPsObj)
            {
                fromLocation = fromPsObj.Properties["Location"]?.Value?.ToString() ?? string.Empty;
                fromDescription = fromPsObj.Properties["Description"]?.Value?.ToString();
                fromLabel = fromPsObj.Properties["Label"]?.Value?.ToString();
                fromColor = fromPsObj.Properties["Color"]?.Value?.ToString();
            }
            else
            {
                fromLocation = From?.ToString() ?? string.Empty;
            }

            // Parse To parameter (string or hashtable)
            string toLocation;
            string? toDescription = null;
            string? toLabel = null;
            string? toColor = null;
            
            if (To is System.Collections.Hashtable toHt)
            {
                toLocation = toHt["Location"]?.ToString() ?? string.Empty;
                toDescription = toHt["Description"]?.ToString();
                toLabel = toHt["Label"]?.ToString();
                toColor = toHt["Color"]?.ToString();
            }
            else if (To is PSObject toPsObj)
            {
                toLocation = toPsObj.Properties["Location"]?.Value?.ToString() ?? string.Empty;
                toDescription = toPsObj.Properties["Description"]?.Value?.ToString();
                toLabel = toPsObj.Properties["Label"]?.Value?.ToString();
                toColor = toPsObj.Properties["Color"]?.Value?.ToString();
            }
            else
            {
                toLocation = To?.ToString() ?? string.Empty;
            }


            // Automatically enable 3D mode if Bearing or Pitch is specified
            if ((Bearing != 0 || Pitch != 0) && !Enable3D)
            {
                Enable3D = true;
                WriteVerbose("3D mode automatically enabled due to Bearing/Pitch parameters");
            }

            // Parse From location
            if (!LocationHelper.TryParseLocation(fromLocation, out double fromLat, out double fromLon,
                msg => WriteVerbose(msg), msg => WriteWarning(msg)))
            {
                var failedFromMarker = new MapMarker
                {
                    Location = fromLocation,
                    Label = !string.IsNullOrEmpty(fromLabel) ? fromLabel : fromLocation,
                    Description = fromDescription,
                    Color = GetMarkerColor(fromColor),
                    Status = "Failed",
                    GeocodingSource = "Unknown"
                };
                WriteObject(failedFromMarker);
                
                WriteError(new ErrorRecord(
                    new ArgumentException($"Invalid From location: {fromLocation}. Use 'latitude,longitude' format or a place name."),
                    "InvalidFromLocation",
                    ErrorCategory.InvalidArgument,
                    fromLocation));
                return;
            }

            // Parse To location
            if (!LocationHelper.TryParseLocation(toLocation, out double toLat, out double toLon,
                msg => WriteVerbose(msg), msg => WriteWarning(msg)))
            {
                // Output successful From marker
                // Output successful From marker (To parsing failed, so we can't do full label logic yet)
                var successFromMarker = new MapMarker
                {
                    Latitude = fromLat,
                    Longitude = fromLon,
                    Label = !string.IsNullOrEmpty(fromLabel) ? fromLabel : fromLocation,
                    Description = fromDescription,
                    Color = GetMarkerColor(fromColor),
                    Location = fromLocation,
                    Status = "Success",
                    GeocodingSource = CoordinateValidator.IsCoordinateString(fromLocation) ? "Coordinates" : "Nominatim"
                };
                WriteObject(successFromMarker);
                
                // Output failed To marker
                var failedToMarker = new MapMarker
                {
                    Location = toLocation,
                    Label = !string.IsNullOrEmpty(toLabel) ? toLabel : toLocation,
                    Description = toDescription,
                    Color = GetMarkerColor(toColor),
                    Status = "Failed",
                    GeocodingSource = "Unknown"
                };
                WriteObject(failedToMarker);
                
                WriteError(new ErrorRecord(
                    new ArgumentException($"Invalid To location: {toLocation}. Use 'latitude,longitude' format or a place name."),
                    "InvalidToLocation",
                    ErrorCategory.InvalidArgument,
                    toLocation));
                return;
            }

            WriteVerbose($"Route from ({fromLat}, {fromLon}) to ({toLat}, {toLon})");

            // Get route from OSRM API
            var routeCoordinates = GetRouteFromOSRM(fromLon, fromLat, toLon, toLat);
            if (routeCoordinates == null || routeCoordinates.Length == 0)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException("Failed to get route from OSRM API. Check network connectivity and location validity."),
                    "RouteNotFound",
                    ErrorCategory.NotSpecified,
                    null));
                return;
            }

            WriteVerbose($"Route retrieved with {routeCoordinates.Length} coordinate points");

            // Update map with route
            // Determine From label: use provided label, or add emoji to location name
            string finalFromLabel;
            if (!string.IsNullOrEmpty(fromLabel))
            {
                // User explicitly specified label - use as-is
                finalFromLabel = fromLabel;
            }
            else if (CoordinateValidator.IsCoordinateString(fromLocation))
            {
                // 座標の場合、逆ジオコーディングを試みる
                if (LocationHelper.TryReverseGeocode(fromLat, fromLon, out string? reversedName, msg => WriteVerbose(msg)))
                {
                    finalFromLabel = $"\U0001F680 {reversedName}";
                    WriteVerbose($"From: Using reverse geocoded name with emoji: {finalFromLabel}");
                }
                else
                {
                    finalFromLabel = $"\U0001F680 {fromLat:F6},{fromLon:F6}";
                    WriteVerbose($"From: Reverse geocoding failed, using coordinates with emoji as label");
                }
            }
            else
            {
                finalFromLabel = $"\U0001F680 {fromLocation}";
            }
            
            // Determine To label: use provided label, or add emoji to location name
            string finalToLabel;
            if (!string.IsNullOrEmpty(toLabel))
            {
                // User explicitly specified label - use as-is
                finalToLabel = toLabel;
            }
            else if (CoordinateValidator.IsCoordinateString(toLocation))
            {
                // 座標の場合、逆ジオコーディングを試みる
                if (LocationHelper.TryReverseGeocode(toLat, toLon, out string? reversedName, msg => WriteVerbose(msg)))
                {
                    finalToLabel = $"\U0001F3AF {reversedName}";
                    WriteVerbose($"To: Using reverse geocoded name with emoji: {finalToLabel}");
                }
                else
                {
                    finalToLabel = $"\U0001F3AF {toLat:F6},{toLon:F6}";
                    WriteVerbose($"To: Reverse geocoding failed, using coordinates with emoji as label");
                }
            }
            else
            {
                finalToLabel = $"\U0001F3AF {toLocation}";
            }
            
            ExecuteWithRetry(server, () => server.UpdateRoute(fromLat, fromLon, toLat, toLon, 
                routeCoordinates, Color, Width, Zoom, false, finalFromLabel, finalToLabel, Duration, Enable3D, Bearing, Pitch, fromDescription, toDescription));

            WriteVerbose("Map updated with route");
            
            // Output From marker
            var fromMarker = new MapMarker
            {
                Latitude = fromLat,
                Longitude = fromLon,
                Label = finalFromLabel,
                Description = fromDescription,
                Color = GetMarkerColor(fromColor),
                Location = fromLocation,
                Status = "Success",
                GeocodingSource = CoordinateValidator.IsCoordinateString(fromLocation) ? "Coordinates" : "Nominatim"
            };
            WriteObject(fromMarker);
            
            // Output To marker
            var toMarker = new MapMarker
            {
                Latitude = toLat,
                Longitude = toLon,
                Label = finalToLabel,
                Description = toDescription,
                Color = GetMarkerColor(toColor),
                Location = toLocation,
                Status = "Success",
                GeocodingSource = CoordinateValidator.IsCoordinateString(toLocation) ? "Coordinates" : "Nominatim"
            };

            WriteObject(toMarker);
        }
        catch (Exception ex)
        {
            WriteError(new ErrorRecord(
                ex,
                "ShowMapRouteFailed",
                ErrorCategory.NotSpecified,
                null));
        }
    }

    /// <summary>
    /// Get route coordinates from OSRM (Open Source Routing Machine) API
    /// </summary>
    /// <returns>Array of [longitude, latitude] coordinate pairs, or null if failed</returns>
    private double[][]? GetRouteFromOSRM(double fromLon, double fromLat, double toLon, double toLat)
    {
        try
        {
            // Use Profile parameter in lowercase for URL
            var profile = Profile.ToLower();
            var url = $"http://router.project-osrm.org/route/v1/{profile}/{fromLon},{fromLat};{toLon},{toLat}?overview=full&geometries=geojson";
            
            WriteVerbose($"Fetching route from OSRM API: {url}");
            var response = HttpClientFactory.RoutingClient.GetStringAsync(url).GetAwaiter().GetResult();
            
            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;
            
            // Parse GeoJSON response
            if (root.TryGetProperty("routes", out var routes) && routes.GetArrayLength() > 0)
            {
                var route = routes[0];
                if (route.TryGetProperty("geometry", out var geometry) &&
                    geometry.TryGetProperty("coordinates", out var coordinates))
                {
                    var coordList = new List<double[]>();
                    foreach (var coord in coordinates.EnumerateArray())
                    {
                        var values = coord.EnumerateArray().ToArray();
                        if (values.Length >= 2)
                        {
                            // GeoJSON format: [longitude, latitude]
                            coordList.Add([values[0].GetDouble(), values[1].GetDouble()]);
                        }
                    }
                    
                    WriteVerbose($"Successfully parsed {coordList.Count} coordinate points from OSRM response");
                    return coordList.ToArray();
                }
            }
            
            WriteWarning("No valid route found in OSRM response");
            return null;
        }
        catch (Exception ex)
        {
            WriteWarning($"Failed to get route from OSRM: {ex.Message}");
            return null;
        }
    }

}
