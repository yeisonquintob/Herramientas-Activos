using Navi.ToolsAssets.Admin.Components;
using Navi.ToolsAssets.Admin.Services.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddTransient<NaviPermissionHttpMessageHandler>();

builder.Services.AddHttpClient("NaviApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["NaviApi:BaseUrl"] ?? "http://localhost:5218");
}).AddHttpMessageHandler<NaviPermissionHttpMessageHandler>();

builder.Services.AddScoped<WebAuthSessionService>();
builder.Services.AddScoped<NaviAccessScopeService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
