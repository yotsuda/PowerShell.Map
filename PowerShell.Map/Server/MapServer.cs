using System.Net;
using System.Text;
using System.Text.Json;
using PowerShell.Map.Helpers;

namespace PowerShell.Map.Server;

public class MapServer
{
    private static readonly MapServer _instance = new();
    public static MapServer Instance => _instance;
    private static readonly object _lock = new();
    private HttpListener? _listener;
    private CancellationTokenSource? _cts;
    private MapState _currentState;
    private DateTime _lastClientAccessTime;
    private readonly List<StreamWriter> _sseClients = [];

    private MapServer()
    {
        _currentState = new MapState { Latitude = 35.6586, Longitude = 139.7454, Zoom = 13, DebugMode = false };
        _lastClientAccessTime = DateTime.MinValue;

        // Ensure cleanup on process exit
        AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
        {
            Stop();
        };
    }

    public string Url { get; private set; } = "http://localhost:8765/";
    public DateTime LastClientAccessTime => _lastClientAccessTime;

    public bool HasActiveClients()
    {
        lock (_lock)
        {
            return _sseClients.Count > 0;
        }
    }

    public bool IsBrowserWindowOpen()
    {
        // Check if any browser window has "PowerShell.Map" in its title
        return WindowHelper.IsBrowserTabOpen("PowerShell.Map");
    }

    public void Start()
    {
        if (_listener != null) return;

        try
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(Url);
            _listener.Start();
            
            _cts = new CancellationTokenSource();
            
            Task.Run(() => HandleRequests(_cts.Token), _cts.Token);
        }
        catch (HttpListenerException)
        {
            // Port already in use by another process/instance
            // This is OK - just means server is already running
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
        
        // Safely stop and close the HttpListener
        if (_listener != null)
        {
            bool shouldStop = false;
            bool shouldClose = false;
            
            // Check if listener is still active (this itself might throw if disposed)
            try
            {
                shouldStop = _listener.IsListening;
                shouldClose = true;  // If IsListening succeeds, we can try Close()
            }
            catch (ObjectDisposedException)
            {
                // Already disposed - nothing to do
            }
            catch
            {
                // Any other error - skip Stop/Close
            }
            
            if (shouldStop)
            {
                try
                {
                    _listener.Stop();
                }
                catch
                {
                    // Ignore any errors during Stop()
                }
            }
            
            if (shouldClose)
            {
                try
                {
                    _listener.Close();
                }
                catch
                {
                    // Ignore any errors during Close()
                }
            }
            
            _listener = null;
        }
        
        // Close all SSE connections
        lock (_lock)
        {
            foreach (var client in _sseClients)
            {
                try { client.Close(); } catch { }
            }
            _sseClients.Clear();
        }
    }

    public MapState GetCurrentState()
    {
        lock (_lock)
        {
            return new MapState
            {
                Latitude = _currentState.Latitude,
                Longitude = _currentState.Longitude,
                Zoom = _currentState.Zoom,
                Marker = _currentState.Marker,
                DebugMode = _currentState.DebugMode,
                Markers = _currentState.Markers
            };
        }
    }

    public bool UpdateMap(double latitude, double longitude, int zoom, string? marker = null, bool debugMode = false, double duration = 1.0)
    {
        lock (_lock)
        {
            _currentState = new MapState
            {
                Latitude = latitude,
                Longitude = longitude,
                Zoom = zoom,
                Marker = marker,
                DebugMode = debugMode,
                Animate = duration > 0,  // Auto-enable animation if duration is specified
                Duration = duration
            };
        }
        
        return NotifyClients();
    }

