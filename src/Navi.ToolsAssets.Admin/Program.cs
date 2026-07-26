using Navi.ToolsAssets.Admin.Components;
using Navi.ToolsAssets.Admin.Services.Api;
using Navi.ToolsAssets.Admin.Services.Auth;
using Navi.ToolsAssets.Admin.Services.Ui;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<WebAuthSessionService>();
builder.Services.AddScoped<IHttpClientFactory, NaviScopedHttpClientFactory>();
builder.Services.AddScoped<NaviAdminPageHeaderState>();
builder.Services.AddScoped<NaviAdminNotificationState>();
builder.Services.AddScoped<NaviAccessScopeService>();

var app = builder.Build();

// NAVI_UNLOAD_COMPATIBILITY_BEGIN
//
// Compatibilidad temporal con el evento unload utilizado
// internamente por la versión actual del runtime de Blazor.
//
// Sintaxis Permissions-Policy:
//     unload=(self)
//
app.Use(async (context, next) =>
{
    context.Response.Headers["Permissions-Policy"] =
        "unload=(self)";

    await next();
});
// NAVI_UNLOAD_COMPATIBILITY_END

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
