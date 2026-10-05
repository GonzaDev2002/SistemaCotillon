# Qué le falta al sistema

Lista ordenada por lo que duele, no por lo que suena lindo. El número es el
orden en que conviene hacerlo. El esfuerzo es grueso y en días de trabajo.

---

## Bloqueantes: sin esto no se puede abrir el local

### 1. Backups automáticos — HECHO

- [x] Copia automática con fecha en el nombre, cada 1/2/3/7 días
- [x] Retención configurable (7, 30 o 90 copias) y borrado de las viejas
- [x] Copia a la carpeta que sincroniza con la nube (OneDrive, Drive, Dropbox)
- [x] Botón "Respaldar ahora" en Configuración y en Inicio
- [x] Aviso en rojo en Inicio si hace más días que la frecuencia elegida
- [x] La copia se verifica con PRAGMA integrity_check antes de darla por buena
- [x] Los adjuntos (PDF e imágenes) van en el mismo zip
- [ ] Restaurar desde la aplicación (hoy es manual: ver abajo)

La copia se saca con VACUUM INTO, que es la única forma correcta de copiar una
base SQLite en uso. File.Copy del archivo .db produce una copia inservible: con
WAL activado parte de los datos está en el archivo -wal, y encima se copia
mientras alguien escribe. Esa copia abre, parece normal, y falla el día que
hace falta.

**Para restaurar** (procedimiento manual, hasta que esté en la aplicación):

1. Cerrar la aplicación en todos los puestos y detener el servicio.
2. Renombrar la base, que vive en la carpeta ProgramData del sistema (no borrarla todavía).
3. Descomprimir el zip del respaldo: deja `cotillon.db` y la carpeta `Adjuntos`.
4. Copiar los dos a esa misma carpeta, reemplazando lo que haya.
5. Arrancar el servicio y verificar que estén las ventas del día del respaldo.

### 2. Imprimir el ticket — HECHO

- [x] Ticket de venta en impresora térmica (58/80 mm) y en A4
- [x] Enganchar `TICKET_PIE` y los datos del negocio
- [x] Reimprimir desde Historial de ventas
- [x] Imprimir el cierre de caja
- [x] Elegir el ancho del papel y si sale solo al cobrar, desde Configuración

El ticket se arma como texto de ancho fijo, que es lo que imprime una térmica.
La impresora se instala en Windows como cualquier otra: no hace falta librería
ni driver especial. No hace falta impresora fiscal salvo que AFIP lo exija.

### 3. Instalador y servicio de Windows — HECHO

- [x] Instalador: doble clic en INSTALAR.bat, pide administrador solo
- [x] La API registrada como servicio de Windows, arranca con la PC y se
      reinicia sola si se cae
- [x] Configuración del puesto (servidor o terminal) desde el instalador
- [x] Actualización sin reinstalar: se corre encima y no toca la base
- [x] Regla de firewall sólo en redes privadas
- [x] Accesos directos, desinstalador y LEEME con los procedimientos
- [x] Publicado autocontenido: no hace falta instalar .NET en la PC del cliente

Se empaqueta con `Instaladormpaquetar.ps1`, que deja una carpeta lista para
llevar al cliente. Los pasos de instalación, actualización y restauración están
en `Instaladoreeme.txt`.

el servicio es lo que hace que la caja ande después de un reinicio, que los
respaldos salgan aunque el local esté cerrado, y que el administrador pueda
entrar desde afuera: antes la api se apagaba cuando se cerraba la aplicación.

---

## Lo que va a doler el primer mes

### 4. Aumento masivo de precios — HECHO

- [x] "+X %" por sección, categoría, marca o temporada
- [x] Vista previa de qué queda en cuánto, antes de aplicar
- [x] Redondeo configurable (a $10, a $100)
- [x] Queda registrado en Auditoría qué se cambió y cuándo

El proveedor aumenta 12 % y hay 400 productos: hoy son 400 formularios. De las
grandes es la más barata de hacer y la que más tiempo devuelve por semana.

### 5. Cuenta corriente de clientes — HECHO (salvo imprimir el resumen)

- [x] Saldo por cliente (la tabla de clientes hoy no tiene saldo)
- [x] Venta a cuenta corriente como medio de pago
- [x] Registrar pagos y imputarlos
- [ ] Resumen de cuenta imprimible (se ve en pantalla; falta el ticket)
- [x] Reporte de quién debe y desde cuándo

