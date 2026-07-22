#!/usr/bin/env python3
"""Replace bounded manual KPI markup with shared NAVI components."""

from pathlib import Path
from textwrap import dedent


ROOT = Path(__file__).resolve().parents[2]


def replace_between(relative, start, end, replacement):
    path = ROOT / relative
    text = path.read_text(encoding="utf-8")
    if replacement.strip() in text:
        return False
    start_at = text.find(start)
    if start_at < 0:
        raise RuntimeError(f"No se encontró inicio en {relative}: {start!r}")
    end_at = text.find(end, start_at)
    if end_at < 0:
        raise RuntimeError(f"No se encontró fin en {relative}: {end!r}")
    text = text[:start_at] + replacement.rstrip() + "\n\n" + text[end_at:]
    path.write_text(text, encoding="utf-8")
    return True


def insert_after(relative, marker, content):
    path = ROOT / relative
    text = path.read_text(encoding="utf-8")
    if content.strip() in text:
        return False
    index = text.find(marker)
    if index < 0:
        raise RuntimeError(f"No se encontró marcador en {relative}: {marker!r}")
    index += len(marker)
    text = text[:index] + "\n" + content.rstrip() + "\n" + text[index:]
    path.write_text(text, encoding="utf-8")
    return True


def main():
    changed = []

    replacements = [
        (
            "src/Navi.ToolsAssets.Admin/Components/Pages/Modules/AssetAssignment.razor",
            '<section class="assignment-v24-kpi-row"',
            '<section class="assignment-v24-filter-card"',
            dedent('''
            <NaviKpiToolbar Class="assignment-v24-kpi-row"
                            Refresh="LoadAsync"
                            IsRefreshing="isLoading"
                            RefreshTitle="Actualizar información"
                            RefreshAriaLabel="Actualizar información">
                <NaviKpiGrid Columns="5" Dividers="true" Class="assignment-v24-kpis">
                    <NaviKpiCard Title="Activos visibles"
                                 Description="Según filtros y alcance"
                                 Tone="neutral">
                        <IconContent><NaviKpiIcon Name="inventory" /></IconContent>
                        <ValueContent>@FilteredTools.Count()</ValueContent>
                    </NaviKpiCard>
                    <NaviKpiCard Title="Disponibles"
                                 Description="Listos para asignar"
                                 Tone="positive">
                        <IconContent><NaviKpiIcon Name="check" /></IconContent>
                        <ValueContent>
                            @(FilteredTools.Count(item =>
                                string.Equals(item.OperationalStatus, "Available", StringComparison.OrdinalIgnoreCase)))
                        </ValueContent>
                    </NaviKpiCard>
                    <NaviKpiCard Title="En uso"
                                 Description="Asignadas o prestadas"
                                 Tone="soft">
                        <IconContent><NaviKpiIcon Name="transfer" /></IconContent>
                        <ValueContent>
                            @(FilteredTools.Count(item =>
                                string.Equals(item.OperationalStatus, "Assigned", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(item.OperationalStatus, "Loaned", StringComparison.OrdinalIgnoreCase)))
                        </ValueContent>
                    </NaviKpiCard>
                    <NaviKpiCard Title="Solicitudes"
                                 Description="Pendientes de decisión"
                                 Tone="warning">
                        <IconContent><NaviKpiIcon Name="document" /></IconContent>
                        <ValueContent>
                            @(FilteredTools.Count(item => FindPendingRequestForTool(item.Id) is not null))
                        </ValueContent>
                    </NaviKpiCard>
                    <NaviKpiCard Title="Sin responsable"
                                 Description="Disponibles o por revisar"
                                 Tone="danger">
                        <IconContent><NaviKpiIcon Name="users" /></IconContent>
                        <ValueContent>@(FilteredTools.Count(item => item.ResponsiblePersonId is null))</ValueContent>
                    </NaviKpiCard>
                </NaviKpiGrid>
            </NaviKpiToolbar>
            '''),
        ),
        (
            "src/Navi.ToolsAssets.Admin/Components/Pages/Modules/AssetAssignmentHistory.razor",
            '        <section class="assignment-history-v26-kpi-row"',
            '        <section class="assignment-history-v26-filter-card"',
            dedent('''
                    <NaviKpiToolbar Class="assignment-history-v26-kpi-row"
                                    Refresh="LoadHistoryAsync"
                                    IsRefreshing="isLoading"
                                    RefreshTitle="Actualizar historial"
                                    RefreshAriaLabel="Actualizar historial">
                        <NaviKpiGrid Columns="5" Dividers="true" Class="assignment-history-v26-kpis">
                            <NaviKpiCard Title="Movimientos" Description="Según filtros activos" Tone="neutral">
                                <IconContent><NaviKpiIcon Name="document" /></IconContent>
                                <ValueContent>@FilteredHistory.Count()</ValueContent>
                            </NaviKpiCard>
                            <NaviKpiCard Title="En uso" Description="Asignadas o prestadas" Tone="soft">
                                <IconContent><NaviKpiIcon Name="transfer" /></IconContent>
                                <ValueContent>
                                    @(FilteredHistory.Count(item =>
                                        string.Equals(item.CurrentStatus, "Assigned", StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(item.CurrentStatus, "Loaned", StringComparison.OrdinalIgnoreCase)))
                                </ValueContent>
                            </NaviKpiCard>
                            <NaviKpiCard Title="Regresos" Description="Con fecha registrada" Tone="positive">
                                <IconContent><NaviKpiIcon Name="check" /></IconContent>
                                <ValueContent>@(FilteredHistory.Count(item => item.ReturnedAt.HasValue))</ValueContent>
                            </NaviKpiCard>
                            <NaviKpiCard Title="Sedes" Description="En el resultado actual" Tone="neutral">
                                <IconContent><NaviKpiIcon Name="location" /></IconContent>
                                <ValueContent>
                                    @(FilteredHistory.Select(item => item.CurrentBranchCode ?? string.Empty)
                                        .Where(value => !string.IsNullOrWhiteSpace(value))
                                        .Distinct(StringComparer.OrdinalIgnoreCase).Count())
                                </ValueContent>
                            </NaviKpiCard>
                            <NaviKpiCard Title="Responsables" Description="Usuarios visibles" Tone="neutral">
                                <IconContent><NaviKpiIcon Name="users" /></IconContent>
                                <ValueContent>
                                    @(FilteredHistory.Select(item => item.CurrentResponsibleName ?? string.Empty)
                                        .Where(value => !string.IsNullOrWhiteSpace(value))
                                        .Distinct(StringComparer.OrdinalIgnoreCase).Count())
                                </ValueContent>
                            </NaviKpiCard>
                        </NaviKpiGrid>
                    </NaviKpiToolbar>
            '''),
        ),
        (
            "src/Navi.ToolsAssets.Admin/Components/Pages/Modules/AssetAvailability.razor",
            '        <section class="availability-v24-kpi-row"',
            '        <section class="availability-v24-filter-card"',
            dedent('''
                    <NaviKpiToolbar Class="availability-v24-kpi-row"
                                    Refresh="LoadAsync"
                                    IsRefreshing="isLoading"
                                    RefreshTitle="Actualizar disponible y ubicación"
                                    RefreshAriaLabel="Actualizar disponible y ubicación">
                        <NaviKpiGrid Columns="5" Dividers="true" Class="availability-v24-kpis">
                            <NaviKpiCard Title="Inventario visible" Description="Herramientas según alcance" Tone="neutral">
                                <IconContent><NaviKpiIcon Name="inventory" /></IconContent>
                                <ValueContent>@FilteredTools.Count()</ValueContent>
                            </NaviKpiCard>
                            <NaviKpiCard Title="Disponibles" Description="Listas para operación" Tone="positive">
                                <IconContent><NaviKpiIcon Name="check" /></IconContent>
                                <ValueContent>
                                    @FilteredTools.Count(tool => string.Equals(
                                        tool.OperationalStatus, "Available", StringComparison.OrdinalIgnoreCase))
                                </ValueContent>
                            </NaviKpiCard>
                            <NaviKpiCard Title="En uso" Description="Asignadas o prestadas" Tone="soft">
                                <IconContent><NaviKpiIcon Name="transfer" /></IconContent>
                                <ValueContent>
                                    @FilteredTools.Count(tool =>
                                        string.Equals(tool.OperationalStatus, "Assigned", StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(tool.OperationalStatus, "Loaned", StringComparison.OrdinalIgnoreCase))
                                </ValueContent>
                            </NaviKpiCard>
                            <NaviKpiCard Title="Mantenimiento" Description="Fuera de operación" Tone="warning">
                                <IconContent><NaviKpiIcon Name="wrench" /></IconContent>
                                <ValueContent>
                                    @FilteredTools.Count(tool => string.Equals(
                                        tool.OperationalStatus, "InMaintenance", StringComparison.OrdinalIgnoreCase))
                                </ValueContent>
                            </NaviKpiCard>
                            <NaviKpiCard Title="Sin ubicación" Description="Requieren revisión" Tone="danger">
                                <IconContent><NaviKpiIcon Name="location" /></IconContent>
                                <ValueContent>
                                    @FilteredTools.Count(tool => string.IsNullOrWhiteSpace(tool.LocationName))
                                </ValueContent>
                            </NaviKpiCard>
                        </NaviKpiGrid>
                    </NaviKpiToolbar>
            '''),
        ),
        (
            "src/Navi.ToolsAssets.Admin/Components/Pages/ToolDetail.razor",
            '    <section class="tool-detail-summary-shell',
            '    <section class="tool-detail-panel',
            dedent('''
                <NaviKpiToolbar Class="tool-detail-summary-shell tool-detail-summary-v30"
                                Refresh="LoadAsync"
                                IsRefreshing="isLoading"
                                RefreshTitle="Actualizar información"
                                RefreshAriaLabel="Actualizar información">
                    <NaviKpiGrid Columns="4" Dividers="true" Class="tool-detail-kpi-strip-v30">
                        <NaviKpiCard Title="Código interno" Description="Identificación del activo" Tone="neutral">
                            <IconContent><NaviKpiIcon Name="inventory" /></IconContent>
                            <ValueContent>@ShowValue(tool.InternalCode)</ValueContent>
                        </NaviKpiCard>
                        <NaviKpiCard Title="Estado operativo" Description="Situación actual" Tone="positive">
                            <IconContent><NaviKpiIcon Name="check" /></IconContent>
                            <ValueContent>@GetStatusLabel(tool.OperationalStatus)</ValueContent>
                        </NaviKpiCard>
                        <NaviKpiCard Title="Clasificación" Description="Tipo de operación" Tone="soft">
                            <IconContent><NaviKpiIcon Name="document" /></IconContent>
                            <ValueContent>@(tool.IsSpecialized ? "Especializada" : "No especializada")</ValueContent>
                        </NaviKpiCard>
                        <NaviKpiCard Title="Responsable" Description="Custodia registrada" Tone="neutral">
                            <IconContent><NaviKpiIcon Name="users" /></IconContent>
                            <ValueContent>@ShowValue(tool.ResponsiblePersonName)</ValueContent>
                        </NaviKpiCard>
                    </NaviKpiGrid>
                </NaviKpiToolbar>
            '''),
        ),
        (
            "src/Navi.ToolsAssets.MobilePwa/Pages/MobileAssetAvailability.razor",
            '        <section class="mobile-availability-v24-kpi-block"',
            '        <section class="mobile-availability-v24-filter-card"',
            dedent('''
                    <NaviMobileKpiOverflow PrimaryCount="3"
                                           OverflowCount="2"
                                           Class="mobile-availability-v24-kpi-block"
                                           ButtonLabel="Mostrar indicadores adicionales">
                        <PrimaryContent>
                            <NaviMobileKpiGrid Columns="3" Dividers="true">
                                <NaviMobileKpiCard Title="Inventario" Description="Activos visibles" IconKey="inventory" Tone="neutral">
                                    <ValueContent>@FilteredTools.Count()</ValueContent>
                                </NaviMobileKpiCard>
                                <NaviMobileKpiCard Title="Disponibles" Description="Listas para uso" IconKey="check" Tone="positive">
                                    <ValueContent>@FilteredTools.Count(IsAvailable)</ValueContent>
                                </NaviMobileKpiCard>
                                <NaviMobileKpiCard Title="En uso" Description="Asignadas" IconKey="transfer" Tone="soft">
                                    <ValueContent>@FilteredTools.Count(IsAssignedOrLoaned)</ValueContent>
                                </NaviMobileKpiCard>
                            </NaviMobileKpiGrid>
                        </PrimaryContent>
                        <OverflowContent>
                            <NaviMobileKpiGrid Columns="2" Dividers="true">
                                <NaviMobileKpiCard Title="Mantenimiento" Description="Fuera de operación" IconKey="wrench" Tone="warning">
                                    <ValueContent>@FilteredTools.Count(IsInMaintenance)</ValueContent>
                                </NaviMobileKpiCard>
                                <NaviMobileKpiCard Title="Sin ubicación" Description="Requieren revisión" IconKey="location" Tone="danger">
                                    <ValueContent>@FilteredTools.Count(IsWithoutLocation)</ValueContent>
                                </NaviMobileKpiCard>
                            </NaviMobileKpiGrid>
                        </OverflowContent>
                    </NaviMobileKpiOverflow>
            '''),
        ),
        (
            "src/Navi.ToolsAssets.MobilePwa/Pages/MobileAssignment.razor",
            '        <section class="mobile-assignment-v25-kpi-shell"',
            '        @if (CanApproveAssignmentRequest || CanDenyAssignmentRequest)',
            dedent('''
                    <NaviMobileKpiOverflow PrimaryCount="3"
                                           OverflowCount="2"
                                           Class="mobile-assignment-v25-kpi-shell"
                                           ButtonLabel="Mostrar indicadores adicionales">
                        <PrimaryContent>
                            <NaviMobileKpiGrid Columns="3" Dividers="true">
                                <NaviMobileKpiCard Title="Activos" Description="Visibles" IconKey="inventory" Tone="neutral">
                                    <ValueContent>@FilteredTools.Count()</ValueContent>
                                </NaviMobileKpiCard>
                                <NaviMobileKpiCard Title="Disponibles" Description="Para asignar" IconKey="check" Tone="positive">
                                    <ValueContent>
                                        @(FilteredTools.Count(tool => string.Equals(
                                            tool.OperationalStatus, "Available", StringComparison.OrdinalIgnoreCase)))
                                    </ValueContent>
                                </NaviMobileKpiCard>
                                <NaviMobileKpiCard Title="Solicitudes" Description="Pendientes" IconKey="document" Tone="warning">
                                    <ValueContent>@PendingRequests.Count()</ValueContent>
                                </NaviMobileKpiCard>
                            </NaviMobileKpiGrid>
                        </PrimaryContent>
                        <OverflowContent>
                            <NaviMobileKpiGrid Columns="2" Dividers="true">
                                <NaviMobileKpiCard Title="En uso" Description="Asignados" IconKey="transfer" Tone="soft">
                                    <ValueContent>
                                        @(FilteredTools.Count(tool =>
                                            string.Equals(tool.OperationalStatus, "Assigned", StringComparison.OrdinalIgnoreCase)
                                            || string.Equals(tool.OperationalStatus, "Loaned", StringComparison.OrdinalIgnoreCase)))
                                    </ValueContent>
                                </NaviMobileKpiCard>
                                <NaviMobileKpiCard Title="Sin responsable" Description="Por asignar" IconKey="users" Tone="danger">
                                    <ValueContent>@(FilteredTools.Count(tool => tool.ResponsiblePersonId is null))</ValueContent>
                                </NaviMobileKpiCard>
                            </NaviMobileKpiGrid>
                        </OverflowContent>
                    </NaviMobileKpiOverflow>
            '''),
        ),
        (
            "src/Navi.ToolsAssets.MobilePwa/Pages/MobileDashboard.razor",
            '            <section class="md22-kpi-shell md22-kpi-shell-tabs"',
            '            <section class="md22-context">',
            dedent('''
                        <NaviMobileKpiOverflow PrimaryCount="3"
                                               OverflowCount="3"
                                               Class="md22-kpi-shell md22-kpi-shell-tabs"
                                               ButtonLabel="Mostrar indicadores adicionales">
                            <PrimaryContent>
                                <NaviMobileKpiGrid Columns="3" Dividers="true">
                                    <NaviMobileKpiCard Title="Inventario" Description="Activos visibles" Value="@FormatDashboardNumber(DashboardTotalTools)" IconKey="inventory" Tone="neutral" />
                                    <NaviMobileKpiCard Title="En uso" Description="Asignadas o prestadas" Value="@FormatDashboardNumber(GetDashboardInUseTotal())" IconKey="transfer" Tone="soft" />
                                    <NaviMobileKpiCard Title="Disponibles" Description="Para asignación" Value="@FormatDashboardNumber(DashboardAvailableTools)" IconKey="check" Tone="positive" />
                                </NaviMobileKpiGrid>
                            </PrimaryContent>
                            <OverflowContent>
                                <NaviMobileKpiGrid Columns="3" Dividers="true">
                                    <NaviMobileKpiCard Title="Mantenimiento" Description="Proceso técnico" Value="@FormatDashboardNumber(DashboardInMaintenanceTools)" IconKey="wrench" Tone="warning" />
                                    <NaviMobileKpiCard Title="Alertas" Description="Requieren atención" Value="@FormatDashboardNumber(GetDashboardAlertTotal())" IconKey="alert" Tone="danger" />
                                    <NaviMobileKpiCard Title="Cobertura" Description="Hojas completas" Value="@GetDashboardLifeCompletionPercentLabel()" IconKey="document" Tone="positive" />
                                </NaviMobileKpiGrid>
                            </OverflowContent>
                        </NaviMobileKpiOverflow>
            '''),
        ),
        (
            "src/Navi.ToolsAssets.MobilePwa/Pages/MobileHistory.razor",
            '        <section class="mobile-history-v25-kpi-shell"',
            '        <section class="mobile-history-v25-filter-card mobile-history-v27-filters"',
            dedent('''
                    <NaviMobileKpiOverflow PrimaryCount="3"
                                           OverflowCount="2"
                                           Class="mobile-history-v25-kpi-shell"
                                           ButtonLabel="Mostrar indicadores adicionales">
                        <PrimaryContent>
                            <NaviMobileKpiGrid Columns="3" Dividers="true">
                                <NaviMobileKpiCard Title="Movimientos" Description="Según filtros" IconKey="document" Tone="neutral">
                                    <ValueContent>@HistoryMovementCount</ValueContent>
                                </NaviMobileKpiCard>
                                <NaviMobileKpiCard Title="En curso" Description="Activas" IconKey="transfer" Tone="soft">
                                    <ValueContent>@HistoryInProgressCount</ValueContent>
                                </NaviMobileKpiCard>
                                <NaviMobileKpiCard Title="Regresos" Description="Devueltas" IconKey="check" Tone="positive">
                                    <ValueContent>@HistoryReturnedCount</ValueContent>
                                </NaviMobileKpiCard>
                            </NaviMobileKpiGrid>
                        </PrimaryContent>
                        <OverflowContent>
                            <NaviMobileKpiGrid Columns="2" Dividers="true">
                                <NaviMobileKpiCard Title="Sedes" Description="En el alcance" IconKey="location" Tone="neutral">
                                    <ValueContent>@VisibleBranchCount</ValueContent>
                                </NaviMobileKpiCard>
                                <NaviMobileKpiCard Title="Responsables" Description="Usuarios visibles" IconKey="users" Tone="neutral">
                                    <ValueContent>@HistoryResponsibleCount</ValueContent>
                                </NaviMobileKpiCard>
                            </NaviMobileKpiGrid>
                        </OverflowContent>
                    </NaviMobileKpiOverflow>
            '''),
        ),
        (
            "src/Navi.ToolsAssets.MobilePwa/Pages/MyTools.razor",
            '        <section class="mobile-inventory-v23-kpi-block"',
            '        <section class="mobile-inventory-v23-filter-card"',
            dedent('''
                    <NaviMobileKpiOverflow PrimaryCount="3"
                                           OverflowCount="3"
                                           Class="mobile-inventory-v23-kpi-block"
                                           ButtonLabel="Mostrar indicadores adicionales">
                        <PrimaryContent>
                            <NaviMobileKpiGrid Columns="3" Dividers="true">
                                <NaviMobileKpiCard Title="Inventario" Description="Activos visibles" Value="@FormatNumber(summaryTools.Count)" IconKey="inventory" Tone="neutral" />
                                <NaviMobileKpiCard Title="Disponibles" Description="Para asignación" Value="@FormatNumber(AvailableCount)" IconKey="check" Tone="positive" />
                                <NaviMobileKpiCard Title="En uso" Description="Asignadas" Value="@FormatNumber(InUseCount)" IconKey="transfer" Tone="soft" />
                            </NaviMobileKpiGrid>
                        </PrimaryContent>
                        <OverflowContent>
                            <NaviMobileKpiGrid Columns="3" Dividers="true">
                                <NaviMobileKpiCard Title="Mantenimiento" Description="Proceso técnico" Value="@FormatNumber(MaintenanceCount)" IconKey="wrench" Tone="warning" />
                                <NaviMobileKpiCard Title="Por validar" Description="Pendientes" Value="@FormatNumber(PendingValidationCount)" IconKey="clock" Tone="warning" />
                                <NaviMobileKpiCard Title="Alertas" Description="Requieren atención" Value="@FormatNumber(AlertCount)" IconKey="alert" Tone="danger" />
                            </NaviMobileKpiGrid>
                        </OverflowContent>
                    </NaviMobileKpiOverflow>
            '''),
        ),
    ]

    for relative, start, end, replacement in replacements:
        if replace_between(relative, start, end, replacement):
            changed.append(relative)

    dashboard_banner = dedent('''
        <NaviMobileAppBanner
            Class="mobile-dashboard-header-publisher"
            Pill="INICIO"
            Title="Dashboard móvil"
            Description="Resumen ejecutivo de herramientas y activos."
            HomeUrl="/"
            ShowHomeButton="false"
            ShowBackButton="false" />
    ''')
    if insert_after(
        "src/Navi.ToolsAssets.MobilePwa/Pages/MobileDashboard.razor",
        '<section class="mobile-page mobile-page-compact navi-mobile-dashboard-v6">',
        dashboard_banner,
    ):
        changed.append("src/Navi.ToolsAssets.MobilePwa/Pages/MobileDashboard.razor (encabezado)")

    inventory_banner = dedent('''
        <NaviMobileAppBanner
            Class="mobile-inventory-header-publisher"
            Pill="ACTIVOS FIJOS"
            Title="@CurrentPageTitle"
            Description="Consulta herramientas y activos visibles según tu alcance."
            HomeUrl="/"
            ShowHomeButton="false"
            ShowBackButton="false" />
    ''')
    if insert_after(
        "src/Navi.ToolsAssets.MobilePwa/Pages/MyTools.razor",
        '<section class="mobile-page mobile-inventory-v23">',
        inventory_banner,
    ):
        changed.append("src/Navi.ToolsAssets.MobilePwa/Pages/MyTools.razor (encabezado)")

    print(f"Archivos/bloques actualizados: {len(changed)}")
    for item in changed:
        print(item)


if __name__ == "__main__":
    main()
