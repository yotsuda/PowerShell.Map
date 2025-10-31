using System.Reflection;

namespace PowerShell.Map.Helpers;

public static class HttpClientFactory
{
    private static readonly string _version = Assembly.GetExecutingAssembly()
        .GetName().Version?.ToString(3) ?? "1.0.0";

    private static readonly Lazy<HttpClient> _geocodingClient = new(() =>
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", $"PowerShell.Map/{_version}");
        client.Timeout = TimeSpan.FromSeconds(10);
        return client;
    });

    private static readonly Lazy<HttpClient> _routingClient = new(() =>
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", $"PowerShell.Map/{_version}");
        client.Timeout = TimeSpan.FromSeconds(30);
        return client;
    });

    public static HttpClient GeocodingClient => _geocodingClient.Value;
    public static HttpClient RoutingClient => _routingClient.Value;
}