    public bool UpdateMapWithMarkers(MapMarker[] markers, int? zoom = null, bool debugMode = false)
    {
        lock (_lock)
        {
            if (markers == null || markers.Length == 0)
            {
                return false;
            }

            // Filter markers with valid coordinates
            var validCoordMarkers = markers.Where(m => m.Latitude.HasValue && m.Longitude.HasValue);
            if (!validCoordMarkers.Any())
            {
                return false;
            }

            // Calculate center point and bounds
            var minLat = validCoordMarkers.Min(m => m.Latitude!.Value);
            var maxLat = validCoordMarkers.Max(m => m.Latitude!.Value);
            var minLon = validCoordMarkers.Min(m => m.Longitude!.Value);
            var maxLon = validCoordMarkers.Max(m => m.Longitude!.Value);
            
            var centerLat = (minLat + maxLat) / 2;
            var centerLon = (minLon + maxLon) / 2;
            
            // Calculate appropriate zoom if not specified
            int calculatedZoom = zoom ?? CalculateOptimalZoom(maxLat - minLat, maxLon - minLon);
            
            _currentState = new MapState
            {
                Latitude = centerLat,
                Longitude = centerLon,
                Zoom = calculatedZoom,
                Markers = markers,
                DebugMode = debugMode
            };
        }
        
        return NotifyClients();
    }

    public bool UpdateRoute(double fromLat, double fromLon, double toLat, double toLon,
                           double[][] routeCoordinates, string? color = null, int width = 4, 
                           int? zoom = null, bool debugMode = false,
                           string? fromLocation = null, string? toLocation = null, 
                           double duration = 1.0)
    {
        lock (_lock)
        {
            // Calculate center point and appropriate zoom
            var centerLat = (fromLat + toLat) / 2;
            var centerLon = (fromLon + toLon) / 2;
            
            // Use specified zoom or calculate appropriate zoom based on distance
            int targetZoom = zoom ?? CalculateOptimalZoom(Math.Abs(fromLat - toLat), Math.Abs(fromLon - toLon));
            // Create route markers for From and To locations
            var routeMarkers = new[]
            {
                new MapMarker { Latitude = fromLat, Longitude = fromLon, Label = $"🚀 {fromLocation ?? "Start"}", Color = "green" },
                new MapMarker { Latitude = toLat, Longitude = toLon, Label = $"🎯 {toLocation ?? "Goal"}", Color = "red" }
            };
            
            _currentState = new MapState
            {
                Latitude = centerLat,
                Longitude = centerLon,
                Zoom = targetZoom,
                RouteCoordinates = routeCoordinates,
                RouteColor = color ?? "#0066ff",
                RouteWidth = width,
                RouteMarkers = routeMarkers,
                DebugMode = debugMode,
                Animate = duration > 0,  // Auto-enable animation if duration is specified
                Duration = duration
            };
        }
        
        return NotifyClients();
    }

    private bool NotifyClients()
    {
        // Return false if no clients are connected
        lock (_lock)
        {
            if (_sseClients.Count == 0)
            {
                return false;
            }
        }
        
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        string json;
        lock (_lock)
        {
            json = JsonSerializer.Serialize(_currentState, options);
        }
        
        List<StreamWriter> deadClients;
        int successCount = 0;
        
        lock (_lock)
        {
            deadClients = new List<StreamWriter>();
            
            foreach (var client in _sseClients)
            {
                try
                {
                    client.WriteLine($"data: {json}");
                    client.WriteLine();
                    client.Flush();
                    successCount++;
                }
                catch
                {
                    deadClients.Add(client);
                }
            }
            
            // Remove dead connections
            foreach (var dead in deadClients)
            {
                try { dead.Close(); } catch { }
                _sseClients.Remove(dead);
            }
        }
        
        return successCount > 0;
    }

