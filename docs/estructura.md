# Estructura del proyecto - KioscoPOS

Este documento describe la organización del repositorio, las capas de la aplicación y las convenciones de carpetas.

---

## Vista general

Este repositorio corresponde a un sistema POS (Point of Sale) para un kiosco 24hs. La solución está organizada en dos proyectos principales:

- `KioscoPOS.Web`: aplicación web de gestión y operaciones del negocio.
- `KioscoPOS.Api`: API para servicios y lógica de negocio, si se expande o se conecta con frontend/backend separado.
- `database`: recursos relacionados con la base de datos y datos persistentes.
- `docs`: documentación funcional y técnica del proyecto.
- `postman`: colecciones o pruebas de endpoints.
- `README.md`: matriz de evidencias y guía de ejecución.

Ambos proyectos **comparten** los modelos (`Models/`) y el `ApplicationDbContext` del proyecto Web a través de `<ProjectReference>` en el `.csproj` de la API. Esto evita duplicación de código y asegura una únic fuente de verdad para el esquema de base de datos.

---

## Árbol de archivos

```text
Laboratorio-II-KioscoElRepo/
├── .gitignore
├── README.md
├── KioscoPOS.slnx
│
├── KioscoPOS.Api/                    # API REST con JWT
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── KioscoPOS.Api.csproj          # referencia a KioscoPOS.Web.csproj
│   ├── KioscoPOS.Api.http            # archivo de requests HTTP (opcional)
│   ├── Program.cs                    # configuración de JWT, Identity y DbContext
│   ├── Properties/
│   │   └── launchSettings.json
│   └── Controllers/
│       ├── AuthController.cs         # POST /api/auth/login
│       ├── ProductosApiController.cs # GET /api/productos, /api/productos/{id}
│       ├── CategoriasApiController.cs# GET /api/categorias, /api/categorias/{id}
│       └── MarcasApiController.cs    # GET /api/marcas, /api/marcas/{id}
│
├── KioscoPOS.Web/                    # Aplicación MVC
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── KioscoPOS.Web.csproj
│   ├── Program.cs                    # Identity + cookies + DI de services
│   ├── Properties/
│   │   └── launchSettings.json
│   │
│   ├── Controllers/
│   │   ├── BusquedaController.cs     # autocompletar productos, búsqueda por código de barras
│   │   ├── CajasController.cs        # apertura, cierre, historial de cajas
│   │   ├── CategoriasController.cs   # CRUD + endpoints JSON para Vue
│   │   ├── ComprasController.cs      # borrador, confirmación, anulación (solo admin)
│   │   ├── CuentaController.cs       # login, logout, perfil (avatar)
│   │   ├── InicioController.cs       # dashboard con KPIs
│   │   ├── MarcasController.cs       # CRUD + endpoints JSON para Vue
│   │   ├── MovimientosCajaController.cs # ingresos/retiros por turno
│   │   ├── ProductosController.cs    # CRUD con manejo de imágenes
│   │   ├── ProveedoresController.cs  # CRUD con validación de CUIT
│   │   ├── StockController.cs        # historial de movimientos + ajuste manual
│   │   └── VentasController.cs       # carrito, confirmar, anular, detalle
│   │
│   ├── Data/
│   │   ├── ApplicationDbContext.cs   # DbSets + configuración de relaciones
│   │   └── Seed/
│   │       └── DbSeeder.cs           # roles, usuarios demo, categorías, marcas
│   │
│   ├── Models/                       # Entidades del dominio
│   │   ├── ApplicationUser.cs        # hereda de IdentityUser
│   │   ├── Caja.cs
│   │   ├── Categoria.cs
│   │   ├── Compra.cs
│   │   ├── DetalleCompra.cs
│   │   ├── DetalleVenta.cs
│   │   ├── ErrorViewModel.cs
│   │   ├── Marca.cs
│   │   ├── MovimientoCaja.cs
│   │   ├── MovimientoStock.cs
│   │   ├── Paginado.cs               # PagedResult<T>
│   │   ├── Producto.cs
│   │   ├── Proveedor.cs
│   │   ├── Venta.cs
│   │   └── Enums/
│   │       └── Enum.cs               # Turno, MedioPago, EstadoVenta, EstadoCompra,
│   │                                 # TipoMovimientoCaja, TipoMovimientoStock, Roles
│   │
│   ├── Services/                     # Lógica de negocio
│   │   ├── StockService.cs           # IStockService + implementación
│   │   ├── StockException.cs
│   │   ├── CajaService.cs            # ICajaService + CajaException + ResumenCaja
│   │   ├── CompraService.cs          # IComprasService + implementación
│   │   └── VentaService.cs           # IVentaService + VentaException + ItemVenta
│   │
│   ├── ViewModels/                   # DTOs de presentación
│   │   ├── AjusteStockVM.cs
│   │   ├── CajaPanelVM.cs
│   │   ├── CompraCrearVM.cs
│   │   ├── IniciarSesionVM.cs
│   │   ├── InicioVM.cs
│   │   ├── MovimientosCajaVM.cs
│   │   ├── PerfilVM.cs
│   │   └── ProductoViewModel.cs
│   │
│   ├── Views/
│   │   ├── Cajas/
│   │   │   ├── Index.cshtml          # panel (abrir / cerrar / resumen)
│   │   │   └── Historial.cshtml
│   │   ├── Categorias/
│   │   │   ├── Index.cshtml
│   │   │   ├── Create.cshtml
│   │   │   ├── Edit.cshtml
│   │   │   ├── Gestion.cshtml        # ABM con Vue.js
│   │   │   └── _Form.cshtml
│   │   ├── Compras/
│   │   │   ├── Index.cshtml
│   │   │   ├── Create.cshtml
│   │   │   └── Detalle.cshtml
│   │   ├── Cuenta/
│   │   │   ├── IniciarSesion.cshtml
│   │   │   ├── AccesoDenegado.cshtml
│   │   │   └── Perfil.cshtml         # editar nombre + subir avatar
│   │   ├── Inicio/
│   │   │   ├── Index.cshtml          # dashboard con KPIs
│   │   │   └── Privacy.cshtml
│   │   ├── Marcas/
│   │   │   └── Gestion.cshtml        # ABM con Vue.js
│   │   ├── MovimientosCaja/
│   │   │   └── Index.cshtml
│   │   ├── Productos/
│   │   │   ├── Index.cshtml
│   │   │   ├── Create.cshtml
│   │   │   ├── Edit.cshtml
│   │   │   └── _Form.cshtml
│   │   ├── Proveedores/
│   │   │   ├── Index.cshtml
│   │   │   ├── Create.cshtml
│   │   │   ├── Edit.cshtml
│   │   │   └── _Form.cshtml
│   │   ├── Shared/
│   │   │   ├── _Layout.cshtml
│   │   │   ├── _ValidationScriptsPartial.cshtml
│   │   │   └── Error.cshtml
│   │   ├── Stock/
│   │   │   ├── Ajuste.cshtml
│   │   │   └── Historial.cshtml
│   │   ├── Ventas/
│   │   │   ├── Index.cshtml
│   │   │   ├── Nueva.cshtml          # carrito con Vue.js
│   │   │   └── Detalle.cshtml
│   │   ├── _ViewImports.cshtml
│   │   └── _ViewStart.cshtml
│   │
│   ├── Migrations/                   # migraciones de EF Core
│   │   ├── ApplicationDbContextModelSnapshot.cs
│   │   └── <timestamp>_*.cs
│   │
│   └── wwwroot/
│       ├── css/
│       │   └── site.css
│       ├── js/
│       │   ├── categorias-gestion.js # ABM Vue.js de Categorías
│       │   ├── marcas-gestion.js     # ABM Vue.js de Marcas
│       │   ├── ventas-nueva.js       # carrito Vue.js de Ventas
│       │   └── site.js
│       ├── lib/                      # Bootstrap, jQuery, etc.
│       └── uploads/
│           ├── productos/            # imágenes de productos
│           └── avatares/             # avatares de usuarios
│
├── database/
│   └── kioscopos-dump.sql            # dump con estructura + datos de prueba
│
├── docs/
│   ├── narrativa.md                  # problemática, propuesta, reglas de negocio
│   ├── requisitos.md                 # RF, RNF, alcance, casos de uso
│   ├── decisiones.md                 # justificación de decisiones técnicas
│   ├── estructura.md                 # este documento
│   ├── diagrama-er.md                # DER + descripción de entidades y relaciones
│   └── evidencias/                   # capturas y GIFs para el README
│       ├── diagrama-er.png
│       ├── perfil.png
│       ├── subida-imagen.gif
│       ├── abm-vue.gif
│       ├── paginado.png
│       ├── autocomplete.gif
│       └── jwt.png
│
└── postman/
    ├── KioscoPOS-API.postman_collection.json
    └── README.md                     # instrucciones de uso
```

