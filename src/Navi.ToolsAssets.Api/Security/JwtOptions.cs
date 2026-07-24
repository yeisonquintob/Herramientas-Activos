namespace Navi.ToolsAssets.Api.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "Navi.ToolsAssets.Api";

    public string Audience { get; set; } = "Navi.ToolsAssets.Clients";

    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 60;

    public int AbsoluteSessionHours { get; set; } = 8;

    public int InactivityMinutes { get; set; } = 30;
}
