using System.Net.Http;

namespace PowerShell.Map.Helpers;

public static class HttpClientFactory
{
    private static readonly Lazy<HttpClient> _geocodingClient = new(() =>
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "PowerShell.Map/0.1.0");
        client.Timeout = TimeSpan.FromSeconds(10);
        return client;
    });

    private static readonly Lazy<HttpClient> _routingClient = new(() =>
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "PowerShell.Map/0.1.0");
        client.Timeout = TimeSpan.FromSeconds(30);
        return client;
    });

    public static HttpClient GeocodingClient => _geocodingClient.Value;
    public static HttpClient RoutingClient => _routingClient.Value;
}