    private async Task HandleRequests(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _listener != null)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => ProcessRequest(context), cancellationToken);
            }
            catch (HttpListenerException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            if (request.Url?.AbsolutePath == "/")
            {
                ServeHtml(response);
            }
            else if (request.Url?.AbsolutePath == "/api/state")
            {
                ServeMapState(response);
            }
            else if (request.Url?.AbsolutePath == "/api/events")
            {
                HandleSseConnection(response);
                return; // Don't close response for SSE
            }
            else
            {
                response.StatusCode = 404;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing request: {ex.Message}");
            response.StatusCode = 500;
        }
        finally
        {
            response.Close();
        }
    }

    private void HandleSseConnection(HttpListenerResponse response)
    {
        response.ContentType = "text/event-stream";
        response.ContentEncoding = Encoding.UTF8;
        response.Headers.Add("Cache-Control", "no-cache");
        response.Headers.Add("Connection", "keep-alive");
        
        var writer = new StreamWriter(response.OutputStream, Encoding.UTF8);
        writer.AutoFlush = true;
        
        lock (_lock)
        {
            _sseClients.Add(writer);
        }
        
        // Send initial state
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        string json;
        lock (_lock)
        {
            json = JsonSerializer.Serialize(_currentState, options);
            _lastClientAccessTime = DateTime.Now;
        }
        
        try
        {
            writer.WriteLine($"data: {json}");
            writer.WriteLine();
            writer.Flush();
            
            
            // Keep connection alive by sending periodic comments
            // If write fails, client has disconnected
            try
            {
                while (!(_cts?.Token.IsCancellationRequested ?? false))
                {
                    // Wait 30 seconds before sending next keep-alive
                    Task.Delay(TimeSpan.FromSeconds(30), _cts?.Token ?? CancellationToken.None).Wait();
                    
                    // Send keep-alive comment (ignored by browser)
                    writer.WriteLine(": keep-alive");
                    writer.WriteLine();
                    writer.Flush();
                }
            }
            catch (OperationCanceledException)
            {
                // Server is shutting down
            }
        }
        catch
        {
            // Client disconnected
        }
        finally
        {
            lock (_lock)
            {
                _sseClients.Remove(writer);
            }
            try { writer.Close(); } catch { }
        }
    }

    private static void ServeHtml(HttpListenerResponse response)
    {
        var html = GetHtmlTemplate();
        var buffer = Encoding.UTF8.GetBytes(html);
        
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
    }

    private void ServeMapState(HttpListenerResponse response)
    {
        MapState state;
        lock (_lock)
        {
            state = _currentState;
            _lastClientAccessTime = DateTime.Now;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var json = JsonSerializer.Serialize(state, options);
        var buffer = Encoding.UTF8.GetBytes(json);
        
        response.ContentType = "application/json";
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
    }

    /// <summary>
    /// Calculates optimal zoom level based on coordinate differences
    /// </summary>
    /// <param name="latDiff">Latitude difference</param>
    /// <param name="lonDiff">Longitude difference</param>
    /// <returns>Optimal zoom level (1-19)</returns>
    private static int CalculateOptimalZoom(double latDiff, double lonDiff)
    {
        var maxDiff = Math.Max(latDiff, lonDiff);
        
        return maxDiff switch
        {
            > 10 => 5,
            > 5 => 6,
            > 2 => 7,
            > 1 => 8,
            > 0.5 => 9,
            > 0.2 => 10,
            > 0.1 => 11,
            > 0.05 => 12,
            _ => 13
        };
    }

    private static string GetHtmlTemplate()
    {
        return Resources.MapHtml.Template;
    }
}

public class MapState
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Zoom { get; set; }
    public string? Marker { get; set; }
    public bool DebugMode { get; set; }
    public double[][]? RouteCoordinates { get; set; }  // [lon, lat] pairs
    public string? RouteColor { get; set; }
    public int RouteWidth { get; set; } = 4;
    public MapMarker[]? Markers { get; set; }  // Multiple markers
    public MapMarker[]? RouteMarkers { get; set; }  // Route start/end markers (use default pin icon)
    public bool Animate { get; set; }  // Enable smooth animation
    public double Duration { get; set; } = 1.0;  // Animation duration in seconds
}

