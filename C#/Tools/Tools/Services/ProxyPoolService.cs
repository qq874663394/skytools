using System.Collections.Concurrent;
using System.Text.Json;

namespace Tools.Services;

public class ProxyPoolService : BackgroundService
{
    private readonly ILogger<ProxyPoolService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private ConcurrentBag<string> _proxies = new();
    private volatile bool _initialized;

    private static readonly string[] ProxyApiUrls = new[]
    {
        "https://proxy.scdn.io/api/get_proxy.php?protocol=https&count=20&country_code=CN"
    };
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(5);

    public bool HasProxies => !_proxies.IsEmpty;

    public string? GetRandomProxy()
    {
        if (_proxies.IsEmpty) return null;
        var arr = _proxies.ToArray();
        return arr[Random.Shared.Next(arr.Length)];
    }

    public ProxyPoolService(ILogger<ProxyPoolService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RefreshProxies();
        _initialized = true;
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(RefreshInterval, stoppingToken);
            await RefreshProxies();
        }
    }

    private async Task RefreshProxies()
    {
        var allProxies = new List<string>();
        foreach (var apiUrl in ProxyApiUrls)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ProxyFetcher");
                var json = await client.GetStringAsync(apiUrl);
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("proxies", out var proxiesArray))
                {
                    foreach (var p in proxiesArray.EnumerateArray())
                    {
                        var ipPort = p.GetString();
                        if (!string.IsNullOrWhiteSpace(ipPort))
                            allProxies.Add($"http://{ipPort}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "获取代理失败：{Url}", apiUrl);
            }
        }

        if (allProxies.Count > 0)
        {
            _proxies = new ConcurrentBag<string>(allProxies);
            _logger.LogInformation("代理池已刷新，共 {Count} 个代理", allProxies.Count);
        }
    }
}