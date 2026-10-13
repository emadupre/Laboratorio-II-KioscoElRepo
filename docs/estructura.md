# Estructura del proyecto - KioscoPOS

## Vista general

Este repositorio corresponde a un sistema POS (Point of Sale) para un kiosco 24hs. La solución está organizada en dos proyectos principales:

- `KioscoPOS.Web`: aplicación web de gestión y operaciones del negocio.
- `KioscoPOS.Api`: API para servicios y lógica de negocio, si se expande o se conecta con frontend/backend separado.
- `database`: recursos relacionados con la base de datos y datos persistentes.
- `docs`: documentación funcional y técnica del proyecto.
- `postman`: colecciones o pruebas de endpoints.

## Árbol de archivos

```text
Laboratorio-II-KioscoElRepo/
├── .gitignore
├── .git/
├── README.md
├── KioscoPOS.slnx
├── KioscoPOS.Api/
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── KioscoPOS.Api.csproj
│   ├── KioscoPOS.Api.http
│   ├── Program.cs
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── Controllers/              # Endpoints o servicios HTTP (a completar)
│   ├── Data/                     # Contexto de datos y acceso a BD
│   └── Models/                   # Entidades del dominio
├── KioscoPOS.Web/
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── KioscoPOS.Web.csproj
│   ├── Program.cs
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── Controllers/              # Controladores MVC
│   │   ├── CajaController.cs
│   │   ├── CategoriasController.cs
│   │   ├── ComprasController.cs
│   │   ├── CuentaController.cs
│   │   ├── InicioController.cs
│   │   ├── MovimientosCajaController.cs
│   │   ├── ProductosController.cs
│   │   ├── ProveedoresController.cs
│   │   └── VentasController.cs
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   └── Seed/
│   │       └── DbSeeder.cs
│   ├── Models/                  # Entidades y modelos del sistema
│   │   ├── ApplicationUser.cs
│   │   ├── Caja.cs
│   │   ├── Categoria.cs
│   │   ├── Compra.cs
│   │   ├── DetalleCompra.cs
│   │   ├── DetalleVenta.cs
│   │   ├── Enums/
│   │   ├── ErrorViewModel.cs
│   │   ├── MovimientoCaja.cs
│   │   ├── MovimientoStock.cs
│   │   ├── Paginado.cs
│   │   ├── Producto.cs
│   │   ├── ProductoviewModel.cs
│   │   ├── Proveedor.cs
│   │   └── Venta.cs
│   ├── ViewModels/
│   │   ├── IniciarSesionVM.cs
│   │   ├── PagedResult.cs
│   │   └── ProductoVM.cs
│   ├── Views/
│   │   ├── Caja/
│   │   ├── Categorias/
│   │   ├── Compras/
│   │   ├── Cuenta/
│   │   ├── Inicio/
│   │   ├── MovimientoCaja/
│   │   ├── Productos/
│   │   ├── Proveedores/
│   │   ├── Shared/
│   │   ├── Ventas/
│   │   ├── _ViewImports.cshtml
│   │   └── _ViewStart.cshtml
│   ├── Migrations/
│   │   ├── ApplicationDbContextModelSnapshot.cs
│   │   └── ...
│   ├── wwwroot/
│   │   ├── css/
│   │   ├── js/
│   │   └── lib/
│   └── obj/
│       └── bin/
├── database/
├── docs/
│   ├── decisiones.md
│   ├── diagrama-er.md
│   ├── estructura.md
│   ├── narrativa.md
│   └── requisitos.md
├── postman/
└── README.md
```

## Descripción por carpeta

### `KioscoPOS.Web`
Es la capa de presentación del sistema. Aquí se encuentra la interfaz del kiosco, con:

- Controladores MVC para ventas, compras, productos, proveedores, caja, categorías y login.
- Modelos de dominio.
- ViewModels para transferir datos entre capa de presentación y lógica.
- Vistas Razor (`.cshtml`) para la UI.
- `Data/ApplicationDbContext.cs` para acceso a la base de datos.
- `Migrations/` para versionado de esquema.
- `wwwroot/` para recursos estáticos (CSS, JS, librerías).

### `KioscoPOS.Api`
Es la capa backend/servicios del proyecto. Su finalidad es proporcionar una API para integrar con otras aplicaciones o funcionalidades externas.

Estructura esperada:

- `Controllers/`: endpoints REST.
- `Data/`: contexto y acceso a datos.
- `Models/`: entidades y DTOs principales.
- `Program.cs`: configuración del host y servicios.

### `database`
Se usa para almacenar scripts, datos y soporte de persistencia.

### `docs`
Documentación del proyecto:

- requisitos funcionales
- decisiones de diseño
- diagrama entidad-relación
- narrativa del negocio
- estructura del proyecto

### `postman`
Carpeta destinada a pruebas de endpoints, importación de colecciones o validación de API.

## Entidades principales del dominio

El proyecto contempla modelos del negocio POS, entre los más relevantes:

- `Categoria`
- `Producto`
- `Proveedor`
- `Compra`
- `DetalleCompra`
- `Venta`
- `DetalleVenta`
- `Caja`
- `MovimientoCaja`
- `MovimientoStock`
- `Usuario` / `ApplicationUser`

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

## Nota

Algunas carpetas como `Controllers/` y `Data/` dentro de `KioscoPOS.Api` pueden estar aún en proceso de implementación o completarse a medida que avance el proyecto.