---

## Descripción por carpeta

### `KioscoPOS.Web`

Es la capa de presentación del sistema. Acá se encuentra la interfaz del kiosco, con:

- Controladores MVC para ventas, compras, productos, proveedores, caja, categorías y login.
- Modelos de dominio.
- ViewModels para transferir datos entre capa de presentación y lógica.
- Services que es la logica del negocio. Validciones, transacciones, cálculos.
- Vistas Razor (`.cshtml`) para la UI.
- `Data/ApplicationDbContext.cs` para acceso a la base de datos.
- `Migrations/` para versionado de esquema.
- `wwwroot/` para recursos estáticos (CSS, JS, librerías).

### `KioscoPOS.Api`

Es la capa backend/servicios del proyecto. Su finalidad es proporcionar una API para integrar con otras aplicaciones o funcionalidades externas.

Estructura esperada:

| Subcarpeta     | Responsabilidad                                                                  |
| -------------- | -------------------------------------------------------------------------------- |
| `Controllers/` | Endpoints REST: `/api/auth`, `/api/productos`, `/api/categorias`, `/api/marcas`. |
| `Program.cs`   | Configuración de JWT, Identity Core y DbContext.                                 |

### `database`

Contiene el dump SQL con estructura y datos de prueba (usuarios demo, categorías, marcas).

