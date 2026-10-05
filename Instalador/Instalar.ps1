<#
    Instala Cotillón en la PC del cliente.

    Dos modos:

      SERVIDOR  La PC del local que guarda la base. Instala la aplicación y el
                servicio de Windows, que arranca con el equipo y queda prendido
                aunque nadie inicie sesión. Es lo que hace que la caja ande
                después de un reinicio, que los respaldos salgan aunque el local
                esté cerrado, y que el administrador pueda entrar desde afuera.

      TERMINAL  Otra PC del local, o la del administrador en su casa. Instala
                sólo la aplicación y la apunta al servidor. No guarda datos.

    Se puede correr de nuevo sobre una instalación existente: actualiza los
    archivos y deja la base y la configuración como están.

    Uso (como administrador):
        .\Instalar.ps1
        .\Instalar.ps1 -Modo Servidor
        .\Instalar.ps1 -Modo Terminal -Servidor 192.168.0.10
#>

[CmdletBinding()]
param(
    [ValidateSet('Servidor', 'Terminal')]
    [string] $Modo,

    [string] $Servidor,

    [int] $Puerto = 5189,

    [string] $Destino = (Join-Path ${env:ProgramFiles} 'Cotillon')
)

$ErrorActionPreference = 'Stop'

$NombreServicio = 'Cotillon'
$MostrarServicio = 'Cotillon - Servicio de datos'

function Escribir($texto, $color = 'Gray') { Write-Host $texto -ForegroundColor $color }

function Confirmar($pregunta) {
    while ($true) {
        $respuesta = Read-Host "$pregunta (S/N)"
        if ($respuesta -match '^[sSyY]') { return $true }
        if ($respuesta -match '^[nN]')   { return $false }
    }
}

# ---------------------------------------------------------------- Requisitos --

