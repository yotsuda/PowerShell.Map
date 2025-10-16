using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace PowerShell.Map.Helpers;

public static class LocationHelper
{
    // Geocoding cache: location name -> (latitude, longitude)
    private static readonly ConcurrentDictionary<string, (double lat, double lon)> _geocodingCache = new();
    
    // Reverse geocoding cache: (latitude, longitude) -> location name
    private static readonly ConcurrentDictionary<string, string> _reverseGeocodingCache = new();
    
    // Helper to create cache key for coordinates (rounded to 6 decimal places)
    private static string GetCoordinateKey(double latitude, double longitude)
    {
        return $"{latitude:F6},{longitude:F6}";
    }

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

        // Try progressively broader searches
        var searchTerms = new List<string> { placeName };
        
        // Remove trailing numbers/hyphens (e.g., "123 Main St" -> "Main St", "上尾市浅間台4-5-27" -> "上尾市浅間台")
        var withoutNumbers = System.Text.RegularExpressions.Regex.Replace(placeName, @"[\d\-\s]+$", "").Trim();
        if (!string.IsNullOrWhiteSpace(withoutNumbers) && withoutNumbers != placeName)
        {
            searchTerms.Add(withoutNumbers);
        }
        
        // Try each search term
        foreach (var searchTerm in searchTerms)
        {
            if (TryGeocodeExact(searchTerm, out latitude, out longitude, writeVerbose))
            {
                return true;
            }
        }
        
        writeWarning?.Invoke($"Location not found: {placeName}. Try using a broader location name or coordinates (latitude,longitude)");
        return false;
    }

    private static bool TryGeocodeExact(string placeName, out double latitude, out double longitude, Action<string>? writeVerbose)
    {
        latitude = 0;
        longitude = 0;

        // Check cache first
        if (_geocodingCache.TryGetValue(placeName, out var cached))
        {
            // Check if this is a cached failure (marked with NaN)
            if (double.IsNaN(cached.lat))
            {
                writeVerbose?.Invoke($"Geocoding failed (from cache): {placeName}");
                return false;
            }
            
            latitude = cached.lat;
            longitude = cached.lon;
            writeVerbose?.Invoke($"Geocoded (from cache): {placeName} -> {latitude}, {longitude}");
            return true;
        }

        try
        {
            writeVerbose?.Invoke($"Trying to geocode: {placeName}");
            
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
                        // Cache the result (both forward and reverse)
                        _geocodingCache.TryAdd(placeName, (latitude, longitude));
                        var coordKey = GetCoordinateKey(latitude, longitude);
                        _reverseGeocodingCache.TryAdd(coordKey, placeName);
                        writeVerbose?.Invoke($"Geocoded: {placeName} -> {latitude}, {longitude}");
                        return true;
                    }
                }
            }
            
            // Cache the failure (only in geocoding cache, NOT in reverse cache)
            _geocodingCache.TryAdd(placeName, (double.NaN, double.NaN));
            writeVerbose?.Invoke($"Geocoding failed: {placeName}");
            return false;
        }
        catch
        {
            // Cache the failure (only in geocoding cache, NOT in reverse cache)
            _geocodingCache.TryAdd(placeName, (double.NaN, double.NaN));
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

    /// <summary>
    /// Try to reverse geocode coordinates to a location name using Nominatim API
    /// </summary>
    public static bool TryReverseGeocode(double latitude, double longitude, out string? locationName, Action<string>? writeVerbose = null)
    {
        locationName = null;

        // Check cache first (using rounded coordinates as key)
        var cacheKey = GetCoordinateKey(latitude, longitude);
        if (_reverseGeocodingCache.TryGetValue(cacheKey, out var cached))
        {
            // Check if this is a cached failure (empty string marker)
            if (string.IsNullOrEmpty(cached))
            {
                writeVerbose?.Invoke($"Reverse geocoding failed (from cache): ({latitude}, {longitude})");
                return false;
            }
            
            locationName = cached;
            writeVerbose?.Invoke($"Reverse geocoded (from cache): ({latitude}, {longitude}) -> {locationName}");
            return true;
        }

        try
        {
            writeVerbose?.Invoke($"Trying to reverse geocode: ({latitude}, {longitude})");
            
            var url = $"https://nominatim.openstreetmap.org/reverse?lat={latitude.ToString(CultureInfo.InvariantCulture)}&lon={longitude.ToString(CultureInfo.InvariantCulture)}&format=json";
            
            var response = HttpClientFactory.GeocodingClient.GetStringAsync(url).GetAwaiter().GetResult();
            
            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;
            
            // Try to get a readable name from the response
            // Priority: 1) name, 2) display_name (first part), 3) address components
            if (root.TryGetProperty("name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String)
            {
                var name = nameElement.GetString();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    locationName = name;
                    // Cache the result (both forward and reverse)
                    _reverseGeocodingCache.TryAdd(cacheKey, locationName);
                    _geocodingCache.TryAdd(locationName, (latitude, longitude));
                    writeVerbose?.Invoke($"Reverse geocoded: ({latitude}, {longitude}) -> {locationName}");
                    return true;
                }
            }
            
            // Fallback to display_name (take first meaningful part)
            if (root.TryGetProperty("display_name", out var displayNameElement) && displayNameElement.ValueKind == JsonValueKind.String)
            {
                var displayName = displayNameElement.GetString();
                if (!string.IsNullOrWhiteSpace(displayName))
                {
                    // Take the first part before the first comma
                    var firstPart = displayName.Split(',')[0].Trim();
                    if (!string.IsNullOrWhiteSpace(firstPart))
                    {
                        locationName = firstPart;
                        // Cache the result (both forward and reverse)
                        _reverseGeocodingCache.TryAdd(cacheKey, locationName);
                        _geocodingCache.TryAdd(locationName, (latitude, longitude));
                        writeVerbose?.Invoke($"Reverse geocoded: ({latitude}, {longitude}) -> {locationName}");
                        return true;
                    }
                }
            }
            
            // Cache the failure (only in reverse cache, NOT in geocoding cache)
            _reverseGeocodingCache.TryAdd(cacheKey, string.Empty);
            writeVerbose?.Invoke($"Reverse geocoding failed: ({latitude}, {longitude})");
            return false;
        }
        catch (Exception ex)
        {
            // Cache the failure (only in reverse cache, NOT in geocoding cache)
            _reverseGeocodingCache.TryAdd(cacheKey, string.Empty);
            writeVerbose?.Invoke($"Reverse geocoding failed: {ex.Message}");
            return false;
        }
    }
}
