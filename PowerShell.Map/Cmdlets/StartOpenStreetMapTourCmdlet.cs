using System.Management.Automation;
using PowerShell.Map.Helpers;
using PowerShell.Map.Server;

namespace PowerShell.Map.Cmdlets;

/// <summary>
/// Creates an animated tour that visits multiple locations sequentially
/// </summary>
[Cmdlet(VerbsLifecycle.Start, "OpenStreetMapTour")]
[OutputType(typeof(MapMarker))]
public class StartOpenStreetMapTourCmdlet : MapCmdletBase
{
    /// <summary>
    /// Array of locations to visit in the tour (place names or coordinates).
    /// Use simple strings for locations without descriptions.
    /// </summary>
    [Parameter(ParameterSetName = "Simple", Position = 0, Mandatory = true, ValueFromPipeline = true)]
    public string[]? Location { get; set; }

    /// <summary>
    /// Array of structured locations with optional descriptions.
    /// Each element should be a hashtable with 'Location' and optional 'Description' keys.
    /// Example: @{ Location = "Tokyo"; Description = "Capital of Japan" }
    /// </summary>
    [Parameter(ParameterSetName = "WithDescription", Mandatory = true, ValueFromPipeline = true)]
    public object[]? Locations { get; set; }

    /// <summary>
    /// Zoom level for each location (1-19, default: 13)
    /// </summary>
    [Parameter]
    [ValidateRange(1, 19)]
    public int Zoom { get; set; } = 13;

    /// <summary>
    /// Time to pause at each location in seconds (0.5-30.0, default: 2.0)
    /// </summary>
    [Parameter]
    [ValidateRange(0.5, 30.0)]
    public double PauseTime { get; set; } = 2.0;

    /// <summary>
    /// Animation duration for transitions between locations in seconds (0.1-10.0, default: 1.5)
    /// </summary>
    [Parameter]
    [ValidateRange(0.1, 10.0)]
    public double Duration { get; set; } = 1.5;

    /// <summary>
    /// Enable 3D display (buildings and terrain)
    /// If neither Enable3D nor Disable3D is specified, preserves current 3D state
    /// </summary>
    [Parameter]
    public SwitchParameter Enable3D { get; set; }

    /// <summary>
    /// Disable 3D display (return to 2D top-down view)
    /// If neither Enable3D nor Disable3D is specified, preserves current 3D state
    /// </summary>
    [Parameter]
    public SwitchParameter Disable3D { get; set; }


    /// <summary>
    /// Camera bearing in degrees (0-360, 0=North, 90=East, 180=South, 270=West)
    /// If not specified, preserves current bearing
    /// </summary>
    [Parameter]
    [ValidateRange(0, 360)]
    public double? Bearing { get; set; }

    /// <summary>
    /// Camera pitch in degrees (0-85, 0=top-down view, 60=default for 3D, 85=almost horizontal)
    /// If not specified, preserves current pitch
    /// </summary>
    [Parameter]
    [ValidateRange(0, 85)]
    public double? Pitch { get; set; }

    // /// <summary>
    // /// Enable debug mode to show detailed logging
    // /// </summary>
    // [Parameter]
    private SwitchParameter DebugMode { get; set; }

    private readonly List<TourLocation> _tourLocations = new();

    protected override void ProcessRecord()
    {
        // Simple parameter set (just location strings)
        if (Location != null)
        {
            foreach (var loc in Location)
            {
                _tourLocations.Add(new TourLocation { Location = loc });
            }
        }
        
        // WithDescription parameter set (structured objects)
        if (Locations != null)
        {
            foreach (var item in Locations)
            {
                TourLocation? tourLoc = null;
                
                // Handle Hashtable
                if (item is System.Collections.Hashtable ht)
                {
                    tourLoc = new TourLocation
                    {
                        Location = ht["Location"]?.ToString() ?? string.Empty,
                        Description = ht["Description"]?.ToString(),
                        Label = ht["Label"]?.ToString(),
                        Color = ht["Color"]?.ToString()
                    };
                }
                // Handle PSObject
                else if (item is PSObject psObj)
                {
                    tourLoc = new TourLocation
                    {
                        Location = psObj.Properties["Location"]?.Value?.ToString() ?? string.Empty,
                        Description = psObj.Properties["Description"]?.Value?.ToString(),
                        Label = psObj.Properties["Label"]?.Value?.ToString(),
                        Color = psObj.Properties["Color"]?.Value?.ToString()
                    };
                }
                
                if (tourLoc != null && !string.IsNullOrEmpty(tourLoc.Location))
                {
                    _tourLocations.Add(tourLoc);
                }
            }
        }
    }

