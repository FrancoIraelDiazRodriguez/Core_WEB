# Core Web API

API REST en **ASP.NET Core 10** — migración de una API existente en **Laravel** a .NET.
Autenticación con **JWT Bearer** y autorización dinámica por permisos sobre **ASP.NET Core Identity**.

> Proyecto en desarrollo activo. Ver [CLAUDE.md](CLAUDE.md) para el estado detallado y las decisiones de diseño.

## Stack

- .NET 10 / ASP.NET Core Web API (sin MVC ni Razor Pages)
- EF Core 10 — SQL Server en producción, **InMemory** en desarrollo
- ASP.NET Core Identity (`AddIdentityCore`, sin UI)
- JWT Bearer + refresh tokens en base de datos

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server / LocalDB **solo** si vas a usar el proveedor `SqlServer` (en dev no hace falta)

## Configuración

La clave de firma JWT **no está en el repositorio** (es un secreto). Configúrala con user-secrets:

```bash
dotnet user-secrets set "Jwt:Key" "TU_CLAVE_DE_MINIMO_32_CARACTERES"
```

El proveedor de base de datos se elige en configuración (`DatabaseProvider`):
- `appsettings.Development.json` → `"DatabaseProvider": "InMemory"` (por defecto en dev)
- `appsettings.json` → `"SqlServer"` (producción)

## Cómo correr (desarrollo, sin SQL Server)

```bash
dotnet run --launch-profile http
```

Arranca en `http://localhost:5011`. Al iniciar, el seeder crea:
- Rol **Admin**
- Usuario **admin@coreweb.com** / **Admin123!**

> La base InMemory vive en RAM: se borra al reiniciar y el seeder la recrea.

## Endpoints disponibles

### `POST /api/auth/login`
```bash
curl -X POST http://localhost:5011/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@coreweb.com","password":"Admin123!"}'
```
Respuesta `200`: `{ access_token, token_type, expires_in }`.

### `POST /api/auth/change-password` — requiere `Authorization: Bearer <token>`
```bash
curl -X POST http://localhost:5011/api/auth/change-password \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <access_token>" \
  -d '{"currentPassword":"Admin123!","newPassword":"NuevaClave123!"}'
```
`204` si cambia · `400` con lista de errores si la clave actual es incorrecta o la nueva no
cumple la política · `401` sin token.

## Arquitectura

- **Controllers flacos**: solo traducen HTTP ↔ dominio. La lógica vive en la capa de servicio.
- **Servicios agnósticos del transporte**: devuelven resultados de dominio (no `IActionResult`)
  y reciben primitivos (no `HttpContext`/`ClaimsPrincipal`).
- **Autorización en dos mitades**: un atributo `[RequiresPermission(module, action)]` declara
  qué exige cada endpoint; la tabla `permissions` declara quién lo tiene (editable sin recompilar).
- **Refresh tokens** aleatorios (CSPRNG) guardados **hasheados** (SHA-256) en BD.

## Estructura

```
Controllers/        Endpoints HTTP (flacos)
Services/           Interfaces/ + Implementations/  (lógica de negocio)
Models/Security/    Entidades: ApplicationUser, ApplicationRole, AppRoute, Log, RefreshToken
Dtos/Security/      Entrada/salida y resultados de dominio
Data/               CoreContext + Seeders
Security/           PermissionRequirement / PermissionHandler
Migrations/         Migraciones EF (SqlServer)
```

## Roadmap

Login ✅ · ChangePassword ✅ · RefreshToken (en curso) · Refresh/Logout · `/me` ·
recuperación de contraseña · auditoría · auto-descubrimiento de permisos · controlador genérico.
Detalle en [CLAUDE.md](CLAUDE.md).
