using System.Security.Cryptography;
using System.Text;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Navi.ToolsAssets.Api.Security;
using Navi.ToolsAssets.Api.Tenancy;
using Navi.ToolsAssets.Domain.Entities.Security;
using Navi.ToolsAssets.Infrastructure.Extensions;
using Navi.ToolsAssets.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.Configure<TenancyOptions>(
    builder.Configuration.GetSection(TenancyOptions.SectionName));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditActionFilter>();
builder.Services.AddControllers(options =>
{
    options.Filters.AddService<AuditActionFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
{
    if (!builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Testing"))
    {
        throw new InvalidOperationException(
            "Configure Jwt:SigningKey mediante una variable de entorno o un almacén de secretos.");
    }

    jwtOptions.SigningKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    builder.Logging.AddFilter("Navi.ToolsAssets.Api.Security", LogLevel.Information);
}

builder.Services.Configure<JwtOptions>(options =>
{
    options.Issuer = jwtOptions.Issuer;
    options.Audience = jwtOptions.Audience;
    options.SigningKey = jwtOptions.SigningKey;
    options.AccessTokenMinutes = jwtOptions.AccessTokenMinutes;
    options.AbsoluteSessionHours = jwtOptions.AbsoluteSessionHours;
    options.InactivityMinutes = jwtOptions.InactivityMinutes;
});
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<NaviSessionValidator>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = System.Security.Claims.ClaimTypes.Name,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var validator = context.HttpContext.RequestServices
                    .GetRequiredService<NaviSessionValidator>();

                if (!await validator.ValidateAsync(
                        context.Principal!,
                        context.HttpContext.RequestAborted))
                {
                    context.Fail("La sesión expiró, fue revocada o ya no es válida.");
                }
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("authentication", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
});

var defaultCorsOrigins = new[]
{
    "http://localhost:5285",
    "https://localhost:7285",
    "http://localhost:5264",
    "https://localhost:7264"
};

var configuredCorsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .GetChildren()
    .Select(x => x.Value)
    .Where(x => !string.IsNullOrWhiteSpace(x))
    .Select(x => x!.Trim())
    .ToArray();

var corsOrigins = configuredCorsOrigins.Length > 0
    ? configuredCorsOrigins
    : defaultCorsOrigins;

var allowLocalDevelopmentOrigins = builder.Environment.IsDevelopment();

builder.Services.AddCors(options =>
{
    options.AddPolicy("NaviMobileCors", policy =>
    {
        policy
            .SetIsOriginAllowed(origin => IsAllowedCorsOrigin(origin, corsOrigins, allowLocalDevelopmentOrigins))
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddInfrastructure(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("NaviToolsAssetsDb")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'NaviToolsAssetsDb'.");

builder.Services.AddHangfire(configuration =>
{
    configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.FromSeconds(15),
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true,
            SchemaName = "HangFire",
            PrepareSchemaIfNecessary = true
        });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.Services.SeedDatabaseAsync();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHangfireDashboard(
        "/hangfire",
        new DashboardOptions
        {
            Authorization = new[] { new NaviHangfireAuthorizationFilter() }
        });
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; frame-ancestors 'none'; object-src 'none'; base-uri 'self'";

    await next();
});

app.UseRateLimiter();
app.UseCors("NaviMobileCors");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    App = "NAVI Herramientas API",
    Status = "Running",
    Database = "NaviToolsAssetsDb",
    Seed = "AGU / Zona Antioquia / Catálogos base",
    Hangfire = app.Environment.IsDevelopment() ? "/hangfire" : "Disabled"
})).AllowAnonymous();

app.Run();

static bool IsAllowedCorsOrigin(string? origin, string[] allowedOrigins, bool allowLocalDevelopmentOrigins)
{
    if (string.IsNullOrWhiteSpace(origin))
    {
        return false;
    }

    if (allowedOrigins.Any(x => string.Equals(x, origin, StringComparison.OrdinalIgnoreCase)))
    {
        return true;
    }

    if (!allowLocalDevelopmentOrigins)
    {
        return false;
    }

    if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
    {
        return false;
    }

    if (uri.Scheme is not ("http" or "https"))
    {
        return false;
    }

    return string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(uri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(uri.Host, "::1", StringComparison.OrdinalIgnoreCase);
}
