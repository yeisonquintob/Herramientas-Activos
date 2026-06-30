using System;
using System.Collections.Generic;
using System.Linq;

namespace Navi.ToolsAssets.Admin.Services.Auth;

public sealed class NaviAccessScopeService
{
    private readonly WebAuthSessionService _auth;

    public NaviAccessScopeService(WebAuthSessionService auth)
    {
        _auth = auth;
    }

    public bool IsAuthenticated => _auth.IsAuthenticated;

    public bool Can(string permission)
    {
        return _auth.HasPermission(permission);
    }

    public bool CanAny(params string[] permissions)
    {
        return _auth.HasAnyPermission(permissions);
    }

    public bool IsAllBranchScope()
    {
        return string.IsNullOrWhiteSpace(_auth.BranchCode) ||
               _auth.BranchCode.Equals("TODAS", StringComparison.OrdinalIgnoreCase) ||
               _auth.BranchCode.Equals("ALL", StringComparison.OrdinalIgnoreCase);
    }

    public bool IsAdminRole()
    {
        return _auth.HasRole("ADMIN", "Administrador NAVI", "Administrador") ||
               CanAny("Security.Roles", "Settings.View", "Settings.Manage");
    }

    public bool IsManagementRole()
    {
        return IsAdminRole() ||
               _auth.HasRole("GERENCIAL", "Gerencia", "Jefatura", "Administrador de sede");
    }

    public bool IsCoordinatorRole()
    {
        return _auth.HasRole("COORD_TALLER", "Coordinador de taller", "COORDINADOR") ||
               CanAny("AssetAssignment.Approve", "Purchases.Approve", "Maintenance.Close", "Reconciliation.Manage");
    }

    public bool IsToolKeeperRole()
    {
        return _auth.HasRole("HERRAMIENTERO", "HERRAMENTERO", "Herramientero", "Herramentero");
    }

    public bool IsTechnicianRole()
    {
        return _auth.HasRole("TECNICO", "Técnico") ||
               Contains(_auth.RoleCode, "TECNICO") ||
               Contains(_auth.RoleName, "Técnico");
    }

    public bool IsOwnDataOnly()
    {
        return IsTechnicianRole();
    }

    public bool CanSeeBranch(string? branchCode)
    {
        if (IsAllBranchScope() || IsAdminRole())
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(branchCode))
        {
            return false;
        }

        return branchCode.Equals(_auth.BranchCode, StringComparison.OrdinalIgnoreCase);
    }

    public bool CanSeeResponsible(string? responsibleName)
    {
        if (!IsOwnDataOnly())
        {
            return true;
        }

        return MatchesCurrentResponsible(responsibleName);
    }

    public bool CanSeeTool(string? branchCode, string? responsibleName)
    {
        // Regla WEB:
        // En módulos maestros web el técnico o usuario de sede debe ver
        // todos los activos de su sede, no solo los asignados a su responsable.
        return CanSeeBranch(branchCode);
    }

    public IEnumerable<T> FilterTools<T>(
        IEnumerable<T> source,
        Func<T, string?> branchSelector,
        Func<T, string?> responsibleSelector)
    {
        // Filtro dinámico por sede.
        // No se filtra por responsable para inventario, disponible, asignación o historial web.
        return source.Where(item => CanSeeBranch(branchSelector(item)));
    }

    public IEnumerable<T> FilterByBranch<T>(
        IEnumerable<T> source,
        Func<T, string?> branchSelector)
    {
        return source.Where(item => CanSeeBranch(branchSelector(item)));
    }

    public bool CanExecute(
        string? permission,
        string? branchCode = null,
        string? responsibleName = null,
        bool ownRequired = false)
    {
        if (!_auth.IsAuthenticated)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(permission) && !_auth.HasPermission(permission))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(branchCode) && !CanSeeBranch(branchCode))
        {
            return false;
        }

        if (ownRequired)
        {
            return MatchesCurrentResponsible(responsibleName);
        }

        return true;
    }

    public bool MatchesCurrentResponsible(string? responsibleName)
    {
        if (string.IsNullOrWhiteSpace(responsibleName))
        {
            return false;
        }

        return Contains(responsibleName, _auth.ResponsiblePersonName) ||
               Contains(responsibleName, _auth.DisplayName) ||
               Contains(responsibleName, _auth.UserName);
    }

    public string GetDefaultBranchFilterText()
    {
        return IsAllBranchScope()
            ? "Todas las sedes"
            : $"{_auth.BranchCode} - {_auth.BranchName}";
    }

    public string GetDefaultBranchCode()
    {
        return IsAllBranchScope() ? string.Empty : _auth.BranchCode;
    }

    private static bool Contains(string? source, string? value)
    {
        return !string.IsNullOrWhiteSpace(source) &&
               !string.IsNullOrWhiteSpace(value) &&
               source.Contains(value, StringComparison.OrdinalIgnoreCase);
    }
}
