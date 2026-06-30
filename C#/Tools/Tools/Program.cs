using Tools.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// 注册 HttpClientFactory
builder.Services.AddHttpClient("ProxyFetcher", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
});
// 注册代理池服务（后台运行）
builder.Services.AddSingleton<ProxyPoolService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<ProxyPoolService>());

// 注册 HttpClient 连接池（单例）
builder.Services.AddSingleton<HttpClientPool>();

// 随机 UA 提供器
builder.Services.AddSingleton<RandomUserAgentProvider>();
//builder.Services.AddSingleton<SkySignature>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