    protected override void EndProcessing()
    {
        if (_tourLocations.Count == 0)
        {
            WriteWarning("No locations provided for tour");
            return;
        }

        try
        {
            var server = MapServer.Instance;
            
            // Convert Enable3D/Disable3D switches to nullable bool
            bool? enable3D = null;
            if (Enable3D && Disable3D)
            {
                WriteError(new ErrorRecord(
                    new ArgumentException("Cannot specify both -Enable3D and -Disable3D"),
                    "MutuallyExclusiveParameters",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }
            else if (Enable3D)
            {
                enable3D = true;
            }
            else if (Disable3D)
            {
                enable3D = false;
            }
            // If neither is specified, enable3D remains null (preserve current state)

            WriteVerbose($"Starting map tour with {_tourLocations.Count} locations");
            for (int i = 0; i < _tourLocations.Count; i++)
            {
                var tourLocation = _tourLocations[i];
                WriteVerbose($"[{i + 1}/{_tourLocations.Count}] Visiting: {tourLocation.Location}");

                if (!LocationHelper.TryParseLocation(tourLocation.Location, out double lat, out double lon,
                    msg => WriteVerbose(msg), msg => WriteWarning(msg)))
                {
                    WriteWarning($"Could not parse location: {tourLocation.Location}, skipping");
                    
                    // Output failed marker
                    WriteObject(new MapMarker
                    {
                        Step = i + 1,
                        TotalSteps = _tourLocations.Count,
                        Location = tourLocation.Location,
                        Label = !string.IsNullOrEmpty(tourLocation.Label) ? tourLocation.Label : tourLocation.Location,
                        Color = GetMarkerColor(tourLocation.Color),
                        Description = tourLocation.Description,
                        Status = "Failed",
                        GeocodingSource = "Unknown"
                    });
                    continue;
                }

                // Determine label: use provided label, or location name, or reverse geocoded name
                string label;
                if (!string.IsNullOrEmpty(tourLocation.Label))
                {
                    label = tourLocation.Label;
                }
                else if (tourLocation.Location.Contains(','))
                {
                    // Try reverse geocoding for coordinates
                    if (LocationHelper.TryReverseGeocode(lat, lon, out string? reversedName, 
                        msg => WriteVerbose(msg)))
                    {
                        label = reversedName!;
                    }
                    else
                    {
                        label = tourLocation.Location;
                    }
                }
                else
                {
                    label = tourLocation.Location;
                }
                // Move to location with animation (always animated in tour mode)
                ExecuteWithRetry(server, () => server.UpdateMap(lat, lon, Zoom, label, GetMarkerColor(tourLocation.Color), DebugMode, Duration, enable3D, Bearing, Pitch, tourLocation.Description));
                
                // Output progress info
                WriteObject(new MapMarker
                {
                    Step = i + 1,
                    TotalSteps = _tourLocations.Count,
                    Location = tourLocation.Location,
                    Latitude = lat,
                    Longitude = lon,
                    Label = label,
                    Color = GetMarkerColor(tourLocation.Color),
                    Description = tourLocation.Description,
                    Status = "Success",
                    GeocodingSource = tourLocation.Location.Contains(',') ? "Coordinates" : "Nominatim"
                });
                
                // Wait at location (transition + pause time)
                var totalWaitMs = (int)((Duration + PauseTime) * 1000);
                System.Threading.Thread.Sleep(totalWaitMs);
            }

            WriteVerbose("Tour completed successfully");
        }
        catch (Exception ex)
        {
            WriteError(new ErrorRecord(
                ex,
                "TourFailed",
                ErrorCategory.NotSpecified,
                null));
        }
    }
}

