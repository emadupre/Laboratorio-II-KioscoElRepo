# KioscoPOS — Narrativa del Proyecto y Modelo de Datos

**Trabajo Práctico Integrador Final — Laboratorio II**

---

## 1. Narrativa del Proyecto y Descripción del Problema

### 1.1. Datos del Grupo

| Integrante | Email |
|---|---|
| Bossa, José | jose.bossa.3@gmail.com |
| Suarez, Fernando | jorgefernandosuarez@gmail.com |
| García, Jesús Emanuel | dupre.dev@gmail.com |

### 1.2. Título / Nombre del Proyecto y Temática

**Título:** KioscoPOS: Sistema de Gestión Integral para Kioscos 24hs

**Temática:** Sistema de Punto de Venta (POS) y gestión de inventario para comercio minorista (kiosco 24 horas).

**Stack tecnológico:**
- ASP.NET Core MVC + Identity + Entity Framework Core
- MySQL 8 (Pomelo EF Provider)
- Razor Views + Bootstrap 5 + Vue.js 3 (módulos SPA)
- Autenticación mixta: cookies (Web) + JWT Bearer (API REST)

### 1.3. Descripción de la Problemática

Un kiosco que opera las 24 horas presenta las siguientes necesidades y problemas operativos:

- **Control de turnos y cajas:** trabaja con tres turnos (mañana, tarde, noche) y cajeros distintos. Necesita saber con precisión quién abrió y cerró la caja, y que el arqueo de dinero cuadre al finalizar cada turno.

- **Gestión de stock deficiente:** vende una amplia variedad de productos (gaseosas, cigarrillos, golosinas, snacks, almacén). El stock se descontrola constantemente porque no hay un seguimiento automatizado.

- **Compras a proveedores:** adquiere mercadería a varios proveedores. Necesita registrar esas compras para que el stock y el costo de los productos se actualicen automáticamente.

- **Falta de alertas:** no cuenta con un sistema que avise cuándo un producto está por agotarse.

- **Seguridad y auditoría:** cada cajero debe tener su propio usuario. No todos deben tener permisos para ver costos o borrar datos. Debe quedar registro de quién realizó cada venta.

- **Movimientos de efectivo:** a veces el cajero saca plata del cajón para pagar a un proveedor o ingresa cambio. Estos movimientos deben quedar registrados para que el arqueo cierre correctamente.

- **Anulación de ventas:** si hay que anular una venta mal cargada, el sistema debe revertir el stock automáticamente y dejar la marca de quién la anuló y por qué.

### 1.4. Propuesta de Solución

Se propone un sistema web MVC con las siguientes funcionalidades:

- **Autenticación con roles** (`Administrador` y `Cajero`) usando ASP.NET Core Identity. El rol Administrador tiene acceso total; el rol Cajero solo puede operar ventas, caja y consultar productos.

- **Gestión de productos** con categoría, marca, código de barras, precios de costo y venta, stock actual/mínimo e imagen.

- **Módulo de ventas** con carrito interactivo (Vue.js) que descuenta stock automáticamente y valida disponibilidad antes de confirmar.

- **Módulo de compras** (solo Administrador) que actualiza stock y el último costo de compra del producto.

- **Control de caja por turno** con cálculo automático del efectivo esperado y arqueo al cierre. El `TotalEsperado` se calcula como `FondoInicial + VentasEfectivo + Ingresos - Retiros`.

- **Auditoría completa** de todos los movimientos (quién, cuándo, por qué) a través de las tablas `MovimientoStock` y `MovimientoCaja`, más las FK de auditoría en `Venta`, `Compra` y `Caja`.

- **Búsqueda asíncrona** de productos, categorías y marcas mediante AJAX (autocompletar con debounce de 300ms).

- **Alertas de stock bajo** en el dashboard y en el listado de productos.

- **API REST con JWT** para integración externa (login, productos, categorías, marcas).

### 1.5. Reglas de Negocio

Las siguientes reglas se implementan en la capa de servicios y se reflejan en el modelo de datos:

1. **No se puede vender sin caja abierta.** `VentaService.ConfirmarAsync` valida `ICajaService.ObtenerCajaAbiertaAsync`.
2. **El stock nunca puede quedar negativo.** `CHECK CONSTRAINT` en la base + validación en `StockService.RegistrarMovimientoAsync`.
3. **Al confirmar una compra, se actualiza el `PrecioCosto` del producto** con el último costo cargado.
4. **Al anular una venta o compra, se revierte el stock** y queda registro de la anulación (`AnuladaPorId`, `FechaAnulacion`, `MotivoAnulacion`).
5. **El `TotalEsperado` de la caja** = `FondoInicial + VentasEfectivo + Ingresos - Retiros`.
6. **Un usuario no puede tener dos cajas abiertas simultáneamente.**
7. **Solo el dueño de la caja puede cerrarla** y registrar movimientos.
8. **No se puede anular una venta de una caja ya cerrada.**
9. **Un retiro no puede exceder el efectivo disponible en la caja.**
10. **Solo el rol Administrador** puede ver `PrecioCosto`, borrar catálogos, confirmar compras y anular ventas.

---

## 2. Modelo de Datos y Diagrama Entidad-Relación

### 2.1. Diagrama Entidad-Relación

El DER completo, junto con la descripción detallada de cada entidad y relación, se encuentra en el documento **[diagrama-er.md](diagrama-er.md)**.

