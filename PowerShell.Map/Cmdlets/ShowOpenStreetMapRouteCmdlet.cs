using System.Management.Automation;
using System.Text.Json;
using PowerShell.Map.Helpers;
using PowerShell.Map.Server;

namespace PowerShell.Map.Cmdlets;

[Cmdlet(VerbsCommon.Show, "OpenStreetMapRoute")]
public class ShowOpenStreetMapRouteCmdlet : PSCmdlet
{
    [Parameter(Position = 0, Mandatory = true)]
    public string? From { get; set; }

    [Parameter(Position = 1, Mandatory = true)]
    public string? To { get; set; }

    [Parameter]
    public string Color { get; set; } = "#0066ff";

    [Parameter]
    [ValidateRange(1, 10)]
    public int Width { get; set; } = 4;

    [Parameter]
    public SwitchParameter DebugMode { get; set; }

    private const int BrowserConnectionWaitMs = 500;

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
                    new ArgumentException($"Invalid From location: {From}"),
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
                    new ArgumentException($"Invalid To location: {To}"),
                    "InvalidToLocation",
                    ErrorCategory.InvalidArgument,
                    To));
                return;
            }

            WriteVerbose($"Route from ({fromLat}, {fromLon}) to ({toLat}, {toLon})");

            // Get route from OSRM
            var routeCoordinates = GetRoute(fromLon, fromLat, toLon, toLat);
            if (routeCoordinates == null || routeCoordinates.Length == 0)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException("Failed to get route from OSRM API"),
                    "RouteNotFound",
                    ErrorCategory.NotSpecified,
                    null));
                return;
            }

            WriteVerbose($"Route retrieved with {routeCoordinates.Length} points");

            ExecuteWithRetry(server, () => server.UpdateRoute(fromLat, fromLon, toLat, toLon, routeCoordinates, Color, Width, DebugMode, From, To));
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

    private double[][]? GetRoute(double fromLon, double fromLat, double toLon, double toLat)
    {
        try
        {
            var url = $"http://router.project-osrm.org/route/v1/driving/{fromLon},{fromLat};{toLon},{toLat}?overview=full&geometries=geojson";
            
            WriteVerbose($"Fetching route from OSRM: {url}");
            var response = HttpClientFactory.RoutingClient.GetStringAsync(url).GetAwaiter().GetResult();
            
            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;
            
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
                            coordList.Add(new[] { values[0].GetDouble(), values[1].GetDouble() });
                        }
                    }
                    return coordList.ToArray();
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            WriteWarning($"Failed to get route: {ex.Message}");
            return null;
        }
    }

    private void ExecuteWithRetry(MapServer server, Func<bool> updateAction)
    {
        bool success = updateAction();

        if (!success)
        {
            WriteVerbose("Update failed (no active clients), opening browser and retrying");
            LocationHelper.OpenBrowser(server.Url, msg => WriteWarning(msg));
            System.Threading.Thread.Sleep(BrowserConnectionWaitMs);
            updateAction();
            WriteVerbose("Resent map update to new browser tab");
        }
    }
}
