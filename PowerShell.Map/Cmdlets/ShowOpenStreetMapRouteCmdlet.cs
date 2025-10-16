using System.Management.Automation;
using System.Text.Json;
using PowerShell.Map.Helpers;
using PowerShell.Map.Server;

namespace PowerShell.Map.Cmdlets;

/// <summary>
/// Displays a route between two locations on an interactive map.
/// </summary>
[Cmdlet(VerbsCommon.Show, "OpenStreetMapRoute")]
public class ShowOpenStreetMapRouteCmdlet : MapCmdletBase
{
    /// <summary>
    /// Starting location (place name or "latitude,longitude" format)
    /// </summary>
    [Parameter(Position = 0, Mandatory = true)]
    public string? From { get; set; }

    /// <summary>
    /// Destination location (place name or "latitude,longitude" format)
    /// </summary>
    [Parameter(Position = 1, Mandatory = true)]
    public string? To { get; set; }

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
    /// Enable debug mode to show detailed logging
    /// </summary>
    [Parameter]
    public SwitchParameter DebugMode { get; set; }

    protected override void ProcessRecord()
    {
        try
        {
            var server = MapServer.Instance;

            // Parse From location
            if (!LocationHelper.TryParseLocation(From!, out double fromLat, out double fromLon,
                msg => WriteVerbose(msg), msg => WriteWarning(msg)))
            {
                WriteError(new ErrorRecord(
                    new ArgumentException($"Invalid From location: {From}. Use 'latitude,longitude' format or a place name."),
                    "InvalidFromLocation",
                    ErrorCategory.InvalidArgument,
                    From));
                return;
            }

            // Parse To location
            if (!LocationHelper.TryParseLocation(To!, out double toLat, out double toLon,
                msg => WriteVerbose(msg), msg => WriteWarning(msg)))
            {
                WriteError(new ErrorRecord(
                    new ArgumentException($"Invalid To location: {To}. Use 'latitude,longitude' format or a place name."),
                    "InvalidToLocation",
                    ErrorCategory.InvalidArgument,
                    To));
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
            ExecuteWithRetry(server, () => server.UpdateRoute(fromLat, fromLon, toLat, toLon, 
                routeCoordinates, Color, Width, DebugMode, From, To));
            WriteVerbose("Map updated with route");
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
            var url = $"http://router.project-osrm.org/route/v1/driving/{fromLon},{fromLat};{toLon},{toLat}?overview=full&geometries=geojson";
            
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
                            coordList.Add(new[] { values[0].GetDouble(), values[1].GetDouble() });
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
