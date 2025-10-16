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

    private const int BrowserConnectionWaitMs = 2000;

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
        // Check if browser window exists first
        bool windowExists = server.IsBrowserWindowOpen();
        WriteVerbose($"Browser window check: {(windowExists ? "exists" : "not found")}");
        
        // Try to update the map first
        bool success = updateAction();
        WriteVerbose($"First update attempt: {(success ? "succeeded" : "failed")}");

        // If window doesn't exist, open browser regardless of updateAction result
        if (!windowExists)
        {
            WriteVerbose("No browser window detected, opening browser");
            LocationHelper.OpenBrowser(server.Url, msg => WriteWarning(msg));
            System.Threading.Thread.Sleep(BrowserConnectionWaitMs);
            success = updateAction();
            WriteVerbose($"Update after opening browser: {(success ? "succeeded" : "failed")}");
            
            if (!success)
            {
                WriteVerbose("Still no connection after opening browser, retrying once more");
                System.Threading.Thread.Sleep(BrowserConnectionWaitMs);
                success = updateAction();
                WriteVerbose($"Final retry: {(success ? "succeeded" : "failed")}");
            }
        }
        // If window exists but update failed, wait and retry
        else if (!success)
        {
            WriteVerbose("Browser window exists but SSE not connected yet, waiting...");
            System.Threading.Thread.Sleep(BrowserConnectionWaitMs);
            success = updateAction();
            WriteVerbose($"Update after waiting: {(success ? "succeeded" : "failed")}");
            
            if (!success)
            {
                WriteVerbose("Still no connection, retrying once more");
                System.Threading.Thread.Sleep(BrowserConnectionWaitMs);
                success = updateAction();
                WriteVerbose($"Final retry: {(success ? "succeeded" : "failed")}");
            }
        }
        
        if (!success)
        {
            WriteWarning("Failed to update map - browser may not have opened correctly.");
        }
        else
        {
            WriteVerbose("Map updated successfully");
        }
    }
}