$identidad = [Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()

if (-not $identidad.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Escribir ''
    Escribir 'Hace falta ejecutar esto como administrador.' Red
    Escribir 'Cerrá esta ventana y usá INSTALAR.bat, que lo pide solo.' Yellow
    Escribir ''
    Read-Host 'Enter para salir'
    exit 1
}

Escribir ''
Escribir '===============================================' Cyan
Escribir '  Cotillon - Instalacion' Cyan
Escribir '===============================================' Cyan
Escribir ''

# ---------------------------------------------------------------------- Modo --

if (-not $Modo) {
    Escribir 'Que es esta computadora?' White
    Escribir ''
    Escribir '  1) SERVIDOR  - la PC del local que guarda los datos' White
    Escribir '  2) TERMINAL  - otra PC que se conecta al servidor' White
    Escribir ''

    $Modo = if ((Read-Host 'Elegi 1 o 2') -eq '2') { 'Terminal' } else { 'Servidor' }
}

if ($Modo -eq 'Terminal' -and -not $Servidor) {
    Escribir ''
    Escribir 'Direccion del servidor: la IP de la PC del local.' White
    Escribir 'Si el administrador entra desde su casa por VPN, va la IP de la VPN.' DarkGray
    Escribir ''

    while (-not $Servidor) { $Servidor = (Read-Host 'IP del servidor').Trim() }
}

Escribir ''
Escribir "Modo    : $Modo" Green
Escribir "Destino : $Destino" Green
if ($Modo -eq 'Terminal') { Escribir "Servidor: $Servidor`:$Puerto" Green }
Escribir ''

$esActualizacion = Test-Path (Join-Path $Destino 'Aplicacion')

if ($esActualizacion) {
    Escribir 'Ya hay una instalacion en ese destino: se van a actualizar los archivos.' Yellow
    Escribir 'La base de datos y la configuracion NO se tocan.' Yellow
    Escribir ''
}

if (-not (Confirmar 'Seguimos')) { exit 0 }

# ------------------------------------------------------------------ Servicio --

$servicioExistente = Get-Service -Name $NombreServicio -ErrorAction SilentlyContinue

if ($servicioExistente) {
    # Hay que pararlo antes de copiar: los ejecutables en uso no se pueden
    # reemplazar, y es la causa número uno de que una actualización falle a mitad.
    Escribir 'Deteniendo el servicio...' Yellow

    if ($servicioExistente.Status -ne 'Stopped') {
        Stop-Service -Name $NombreServicio -Force
        $servicioExistente.WaitForStatus('Stopped', '00:00:30')
    }
}

# ------------------------------------------------------------------- Copiado --

Escribir 'Copiando archivos...' Yellow

New-Item -ItemType Directory -Force -Path $Destino | Out-Null

Copy-Item (Join-Path $PSScriptRoot 'Aplicacion') $Destino -Recurse -Force

if ($Modo -eq 'Servidor') {
    Copy-Item (Join-Path $PSScriptRoot 'Servicio') $Destino -Recurse -Force
}

$rutaApp = Join-Path $Destino 'Aplicacion\Cotillon.Desktop.Wpf.exe'
$rutaServicio = Join-Path $Destino 'Servicio\Cotillon.LocalApi.Service.exe'

# --------------------------------------------------------- Configuracion del puesto --

# Qué servidor usa esta PC. Se reescribe siempre porque es justamente lo que
# distingue un puesto de otro y lo que el instalador vino a definir.
$urlApi = if ($Modo -eq 'Servidor') { "http://localhost:$Puerto/" } else { "http://$Servidor`:$Puerto/" }

$configuracion = [ordered]@{
    UrlApi     = $urlApi
    EsServidor = ($Modo -eq 'Servidor')
}

$configuracion | ConvertTo-Json | Set-Content `
    -Path (Join-Path $Destino 'Aplicacion\cotillon.config.json') `
    -Encoding utf8

Escribir "Puesto configurado contra $urlApi" DarkGray

# ------------------------------------------------------- Servicio de Windows --

if ($Modo -eq 'Servidor') {
    if ($servicioExistente) {
        Escribir 'Actualizando el servicio...' Yellow
        # Por si cambió la ruta de instalación entre versiones.
        & sc.exe config $NombreServicio binPath= "`"$rutaServicio`"" start= auto | Out-Null
    }
    else {
        Escribir 'Instalando el servicio de Windows...' Yellow

        New-Service -Name $NombreServicio `
            -BinaryPathName "`"$rutaServicio`"" `
            -DisplayName $MostrarServicio `
            -Description 'Guarda los datos de Cotillon y los sirve a las terminales del local.' `
            -StartupType Automatic | Out-Null
    }

    # Que se levante solo si se cae, en vez de dejar la caja sin sistema hasta
    # que alguien se dé cuenta. Tres intentos y después cada 5 minutos.
    & sc.exe failure $NombreServicio reset= 86400 actions= restart/5000/restart/10000/restart/30000 | Out-Null

    Escribir 'Arrancando el servicio...' Yellow
    Start-Service -Name $NombreServicio

    # ------------------------------------------------------------ Firewall --

    $regla = 'Cotillon - Servicio de datos'

    if (-not (Get-NetFirewallRule -DisplayName $regla -ErrorAction SilentlyContinue)) {
        Escribir 'Abriendo el puerto en el firewall...' Yellow

        # Sólo en redes privadas: la del local. En una red pública queda cerrado,
        # que es lo correcto porque el servicio no pide credenciales por llamada.
        New-NetFirewallRule -DisplayName $regla `
            -Direction Inbound -Action Allow -Protocol TCP `
            -LocalPort $Puerto -Profile Private, Domain | Out-Null
    }
}

# ------------------------------------------------------------------- Accesos --

Escribir 'Creando los accesos directos...' Yellow

$shell = New-Object -ComObject WScript.Shell

foreach ($carpeta in @(
    [Environment]::GetFolderPath('CommonDesktopDirectory'),
    (Join-Path ${env:ProgramData} 'Microsoft\Windows\Start Menu\Programs')
)) {
    $acceso = $shell.CreateShortcut((Join-Path $carpeta 'Cotillon.lnk'))
    $acceso.TargetPath = $rutaApp
    $acceso.WorkingDirectory = Split-Path $rutaApp
    $acceso.Description = 'Sistema de gestion Cotillon'
    $acceso.Save()
}

# ------------------------------------------------------------ Verificacion --

Escribir ''
Escribir 'Verificando...' Yellow

if ($Modo -eq 'Servidor') {
    $urlPrueba = "http://localhost:$Puerto/api/Configuracion"
    $anduvo = $false

    # El servicio tarda en levantar: crea la base y aplica las migraciones en el
    # primer arranque.
    foreach ($intento in 1..20) {
        try {
            Invoke-WebRequest -Uri $urlPrueba -UseBasicParsing -TimeoutSec 3 | Out-Null
            $anduvo = $true
            break
        }
        catch { Start-Sleep -Seconds 2 }
    }

    if ($anduvo) {
        Escribir 'El servicio responde correctamente.' Green
    }
    else {
        Escribir 'El servicio no respondio todavia.' Red
        Escribir "Mira los logs en: $(Join-Path ${env:ProgramData} 'Cotillon\Logs')" Yellow
    }

    $ips = (Get-NetIPAddress -AddressFamily IPv4 |
            Where-Object { $_.IPAddress -notlike '127.*' -and $_.IPAddress -notlike '169.254.*' } |
            Select-Object -ExpandProperty IPAddress) -join ', '

    Escribir ''
    Escribir '-----------------------------------------------' Cyan
    Escribir ' Para las terminales, la direccion de esta PC:' Cyan
    Escribir "   $ips" White
    Escribir '-----------------------------------------------' Cyan
}
else {
    try {
        Invoke-WebRequest -Uri "http://$Servidor`:$Puerto/api/Configuracion" `
            -UseBasicParsing -TimeoutSec 5 | Out-Null

        Escribir 'Se llega al servidor correctamente.' Green
    }
    catch {
        Escribir 'No se pudo conectar con el servidor.' Red
        Escribir 'Revisa que la PC del local este prendida, la IP, y el firewall.' Yellow
        Escribir 'La aplicacion queda instalada: se puede corregir la direccion desde Configuracion.' Yellow
    }
}

Escribir ''
Escribir '===============================================' Green
Escribir '  Instalacion terminada' Green
Escribir '===============================================' Green
Escribir ''

if (-not $esActualizacion -and $Modo -eq 'Servidor') {
    Escribir 'Primer ingreso:  usuario admin  /  PIN 1234' Yellow
    Escribir 'Cambiale el PIN antes de dejar el equipo.' Yellow
    Escribir ''
    Escribir 'Y antes de que el local empiece a usarlo, configura en Configuracion' White
    Escribir 'la carpeta de respaldos sincronizada con la nube.' White
    Escribir ''
}

Read-Host 'Enter para cerrar'