Jardines, salones y organizadoras compran en el mes y pagan a fin de mes. Hoy
eso se anota en un cuaderno.

### 6. Facturación / AFIP — a definir

- [ ] **Decisión pendiente:** cómo se factura hoy y si hace falta factura electrónica

Es la pieza más grande y la única que no se puede estimar sin esa respuesta.
Puede ir de "no hace falta nada" a varias semanas.

---

## Lo que lo hace *para cotillón* y no un POS genérico

### 7. Combos y bolsitas — HECHO

- [x] Un producto que se compone de otros productos
- [x] Al venderlo descuenta el stock de cada componente
- [x] Costo y margen del combo calculados desde los componentes
- [x] Avisar si no se puede armar por falta de un componente

La bolsita sorpresa son 8 cosas distintas que se venden como una. Es *la*
función de cotillón y hoy no se puede representar.

**Decidido:** se arman en el momento. El combo no tiene stock propio y su
disponible sale de los componentes. Si en la práctica las armás con
anticipación y querés que tengan stock propio, hay que agregar una operación
de "armar N combos" que convierta componentes en stock del combo.

### 8. Bulto y unidad — HECHO

- [x] Conversión por producto (1 bulto = 50 unidades)
- [x] Comprar por bulto y vender por unidad sin que el stock se desarme
- [x] Precio por bulto y precio por unidad

Los globos vienen en bolsa de 50 y se vende la bolsa **y** el globo suelto. Es
hermano de lo que ya está hecho con kilo/gramos: la mitad del camino existe.

### 9. Encargues con seña — HECHO

- [x] Pedido con fecha de entrega
- [x] Seña cobrada y saldo pendiente
- [x] Lista de "qué hay que entregar esta semana"
- [x] Al entregar se convierte en venta y descuenta stock

"Me preparás 60 bolsitas para el 15, te dejo seña." En cotillón es diario.

### 10. Temporadas — HECHO

- [x] Marca de temporada en el producto (Halloween, Navidad, Día del Niño, carnaval)
- [x] Reporte comparando contra la misma temporada del año anterior

Es la comparación que importa en un negocio estacional, y los reportes de hoy
no la tienen.

---

## Más chicas

- [ ] **11.** Imprimir etiquetas con código de barras, para los productos a granel que no traen código — 1 día
- [ ] **12.** Rentabilidad consolidada por categoría (el margen por producto ya está, falta sumarlo) — medio día
- [ ] **13.** Arqueo de caja ciego: que el cajero no vea el esperado antes de contar — medio día
- [ ] **14.** Decodificar las etiquetas de la balanza con el prefijo real, cuando se sepa qué balanza es — ya está hecho, falta configurar la máscara

---

## Mantenimiento pendiente

- [ ] **Sacar el repo de OneDrive.** Ya bloqueó el build dos veces. Mover a `C:\Dev\Cotillon.Local` conservando el historial de git.
- [ ] **Borrar la copia corrupta** en `...\Proyectos\Cotillon.Local`, que algún día se va a abrir por error.

---

## Nota de seguridad

El sistema de permisos es un **control operativo, no una barrera de seguridad**.
La API local no pide credenciales en cada llamada: evita errores y curiosidad,
pero no a alguien que le pegue directo a la API o que tenga acceso a la PC
servidor. Para que sea una barrera real hace falta token en cada request.

No es urgente mientras todo viva en la red del local. Sí lo es el día que se
quiera entrar desde afuera.

---

## Lo que ya está

Ventas con lector y sin mouse · venta por peso y por metro · lectura de
etiquetas de balanza · caja · historial y anulación · productos con alta en
cadena · importación de lista desde Excel · secciones y categorías · stock con
trazabilidad · compras · clientes · proveedores · reportes · usuarios con
permisos por módulo · auditoría · adjuntos (PDF e imágenes) · refresco en vivo
de las pantallas de mirar · ticket y cierre de caja impresos · combos · bulto ·
temporadas · cuenta corriente · encargues con seña · aumento masivo de precios ·
color de pantalla según el rol · calculadora de precios · controles de caja ·
respaldos automáticos con copia a la nube.

234 tests automáticos.
