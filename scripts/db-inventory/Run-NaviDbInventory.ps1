param(
    [string]$ServerInstance = "localhost,1434",
    [string]$Database = "NaviToolsAssetsDb",
    [string]$SqlUser = "sa",
    [switch]$UseWindowsAuth
)

$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$OutputFolder = Join-Path $Root ("outputs\{0}" -f (Get-Date -Format "yyyyMMdd_HHmmss"))

New-Item -ItemType Directory -Force -Path $OutputFolder | Out-Null

$sqlcmdCommand = Get-Command sqlcmd -ErrorAction SilentlyContinue

if (-not $sqlcmdCommand) {
    Write-Host "No se encontro sqlcmd en Windows." -ForegroundColor Yellow
    Write-Host "Se intentara usar sqlcmd dentro del contenedor Docker." -ForegroundColor Yellow

    $containers = docker ps --format "{{.Names}}" 2>$null
    $sqlContainer = $containers | Where-Object { $_ -match "sql|mssql|server" } | Select-Object -First 1

    if (-not $sqlContainer) {
        throw "No se encontro sqlcmd local ni un contenedor SQL Server activo. Verifica Docker o instala sqlcmd."
    }

    Write-Host "Contenedor detectado: $sqlContainer" -ForegroundColor Cyan
    $UseDocker = $true
}
else {
    $UseDocker = $false
}

if (-not $UseWindowsAuth) {
    $SecurePassword = Read-Host "Ingrese password SQL para usuario $SqlUser" -AsSecureString
    $BSTR = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecurePassword)

    try {
        $PlainPassword = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($BSTR)
        $env:SQLCMDPASSWORD = $PlainPassword
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
    }
}

try {
    Write-Host "============================================================" -ForegroundColor Cyan
    Write-Host "NAVI Herramientas - Inventario BD" -ForegroundColor Cyan
    Write-Host "Servidor: $ServerInstance" -ForegroundColor Cyan
    Write-Host "Base: $Database" -ForegroundColor Cyan
    Write-Host "Salida: $OutputFolder" -ForegroundColor Cyan
    Write-Host "============================================================" -ForegroundColor Cyan

    $Scripts = Get-ChildItem -Path $Root -Filter "*.sql" | Sort-Object Name

    foreach ($Script in $Scripts) {
        $OutputFile = Join-Path $OutputFolder ($Script.BaseName + ".txt")

        Write-Host "Ejecutando $($Script.Name)..." -ForegroundColor Yellow

        if (-not $UseDocker) {
            $Args = @(
                "-S", $ServerInstance,
                "-d", $Database,
                "-C",
                "-b",
                "-r", "1",
                "-i", $Script.FullName,
                "-o", $OutputFile,
                "-W",
                "-w", "65535",
                "-y", "0",
                "-Y", "0"
            )

            if ($UseWindowsAuth) {
                $Args += "-E"
            }
            else {
                $Args += @("-U", $SqlUser)
            }

            & sqlcmd @Args
        }
        else {
            $TempName = "/tmp/" + $Script.Name
            Get-Content $Script.FullName -Raw | docker exec -i $sqlContainer bash -c "cat > '$TempName'"

            $DockerSqlcmd = "/opt/mssql-tools18/bin/sqlcmd"
            docker exec $sqlContainer test -f $DockerSqlcmd
            if ($LASTEXITCODE -ne 0) {
                $DockerSqlcmd = "/opt/mssql-tools/bin/sqlcmd"
            }

            $DockerOutput = docker exec -e SQLCMDPASSWORD=$env:SQLCMDPASSWORD $sqlContainer $DockerSqlcmd -S localhost -d $Database -U $SqlUser -C -b -r 1 -i $TempName -W -w 65535 -y 0 -Y 0 2>&1
            $DockerOutput | Out-File -FilePath $OutputFile -Encoding UTF8
        }

        if ($LASTEXITCODE -ne 0) {
            Write-Warning "El script $($Script.Name) termino con codigo $LASTEXITCODE. Revisa $OutputFile."
        }
        else {
            Write-Host "OK -> $OutputFile" -ForegroundColor Green
        }
    }

    Write-Host "============================================================" -ForegroundColor Green
    Write-Host "Proceso finalizado. Archivos generados en:" -ForegroundColor Green
    Write-Host $OutputFolder -ForegroundColor Green
    Write-Host "============================================================" -ForegroundColor Green
}
finally {
    if (Test-Path Env:\SQLCMDPASSWORD) {
        Remove-Item Env:\SQLCMDPASSWORD -ErrorAction SilentlyContinue
    }
}
