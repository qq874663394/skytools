using System.Collections.Concurrent;
using System.Net;
using System.Net.Security;

namespace Tools.Services;

public class HttpClientPool
{
    private readonly ConcurrentDictionary<string, SocketsHttpHandler> _handlers = new();

    public HttpClient GetClient(string? proxyUrl, string userAgent)
    {
        var key = proxyUrl ?? "__DIRECT__";
        var handler = _handlers.GetOrAdd(key, _ =>
        {
            var h = new SocketsHttpHandler
            {
                // ... 原有配置
                ActivityHeadersPropagator = null
            };
            return h;
        });

        var client = new HttpClient(handler, disposeHandler: false);
        // 绕过验证直接设置 User-Agent
        client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent);
        client.Timeout = TimeSpan.FromSeconds(15);
        return client;
    }
}