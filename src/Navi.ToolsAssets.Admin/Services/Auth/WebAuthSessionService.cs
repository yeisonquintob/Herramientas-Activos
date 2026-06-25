namespace Navi.ToolsAssets.Admin.Services.Auth;

public sealed class WebAuthSessionService
{
    private readonly HashSet<string> _permissions = new(StringComparer.OrdinalIgnoreCase);

    public bool IsAuthenticated { get; private set; }

    public Guid? UserId { get; private set; }

    public string UserName { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    public string? Position { get; private set; }

    public string? Area { get; private set; }

    public Guid? RoleId { get; private set; }

    public string RoleCode { get; private set; } = string.Empty;

    public string RoleName { get; private set; } = string.Empty;

    public Guid? BranchId { get; private set; }

    public string BranchCode { get; private set; } = "TODAS";

    public string BranchName { get; private set; } = "Todas las sedes";

    public Guid? ResponsiblePersonId { get; private set; }

    public string? ResponsiblePersonName { get; private set; }

    public IReadOnlyCollection<string> Permissions => _permissions;

    public string AuditUser => string.IsNullOrWhiteSpace(UserName) ? "admin-web" : UserName;

    public event Action? OnChange;

    public AuthSessionUser? CurrentUser
    {
        get
        {
            if (!IsAuthenticated || UserId is null || RoleId is null)
            {
                return null;
            }

            return new AuthSessionUser
            {
                UserId = UserId.Value,
                UserName = UserName,
                DisplayName = DisplayName,
                Email = Email,
                Position = Position,
                Area = Area,
                RoleId = RoleId.Value,
                RoleCode = RoleCode,
                RoleName = RoleName,
                BranchId = BranchId,
                BranchCode = BranchCode,
                BranchName = BranchName,
                ResponsiblePersonId = ResponsiblePersonId,
                ResponsiblePersonName = ResponsiblePersonName,
                Permissions = _permissions.ToList()
            };
        }
    }

    public void Login(AuthSessionUser user)
    {
        IsAuthenticated = true;

        UserId = user.UserId;
        UserName = user.UserName ?? string.Empty;
        DisplayName = string.IsNullOrWhiteSpace(user.DisplayName) ? UserName : user.DisplayName!;
        Email = user.Email;
        Position = user.Position;
        Area = user.Area;
        RoleId = user.RoleId;
        RoleCode = user.RoleCode ?? string.Empty;
        RoleName = user.RoleName ?? string.Empty;
        BranchId = user.BranchId;
        BranchCode = string.IsNullOrWhiteSpace(user.BranchCode) ? "TODAS" : user.BranchCode!;
        BranchName = string.IsNullOrWhiteSpace(user.BranchName) ? "Todas las sedes" : user.BranchName!;
        ResponsiblePersonId = user.ResponsiblePersonId;
        ResponsiblePersonName = user.ResponsiblePersonName;

        _permissions.Clear();

        foreach (var permission in user.Permissions ?? new())
        {
            if (!string.IsNullOrWhiteSpace(permission))
            {
                _permissions.Add(permission.Trim());
            }
        }

        NotifyStateChanged();
    }

    public void Logout()
    {
        IsAuthenticated = false;

        UserId = null;
        UserName = string.Empty;
        DisplayName = string.Empty;
        Email = null;
        Position = null;
        Area = null;
        RoleId = null;
        RoleCode = string.Empty;
        RoleName = string.Empty;
        BranchId = null;
        BranchCode = "TODAS";
        BranchName = "Todas las sedes";
        ResponsiblePersonId = null;
        ResponsiblePersonName = null;

        _permissions.Clear();

        NotifyStateChanged();
    }

    public bool HasRole(params string[] roles)
    {
        if (!IsAuthenticated)
        {
            return false;
        }

        return roles.Any(x =>
            string.Equals(x, RoleCode, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(x, RoleName, StringComparison.OrdinalIgnoreCase));
    }

    public bool HasPermission(string permission)
    {
        if (!IsAuthenticated || string.IsNullOrWhiteSpace(permission))
        {
            return false;
        }

        return _permissions.Contains(permission);
    }

    public bool HasAnyPermission(params string[] permissions)
    {
        if (!IsAuthenticated)
        {
            return false;
        }

        return permissions.Any(HasPermission);
    }

    private void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }
}

public sealed class AuthSessionUser
{
    public Guid UserId { get; set; }

    public string? UserName { get; set; }

    public string? DisplayName { get; set; }

    public string? Email { get; set; }

    public string? Position { get; set; }

    public string? Area { get; set; }

    public Guid RoleId { get; set; }

    public string? RoleCode { get; set; }

    public string? RoleName { get; set; }

    public Guid? BranchId { get; set; }

    public string? BranchCode { get; set; }

    public string? BranchName { get; set; }

    public Guid? ResponsiblePersonId { get; set; }

    public string? ResponsiblePersonName { get; set; }

    public List<string> Permissions { get; set; } = new();

    public DateTime? LastLoginAt { get; set; }
}
