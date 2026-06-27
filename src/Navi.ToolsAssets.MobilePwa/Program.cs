using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Navi.ToolsAssets.MobilePwa;
using Navi.ToolsAssets.MobilePwa.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["NaviApi:BaseUrl"] ?? "http://localhost:5218/";

builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

builder.Services.AddScoped<MobileAuthSessionService>();
builder.Services.AddScoped<NaviMobileApiClient>();

await builder.Build().RunAsync();
