using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace PowerShell.Map.Helpers;

public static class LocationHelper
{
    public static bool TryParseLocation(string location, out double latitude, out double longitude, Action<string>? writeVerbose = null, Action<string>? writeWarning = null)
    {
        latitude = 0;
        longitude = 0;

        if (string.IsNullOrWhiteSpace(location))
            return false;

        // Try parse as coordinates (lat,lon)
        var parts = location.Split(',');
        if (parts.Length == 2 &&
            double.TryParse(parts[0].Trim(), out latitude) &&
            double.TryParse(parts[1].Trim(), out longitude))
        {
            return true;
        }

        // Try geocoding
        return TryGeocodeLocation(location, out latitude, out longitude, writeVerbose, writeWarning);
    }

    private static bool TryGeocodeLocation(string placeName, out double latitude, out double longitude, Action<string>? writeVerbose = null, Action<string>? writeWarning = null)
    {
        latitude = 0;
        longitude = 0;

        try
        {
            writeVerbose?.Invoke($"Geocoding location: {placeName}");
            
            var encodedPlace = Uri.EscapeDataString(placeName);
            var url = $"https://nominatim.openstreetmap.org/search?q={encodedPlace}&format=json&limit=1";
            
            var response = HttpClientFactory.GeocodingClient.GetStringAsync(url).GetAwaiter().GetResult();
            
            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;
            
            if (root.GetArrayLength() > 0)
            {
                var first = root[0];
                if (first.TryGetProperty("lat", out var latElement) &&
                    first.TryGetProperty("lon", out var lonElement))
                {
                    var latStr = latElement.GetString();
                    var lonStr = lonElement.GetString();
                    
                    if (double.TryParse(latStr, NumberStyles.Float, CultureInfo.InvariantCulture, out latitude) &&
                        double.TryParse(lonStr, NumberStyles.Float, CultureInfo.InvariantCulture, out longitude))
                    {
                        writeVerbose?.Invoke($"Geocoded: {placeName} -> {latitude}, {longitude}");
                        return true;
                    }
                }
            }
            
            writeWarning?.Invoke($"Could not geocode location: {placeName}");
            return false;
        }
        catch (Exception ex)
        {
            writeWarning?.Invoke($"Geocoding failed: {ex.Message}");
            return false;
        }
    }

    public static void OpenBrowser(string url, Action<string>? writeWarning = null)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            writeWarning?.Invoke($"Failed to open browser: {ex.Message}");
        }
    }
}