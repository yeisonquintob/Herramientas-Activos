$ErrorActionPreference = "Stop"

$apiBase = "http://localhost:5218"

Write-Host "Validando Dashboard NAVI..." -ForegroundColor Cyan

Write-Host "`n=== SUMMARY ===" -ForegroundColor Cyan
$summary = Invoke-RestMethod "$apiBase/api/dashboard/summary"
$summary | Format-List

Write-Host "`n=== HERRAMIENTAS POR ESTADO ===" -ForegroundColor Cyan
Invoke-RestMethod "$apiBase/api/dashboard/tools-by-status" |
    Format-Table status, statusLabel, count -AutoSize

Write-Host "`n=== HERRAMIENTAS POR SEDE ===" -ForegroundColor Cyan
Invoke-RestMethod "$apiBase/api/dashboard/tools-by-branch" |
    Format-Table branchCode, branchName, totalTools, availableTools, loanedTools, inMaintenanceTools, damagedTools -AutoSize

Write-Host "`n=== ALERTAS ===" -ForegroundColor Cyan
Invoke-RestMethod "$apiBase/api/dashboard/alerts" |
    Format-Table severity, internalCode, message -AutoSize

Write-Host "`n=== ACTIVIDAD RECIENTE ===" -ForegroundColor Cyan
Invoke-RestMethod "$apiBase/api/dashboard/recent-activity" |
    Select-Object -First 10 |
    Format-Table eventType, title, toolInternalCode, performedBy, eventDate -AutoSize

Write-Host "`nDashboard validado correctamente." -ForegroundColor Green
