# Qué le falta al sistema

Lista ordenada por lo que duele, no por lo que suena lindo. El número es el
orden en que conviene hacerlo. El esfuerzo es grueso y en días de trabajo.

---

## Bloqueantes: sin esto no se puede abrir el local

### 1. Backups automáticos — 1 día

- [ ] Copia diaria de la base con fecha en el nombre
- [ ] Retención configurable (ej. últimos 30 días) y borrado de las viejas
- [ ] Copia espejo a pendrive o carpeta de red
- [ ] Botón "Respaldar ahora" en Configuración
- [ ] Aviso si hace más de N días que no hay backup

Hoy todo el negocio vive en **un archivo**. Disco que falla, ransomware, alguien
que borra la carpeta, OneDrive que sincroniza una versión rota: se pierden
ventas, stock, clientes y la historia entera, sin vuelta atrás. Ninguna otra
cosa de esta lista importa si falta ésta.

### 2. Imprimir el ticket — 1 a 2 días

- [ ] Ticket de venta en impresora térmica (58/80 mm) y en A4
- [ ] Enganchar `TICKET_PIE` y los datos del negocio, que ya se configuran y hoy no se usan en ningún lado
- [ ] Reimprimir desde Historial de ventas
- [ ] Imprimir el cierre de caja

No hay **nada** de impresión en el sistema. Si el cliente pide comprobante, no
hay. Para cotillón alcanza con ticket simple: no hace falta impresora fiscal
salvo que AFIP lo exija.

### 3. Instalador y servicio de Windows — 2 a 3 días

- [ ] Instalador real (no copiar una carpeta)
- [ ] La API registrada como servicio de Windows, que arranca sola con la PC
- [ ] Configuración del puesto (servidor o terminal) desde el instalador
- [ ] Actualización sin reinstalar todo

Hoy la API se apaga cuando se cierra el cliente. Si la PC se reinicia un sábado
a la mañana, alguien tiene que saber qué ejecutable abrir.

---

## Lo que va a doler el primer mes

### 4. Aumento masivo de precios — 1 día

- [ ] "+X %" a una categoría, una sección o un proveedor
- [ ] Vista previa de qué queda en cuánto, antes de aplicar
- [ ] Redondeo configurable (a $10, a $100)
- [ ] Queda registrado en Auditoría qué se cambió y cuándo

El proveedor aumenta 12 % y hay 400 productos: hoy son 400 formularios. De las
grandes es la más barata de hacer y la que más tiempo devuelve por semana.

### 5. Cuenta corriente de clientes — 2 a 3 días

- [ ] Saldo por cliente (la tabla de clientes hoy no tiene saldo)
- [ ] Venta a cuenta corriente como medio de pago
- [ ] Registrar pagos y imputarlos
- [ ] Resumen de cuenta imprimible
- [ ] Reporte de quién debe y desde cuándo

Jardines, salones y organizadoras compran en el mes y pagan a fin de mes. Hoy
eso se anota en un cuaderno.

### 6. Facturación / AFIP — a definir

- [ ] **Decisión pendiente:** cómo se factura hoy y si hace falta factura electrónica

Es la pieza más grande y la única que no se puede estimar sin esa respuesta.
Puede ir de "no hace falta nada" a varias semanas.

---

## Lo que lo hace *para cotillón* y no un POS genérico

### 7. Combos y bolsitas — 3 a 4 días

- [ ] Un producto que se compone de otros productos
- [ ] Al venderlo descuenta el stock de cada componente
- [ ] Costo y margen del combo calculados desde los componentes
- [ ] Avisar si no se puede armar por falta de un componente

La bolsita sorpresa son 8 cosas distintas que se venden como una. Es *la*
función de cotillón y hoy no se puede representar.

**Pregunta abierta:** ¿las bolsitas se arman antes y se cargan como un producto
con su propio stock, o se arman en el momento según lo que pide el cliente? La
respuesta cambia el diseño.

### 8. Bulto y unidad — 2 días

- [ ] Conversión por producto (1 bulto = 50 unidades)
- [ ] Comprar por bulto y vender por unidad sin que el stock se desarme
- [ ] Precio por bulto y precio por unidad

Los globos vienen en bolsa de 50 y se vende la bolsa **y** el globo suelto. Es
hermano de lo que ya está hecho con kilo/gramos: la mitad del camino existe.

### 9. Encargues con seña — 3 días

- [ ] Pedido con fecha de entrega
- [ ] Seña cobrada y saldo pendiente
- [ ] Lista de "qué hay que entregar esta semana"
- [ ] Al entregar se convierte en venta y descuenta stock

"Me preparás 60 bolsitas para el 15, te dejo seña." En cotillón es diario.

### 10. Temporadas — 2 días

- [ ] Marca de temporada en el producto (Halloween, Navidad, Día del Niño, carnaval)
- [ ] Reporte comparando contra la misma temporada del año anterior

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
de las pantallas de mirar.

137 tests automáticos.