Resumen de entidades (12 en total, más las tablas de Identity):

| # | Entidad | Rol en el sistema |
|---|---|---|
| 1 | Categoría | Rubros de productos |
| 2 | Marca | Marcas comerciales |
| 3 | Producto | Ítems vendibles |
| 4 | Proveedor | Empresas proveedoras |
| 5 | Compra | Cabecera de ingreso de mercadería |
| 6 | DetalleCompra | Líneas de una compra |
| 7 | Venta | Cabecera de transacción de salida |
| 8 | DetalleVenta | Líneas de una venta |
| 9 | Caja | Turno de un cajero |
| 10 | MovimientoCaja | Ingresos/retiros de efectivo |
| 11 | MovimientoStock | Auditoría de stock |
| 12 | Usuario | Personal (`ApplicationUser : IdentityUser`) |

### 2.2. Relaciones principales

**Relaciones 1:N:**
- Categoría → Producto
- Marca → Producto
- Proveedor → Compra
- Compra → DetalleCompra
- Venta → DetalleVenta
- Caja → Venta
- Caja → MovimientoCaja
- Producto → MovimientoStock

**Relaciones N:M resueltas con tabla intermedia:**
- Producto ↔ Venta (a través de `DetalleVenta`, con atributos `Cantidad` y `PrecioUnitario`)
- Producto ↔ Compra (a través de `DetalleCompra`, con atributos `Cantidad` y `CostoUnitario`)

**Relaciones de auditoría (FK a `ApplicationUser`):**
- Usuario → Venta (`UsuarioId` + `AnuladaPorId`)
- Usuario → Compra (`UsuarioId` + `AnuladaPorId`)
- Usuario → Caja
- Usuario → MovimientoCaja
- Usuario → MovimientoStock

### 2.3. Decisiones de diseño destacadas

- **Agregado de `Marca` como entidad independiente** (no como atributo de `Producto`): permite estandarizar información, evitar duplicados por errores de tipeo y agilizar la búsqueda en el POS. Un kiosco vende la misma categoría ("Cigarrillos") con múltiples marcas ("Marlboro", "Philip Morris"), y los clientes piden por marca específica.

- **`RowVersion` en `Producto`** como token de concurrencia optimista: protege contra condiciones de carrera cuando dos cajeros venden el mismo producto simultáneamente. Si el stock cambió entre la lectura y la escritura, EF Core lanza `DbUpdateConcurrencyException` y la transacción se revierte.

- **Doble FK a `ApplicationUser` en `Venta` y `Compra`**: `UsuarioId` para el creador, `AnuladaPorId` para el anulador (opcional). Permite trazabilidad completa sin tablas de auditoría adicionales.

- **Tablas `MovimientoStock` y `MovimientoCaja`** separadas del estado principal: actúan como logs append-only. El estado actual de `Producto.StockActual` y `Caja.TotalEsperado` se puede reconstruir siempre a partir de los movimientos.

- **Abstracción de las tablas de Identity** en el DER conceptual: las tablas `AspNetUsers`, `AspNetRoles`, etc., son infraestructura del framework y no se incluyen en el diagrama de dominio (ver apéndice en `diagrama-er.md`).

---

## 3. Requisitos Técnicos Cumplidos

| Requerimiento | Estado | Implementación |
|---|---|---|
| ≥6 tablas con 1:N y N:M | ✅ | 12 entidades (más Identity). N:M resuelta vía `DetalleVenta` / `DetalleCompra`. |
| Login / Logout con Identity | ✅ | `CuentaController` + `SignInManager` |
| Roles y restricción por rol | ✅ | `[Authorize(Roles = "Administrador")]` en acciones críticas |
| Avatar de usuario | ✅ | Subida a `wwwroot/uploads/avatares/` con validación |
| Manejo de archivos | ✅ | Imágenes de producto con validación de extensión y tamaño |
| ABM con Vue.js + AJAX | ✅ | Dos módulos: Categorías y Marcas |
| Paginado por servidor | ✅ | `Skip/Take` en todos los listados principales |
| Búsqueda asíncrona | ✅ | `/Productos/Buscar`, `/Categorias/Buscar`, `/Marcas/Buscar`, `/Busqueda/Productos` |
| Web API + JWT | ✅ | `KioscoPOS.Api` con `POST /api/auth/login` y endpoints protegidos |

---

## 4. Cómo ejecutar el proyecto

### Requisitos previos
- .NET 9 SDK
- MySQL 8

### Pasos

```bash
# 1. Clonar el repositorio
git clone <URL_DEL_REPO>
cd Laboratorio-II-KioscoElRepo

# 2. Restaurar la base de datos
mysql -u root -p < database/kioscopos-dump.sql

# 3. Configurar la connection string en:
#    - KioscoPOS.Web/appsettings.json
#    - KioscoPOS.Api/appsettings.json

# 4. Correr la Web
cd KioscoPOS.Web
dotnet run

# 5. Correr la API (otra terminal)
cd ../KioscoPOS.Api
dotnet run
```

### Usuarios de prueba

| Rol | Email | Contraseña |
|---|---|---|
| Administrador | `admin@kiosco.com` | `Admin#2026` |
| Cajero | `cajero@kiosco.com` | `Cajero#2026` |

---

*Última actualización: Octubre 2026*