<#
    Arma el paquete que se lleva a la PC del cliente.

    Lo corre el desarrollador en su máquina, no el cliente. Deja una carpeta
    "Paquete" con todo adentro: el servicio, la aplicación de escritorio y los
    scripts de instalación. Esa carpeta es lo único que hay que copiar al
    equipo del local, en un pendrive o por red.

    Se publica autocontenido a propósito. Depender de que la PC del cliente
    tenga instalado el runtime de .NET correcto es el problema más común de un
    despliegue, y se descubre siempre el día de la instalación, en el local, con
    el cliente mirando. Pesa más y no importa.

    Uso:
        .\Empaquetar.ps1
        .\Empaquetar.ps1 -Salida D:\entregas\cotillon
#>

[CmdletBinding()]
param(
    [string] $Salida = (Join-Path $PSScriptRoot 'Paquete'),
    [string] $Configuracion = 'Release'
)

$ErrorActionPreference = 'Stop'

$raiz = Split-Path -Parent $PSScriptRoot
$api = Join-Path $raiz 'Cotillon.LocalApi.Service'
$escritorio = Join-Path $raiz 'Cotillon.Desktop.Wpf'

Write-Host ''
Write-Host '== Empaquetando Cotillon ==' -ForegroundColor Cyan
Write-Host "   Proyecto : $raiz"
Write-Host "   Salida   : $Salida"
Write-Host ''

# Se parte de cero: restos de un empaquetado anterior son la forma más fácil de
# llevarle al cliente una mezcla de dos versiones.
if (Test-Path $Salida) {
    Write-Host 'Limpiando la salida anterior...' -ForegroundColor DarkGray
    Remove-Item $Salida -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $Salida | Out-Null

$parametros = @(
    '--configuration', $Configuracion,
    '--runtime', 'win-x64',
    '--self-contained', 'true',
    '--nologo',
    '-p:PublishSingleFile=false',
    '-p:DebugType=none'
)

Write-Host 'Publicando el servicio...' -ForegroundColor Yellow
dotnet publish $api @parametros --output (Join-Path $Salida 'Servicio')
if ($LASTEXITCODE -ne 0) { throw 'Falló la publicación del servicio.' }

Write-Host 'Publicando la aplicación...' -ForegroundColor Yellow
dotnet publish $escritorio @parametros --output (Join-Path $Salida 'Aplicacion')
if ($LASTEXITCODE -ne 0) { throw 'Falló la publicación de la aplicación.' }

Write-Host 'Copiando los scripts de instalación...' -ForegroundColor Yellow
Copy-Item (Join-Path $PSScriptRoot 'Instalar.ps1')     $Salida
Copy-Item (Join-Path $PSScriptRoot 'Desinstalar.ps1')  $Salida
Copy-Item (Join-Path $PSScriptRoot 'INSTALAR.bat')     $Salida
Copy-Item (Join-Path $PSScriptRoot 'DESINSTALAR.bat')  $Salida
Copy-Item (Join-Path $PSScriptRoot 'LEEME.txt')        $Salida

$tamano = (Get-ChildItem $Salida -Recurse -File | Measure-Object -Property Length -Sum).Sum / 1MB

Write-Host ''
Write-Host 'Listo.' -ForegroundColor Green
Write-Host ("   Paquete: {0}  ({1:N0} MB)" -f $Salida, $tamano)
Write-Host '   Copiá esa carpeta entera a la PC del cliente y ejecutá INSTALAR.bat.'
Write-Host ''
