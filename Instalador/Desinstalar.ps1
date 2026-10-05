<#
    Desinstala Cotillón.

    NO borra la base de datos ni los respaldos. Viven en ProgramData\Cotillon y
    quedan donde están: son los datos del negocio, no del programa. Si alguna
    vez hay que borrarlos de verdad, se hace a mano y a conciencia.

    Uso (como administrador):
        .\Desinstalar.ps1
#>

[CmdletBinding()]
param(
    [string] $Destino = (Join-Path ${env:ProgramFiles} 'Cotillon')
)

$ErrorActionPreference = 'Stop'

$NombreServicio = 'Cotillon'

function Escribir($texto, $color = 'Gray') { Write-Host $texto -ForegroundColor $color }

$identidad = [Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()

if (-not $identidad.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Escribir 'Hace falta ejecutar esto como administrador. Usa DESINSTALAR.bat.' Red
    Read-Host 'Enter para salir'
    exit 1
}

Escribir ''
Escribir 'Cotillon - Desinstalacion' Cyan
Escribir ''
Escribir 'Se quitan el programa y el servicio.' White
Escribir 'La base de datos y los respaldos NO se borran:' Yellow
Escribir "   $(Join-Path ${env:ProgramData} 'Cotillon')" Yellow
Escribir ''

if ((Read-Host 'Seguimos (S/N)') -notmatch '^[sSyY]') { exit 0 }

$servicio = Get-Service -Name $NombreServicio -ErrorAction SilentlyContinue

if ($servicio) {
    Escribir 'Deteniendo y quitando el servicio...' Yellow

    if ($servicio.Status -ne 'Stopped') {
        Stop-Service -Name $NombreServicio -Force
        $servicio.WaitForStatus('Stopped', '00:00:30')
    }

    & sc.exe delete $NombreServicio | Out-Null
}

$regla = 'Cotillon - Servicio de datos'

if (Get-NetFirewallRule -DisplayName $regla -ErrorAction SilentlyContinue) {
    Escribir 'Quitando la regla del firewall...' Yellow
    Remove-NetFirewallRule -DisplayName $regla
}

Escribir 'Quitando los accesos directos...' Yellow

foreach ($carpeta in @(
    [Environment]::GetFolderPath('CommonDesktopDirectory'),
    (Join-Path ${env:ProgramData} 'Microsoft\Windows\Start Menu\Programs')
)) {
    $acceso = Join-Path $carpeta 'Cotillon.lnk'
    if (Test-Path $acceso) { Remove-Item $acceso -Force }
}

if (Test-Path $Destino) {
    Escribir 'Borrando los archivos del programa...' Yellow

    # El servicio recién detenido puede tardar un instante en soltar sus archivos.
    foreach ($intento in 1..5) {
        try {
            Remove-Item $Destino -Recurse -Force
            break
        }
        catch {
            if ($intento -eq 5) {
                Escribir "No se pudieron borrar todos los archivos de $Destino" Red
                Escribir 'Reinicia el equipo y borra esa carpeta a mano.' Yellow
            }
            else { Start-Sleep -Seconds 2 }
        }
    }
}

Escribir ''
Escribir 'Desinstalacion terminada.' Green
Escribir "Los datos siguen en $(Join-Path ${env:ProgramData} 'Cotillon')" White
Escribir ''

Read-Host 'Enter para cerrar'