### `docs`

Documentación del proyecto:

- narrativa del negocio.
- requisitos funcionales.
- decisiones de diseño.
- diagrama entidad-relación.
- estructura del proyecto.
- evidencias referenciaados en el README.

### `postman`

Carpeta destinada a pruebas de endpoints, importación de colecciones o validación de API.

## Entidades principales del dominio

El proyecto contempla modelos del negocio POS, entre los más relevantes:

| Entidad                    | Rol en el sistema                                |
| -------------------------- | ------------------------------------------------ |
| `Categoria`                | Rubros de productos (Bebidas, Cigarrillos, etc.) |
| `Marca`                    | Marcas comerciales (Coca-Cola, Marlboro, etc.)   |
| `Producto`                 | Ítems vendibles con stock, precios, imagen       |
| `Proveedor`                | Empresas proveedoras                             |
| `Compra` + `DetalleCompra` | Ingreso de mercadería                            |
| `Venta` + `DetalleVenta`   | Salida de mercadería                             |
| `Caja`                     | Turno de un cajero (apertura, cierre, arqueo)    |
| `MovimientoCaja`           | Ingresos/retiros de efectivo                     |
| `MovimientoStock`          | Auditoría de cambios de stock                    |
| `ApplicationUser`          | Usuarios (extiende `IdentityUser`)               |

## Flujo general de la solución

1. El usuario interactúa con la aplicación web (`KioscoPOS.Web`).
2. Los controladores manejan las operaciones del negocio.
3. El contexto de datos accede a la base de datos.
4. Se persisten ventas, compras, movimientos de stock y caja.
5. La documentación en `docs/` explica los requisitos y decisiones del proyecto.

## Recomendación de trabajo

Para continuar el desarrollo, conviene mantener esta organización:

- `Web` para frontend y flujo de usuario.
- `Api` para servicios y lógica reutilizable.
- `docs` para especificaciones y decisiones.
- `database` para scripts y migraciones relevantes.
- `postman` para pruebas de APIs.

## Nota sobre `appsettings.Development.json`

El `.gitignore` excluye `appsettings.Development.json` de ambos proyectos. Esto permite tener **overrides locales** (por ejemplo, la password real de MySQL) sin riesgo de filtrarlos al repositorio. En el README se indica cómo configurar la connection string para el primer arranque.
