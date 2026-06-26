using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Tools.Services;

namespace Tools.Controllers;

public class WingController : Controller
{
    private readonly ILogger<WingController> _logger;
    private readonly ProxyPoolService _proxyPool;
    private readonly HttpClientPool _httpClientPool;
    private readonly RandomUserAgentProvider _uaProvider;
    // 网易 API 固定参数（根据实际抓包更新）
    private const string TS = "1782378941";
    private const string UF = "004ba46c-27e6-4ba5-9bf6-32afbd2c1373";
    private const string AB = "dff60cb47bd013a953b1669caf31760095";
    private const string EF = "4783885dcdd100b5cfcb348e406a5acaf3";
    public WingController(ILogger<WingController> logger,
        ProxyPoolService proxyPool,
        HttpClientPool httpClientPool,
        RandomUserAgentProvider uaProvider)
    {
        _logger = logger;
        _proxyPool = proxyPool;
        _httpClientPool = httpClientPool;
        _uaProvider = uaProvider;
    }

    // 返回光翼查询页面
    public IActionResult Index()
    {
        return View();
    }

    // 后端代理查询光翼接口：GET /query?roleId=xxx
    [HttpGet("/query")]
    public async Task<IActionResult> QueryWingBuff(string roleId)
    {
        if (string.IsNullOrWhiteSpace(roleId))
            return BadRequest(new { error = "缺少 roleId" });

        var userAgent = _uaProvider.GetSkyMobileUserAgent();
        var maxRetries = 3;

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            string? proxy = _proxyPool.GetRandomProxy(); // 暂时禁用代理，调试通过后可恢复
            var client = _httpClientPool.GetClient(proxy, userAgent);

            try
            {
                //var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                //var nonce = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid().ToString().ToUpper()}";
                var body = JsonSerializer.Serialize(new { roleId, server = "8000" });

                var request = new HttpRequestMessage(HttpMethod.Post,
                    $"https://god.gameyw.netease.com/v1/app/gameData/queryRoleWingBuff?ts={TS}&uf={UF}&ab={AB}&ef={EF}");

                // 设置 Content-Type 为纯 application/json（无 charset）
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                // 移除可能自动添加的 charset
                request.Content.Headers.ContentType!.CharSet = null;

                request.Headers.Host = "god.gameyw.netease.com";

                request.Headers.Add("GL-Uid", "0801d86b58df4b1cae61ccdab53d518e");
                request.Headers.Add("Accept", "application/json, text/plain, */*");
                request.Headers.Add("GL-Version", "4.19.2");
                request.Headers.Add("GL-Source", "URS");
                request.Headers.Add("Origin", "https://act.ds.163.com");
                request.Headers.Add("Referer", "https://act.ds.163.com/");
                request.Headers.Add("GL-ClientType", "51");
                request.Headers.Add("GL-Nonce", "1782378942197_F229FBF8-DB0E-4C5C-A435-570E2994B32E");
                request.Headers.Add("GL-Token", "b0d5655c419f45a4a3e048332fa668a1");
                request.Headers.Add("GL-DeviceId", "1BFA9166-F683-44EE-8533-A91C697C5D87");
                request.Headers.Add("Accept-Language", "zh-CN,zh-Hans;q=0.9");
                request.Headers.Add("Accept-Encoding", "gzip, deflate");

                var response = await client.SendAsync(request);
                var respContent = await response.Content.ReadAsStringAsync();

                // 检查 HTTP 状态码
                if (!response.IsSuccessStatusCode)
                {
                    // 非 2xx：将原始状态码和响应体（如网易错误 JSON）完整返回
                    return StatusCode((int)response.StatusCode, respContent);
                }

                // 2xx 成功，但需要验证内容是否为 JSON（代理可能返回 HTML）
                var trimmed = respContent.TrimStart();
                if (!trimmed.StartsWith('{') && !trimmed.StartsWith('['))
                {
                    _logger.LogWarning("代理 {Proxy} 返回非 JSON：{Content}", proxy, respContent);
                    continue; // 如果是代理返回的非 JSON，应该重试而不是终止
                }

                // 解析 JSON
                using var doc = JsonDocument.Parse(respContent);
                var root = doc.RootElement;

                // 情况 1：code != 200 的业务错误（如 868）
                if (root.TryGetProperty("code", out var codeProp) && codeProp.TryGetInt32(out var code) && code != 200)
                {
                    // 直接返回网易的错误 JSON，让前端处理
                    return Content(respContent, "application/json");
                }

                // 情况 2：code == 200 且 result 是嵌套的 JSON 字符串
                if (root.TryGetProperty("result", out var resultProp) &&
                    resultProp.ValueKind == JsonValueKind.String)
                {
                    var innerJson = resultProp.GetString()!;
                    return Content(innerJson, "application/json");
                }

                // 情况 3：其他直接返回
                return Content(respContent, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "查询异常 (尝试 {Attempt})", attempt + 1);
                if (attempt == maxRetries - 1)
                {
                    return StatusCode(502, new { v = $"所有重试均失败，原因：{ex.Message}" });
                }
            }
        }

        return StatusCode(502, new { message = "未知错误" });
    }
}