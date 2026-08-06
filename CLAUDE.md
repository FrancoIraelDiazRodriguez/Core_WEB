# Contexto del proyecto

Migración de una API REST de **Laravel** a **ASP.NET Core 10 Web API**.

El desarrollador viene de Laravel/Eloquent y está **aprendiendo .NET**. El objetivo no es
terminar rápido, es entender. Lee la sección de modo aprendizaje antes de responder nada.

---

## ⚠️ MODO APRENDIZAJE — reglas de interacción

Estas reglas tienen prioridad sobre "resolver la tarea rápido".

1. **No escribas implementaciones completas.** Da el esqueleto con `// TODO:` y pistas de
   qué API buscar. El desarrollador escribe el cuerpo.
2. **Máximo ~20 líneas de código por respuesta**, salvo que se pida explícitamente lo
   contrario (o sea criptografía/seguridad, donde improvisar es peligroso).
3. **Cuando corrijas, explica el por qué**, no solo el qué. El error es la oportunidad
   de aprendizaje.
4. **Usa analogías con Laravel/Eloquent.** Funcionan muy bien con este desarrollador:
   "esto es tu `boot()`", "esto reemplaza a Tymon", "`ToListAsync()` es tu `->get()`".
5. **Antes de dar código, pregunta qué cree que va a fallar.** Si acierta, lo entendió.
6. **Pide que te explique conceptos de vuelta** de vez en cuando ("a ver si lo tengo: el
   atributo es X, el handler Y"). Es la prueba real de comprensión.
7. **Prioriza ejecutar sobre acumular.** No implementes nada nuevo si lo anterior no se
   ha probado corriendo. Insiste en esto: es la debilidad principal detectada.
8. **Un vertical fino antes que muchas capas anchas.** Login + un endpoint protegido
   funcionando de punta a punta > ocho módulos a medias.

---

## Stack

- .NET 10 / ASP.NET Core Web API (no MVC, no Razor Pages)
- EF Core 10 + SQL Server
- ASP.NET Core Identity con `AddIdentityCore` (**sin UI**, es una API)
- JWT Bearer para autenticación
- `Microsoft.OpenApi` **pineado a 2.7.5** — la 3.x rompe el source generator de
  `Microsoft.AspNetCore.OpenApi` (CS0200 en `IOpenApiMediaType.Example`), y la 2.0.0 tiene
  CVE-2026-49451. No cambiar sin motivo.

---

## Estado actual

### Escrito
| Pieza | Notas |
|---|---|
| `IAuditable` | `CreatedAt`, `UpdatedAt` |
| `ApplicationUser : IdentityUser<long>, IAuditable` | + FirstName, LastName, IdentityNumber, Address, IsActive, PasswordExpireAt, Logs |
| `ApplicationRole : IdentityRole<long>, IAuditable` | + Description, IsActive, Routes |
| `AppRoute` | Module, Action, MenuModule, IsActive |
| `Log` | UserId nullable, UserEmail snapshot, IpAddress, UserAgent, AttemptedIdentifier |
| `CoreContext : IdentityDbContext<ApplicationUser, ApplicationRole, long>` | pivote `permissions`, índices únicos, `SetNull` en Logs |
| `ITokenService` / `TokenService` | `CreateToken` → `TokenResult(string AccessToken, int ExpiresIn)`; `GenerateRefreshToken()` (CSPRNG 512-bit Base64); `HashToken()` (SHA-256) |
| `IAuthService` / `AuthService` | capa de servicio; `LoginAsync` → `LoginResult(LoginStatus, TokenResult?, ...)`; `ChangePasswordAsync` |
| `AuthController.Login` | flaco: llama al servicio y traduce `LoginStatus` → HTTP. Con lockout, IsActive, PasswordExpireAt |
| `AuthController.ChangePassword` | `[Authorize]`, saca el userId del token, `UserManager.ChangePasswordAsync` |
| `RefreshToken` (entidad) | registrada en `CoreContext` (`DbSet` + índice único en `Token`) |
| `IRefreshTokenService` / `RefreshTokenService` | `CreateRefreshTokenAsync`: genera plano, guarda el **hash**, devuelve el plano en `RefreshTokenResult` |
| `RequiresPermissionAttribute` | `(string module, string action)` |
| `PermissionRequirement` / `PermissionHandler` | lee el atributo del endpoint; sin atributo = `Succeed()` |
| `IPermissionService` / `PermissionService` | query UserRoles → Roles → Routes |
| `DbSeeder` | rol Admin + usuario `admin@coreweb.com` / `Admin123!`, corre al arrancar |
| Migración `InitialSecurity` | existe (SqlServer). En dev se corre con **InMemory**, sin migración |

### ✅ Probado end-to-end (corriendo en InMemory)
- **Login**: 200 + `access_token` (verificado en jwt.io), 401 con credenciales malas.
- **`ChangePassword`**: 401 sin token, 400 con clave actual mala, 204 con clave correcta,
  y el cambio **persiste** (la clave vieja deja de servir).
- Arranque condicionado por config: `DatabaseProvider=InMemory` en
  `appsettings.Development.json` levanta sin SQL Server; el seeder crea el admin al arrancar.
- Falta: probar el ciclo completo de refresh/logout (ver Pendiente #3).

> **Correr en dev:** `dotnet run --launch-profile http` → `http://localhost:5011`.
> La BD InMemory se borra al reiniciar y el seeder recrea el admin (`Admin123!`).

---

## Decisiones tomadas (no re-litigar sin motivo)

- **Sin clase base `CoreModel`.** El `Id` sale por convención de EF; la auditoría va por
  interfaz `IAuditable` (necesario porque `ApplicationUser` ya hereda de `IdentityUser`
  y C# no permite herencia múltiple).
- **Identity gestiona User↔Role.** No hay navigation property `Roles` en `ApplicationUser`.
- **Autorización dinámica en dos mitades:** el atributo declara *qué permiso exige el
  endpoint* (código, no cambia en caliente); la tabla `permissions` declara *quién lo
  tiene* (datos, el admin lo cambia sin recompilar).
- **`BypassPermissions` descartado.** En su lugar, el auto-descubrimiento asignará las
  rutas nuevas al rol Admin. Un único mecanismo de autorización, sin puertas traseras.
- **`Log` no implementa `IAuditable`** (es inmutable) y `UserId` es nullable (eventos
  anónimos: login fallido, recuperación de contraseña). Guarda `UserEmail` como snapshot
  para que el log siga siendo verdad si el usuario se borra o cambia de email.
- **Fail-closed.** En el handler, no llamar a `Succeed()` = 403. Nunca permitir por defecto.
- **Un solo proveedor de BD (SQL Server).** Multi-proveedor descartado: las migraciones de
  EF Core son específicas del proveedor y `HasFilter("[X] IS NOT NULL")` ya es T-SQL.
- **Access token corto + refresh token en BD.** Un JWT no es revocable; el `auth()->logout()`
  de Laravel funcionaba por la blacklist de Tymon, que .NET no tiene.
- **Capa de servicio devuelve dominio, no HTTP.** Los servicios devuelven resultados de
  dominio (`LoginResult`, `ChangePasswordResult`), **nunca** `IActionResult`. El controller
  traduce a HTTP. Así el negocio es testeable sin `HttpContext`. El controller extrae lo web
  (userId del token, IP) y pasa **primitivos** al servicio.
- **Refresh token hasheado (SHA-256), no en texto plano.** Es aleatorio de alta entropía →
  hash rápido sin salt (no bcrypt: eso es para contraseñas adivinables). El plano solo se
  devuelve al cliente al emitirlo; en BD vive el hash. En el refresh se hashea lo que llega
  y se busca por hash.
- **Token JWT magro.** Lleva id (`sub`) + roles (autorizar sin tocar BD). El perfil (email,
  nombre) NO es fuente de verdad del token → irá por `/me`. Los **permisos** no van en el
  token: se comprueban en vivo en el `PermissionHandler` (revocación inmediata).
- **Dev sin SQL Server.** `DatabaseProvider` en config elige proveedor; `InMemory` en
  `appsettings.Development.json` para probar sin instalar SQL Server. Producción = SqlServer.

---

## Pendiente, en orden

1. ✅ ~~PROBAR el login end-to-end.~~ **Hecho** (login + change-password probados en InMemory).
2. ⚠️ **Seeder**: rol Admin + usuario admin **hechos**. Falta sembrar **rutas** asignadas al
   rol. Sin rutas sembradas, todo endpoint con `[RequiresPermission]` da 403 a todos.
3. **`RefreshToken`** — EN CURSO. Hecho: entidad + `CreateRefreshTokenAsync` (genera/hashea/
   guarda). **Falta**: `ValidateAsync`/rotación + `RevokeAsync`, engancharlo al login (emitir
   ambos tokens) y los endpoints `Refresh` / `Logout`.
   Access 15 min web / 30 min móvil; refresh 7 días / 30 días.
4. **Resto del `AuthController`**: `ChangePassword` **hecho**. Falta `Me`, y recuperación de
   contraseña con `GeneratePasswordResetTokenAsync` / `ResetPasswordAsync`.
   ⚠️ El Laravel original usaba `Crypt::encrypt($user->id)` como token de reset: no caduca
   y es reutilizable. **No portar ese patrón.**
5. **`GetPermissions`**: agrupado por `module` con `menu_module` y `actions`, lo consume el
   front para pintar el menú.
6. **Auditoría**: `ICurrentUserService` + override de `SaveChangesAsync` en `CoreContext`.
   Es el equivalente del `boot()` de Laravel. Registrar también logins fallidos (el Laravel
   original solo registraba los exitosos).
7. **Auto-descubrimiento de endpoints**: recorrer `EndpointDataSource` al arrancar, buscar
   `RequiresPermissionAttribute` en la metadata y hacer upsert en `AppRoutes`.
8. **Caché en `PermissionService`** (`IMemoryCache`): se ejecuta en cada petición.
9. **`CoreController<T>` genérico** con el `ProcessRequest` del Laravel original
   (`relations`, `orderBy`, `attr`, `pagination`, `select`).
   ⚠️ **Lista blanca obligatoria** de campos y relaciones. El original deja que el cliente
   elija columnas (`select=["password"]`) y filtre por cualquier campo. No portar el agujero.
10. DTOs de entrada/salida, middleware de excepciones (formato `{message, errors}`), CORS,
    configuración de serialización JSON (ciclos por `Include`, nombres snake_case).
11. Exports PDF/Excel: QuestPDF / ClosedXML (Maatwebsite no existe en .NET).
12. Dominio: `Contact`, `Event`, `Theater`, `Invitation`, `Application`...

---

## Trampas ya encontradas (no repetir)

- `ApplicationUser` **no tiene `.Roles`** → usar `_context.UserRoles` y hacer el join a mano.
- `base.OnModelCreating(builder)` es **obligatorio y va primero** en un `IdentityDbContext`.
- `app.UseAuthentication()` **antes** de `app.UseAuthorization()`. Sin el primero, todo da 401.
- El tipo de rol debe coincidir en los tres sitios: herencia del context, `AddRoles<>`,
  y cualquier `RoleManager<>`. Si no, falla en runtime, no al compilar.
- Usar `SignInManager.CheckPasswordSignInAsync` (no `UserManager.CheckPasswordAsync`), que
  es la única que aplica el lockout.
- `ToListAsync()` es el punto de no retorno `IQueryable` → memoria. Todo filtro va antes.
- `Jwt:Key` ≥ 32 caracteres y en user-secrets, nunca en `appsettings.json`.
- No declarar `DbSet<ApplicationUser> Users` ni `Roles` en el context: `IdentityDbContext`
  ya los trae y declararlos los oculta (CS0108).
- En el login, comprobar la contraseña **antes** del estado de la cuenta, y devolver el
  mismo mensaje genérico para "email no existe" y "contraseña mala".
- **Registrar TODO servicio en el contenedor** (`AddScoped<IX, X>()`). Olvidarlo compila
  pero peta en runtime en la primera petición ("Unable to resolve service"). Pasó con
  `IAuthService`.
- **`SetDefaultPolicy` ≠ `SetFallbackPolicy`.** Default solo aplica a endpoints con
  `[Authorize]`; Fallback aplica a los que no declaran nada. Para fail-closed real → Fallback.
- **`GetConnectionString("X")` devuelve `null`** si la clave no existe (no lanza). El nombre
  en `Program.cs` debe coincidir con `appsettings.json` (`SqlServer`, no `DefaultConnection`).
- **Un `dotnet run` vivo bloquea el build** (agarra el `.dll`/`.exe`). Con `dotnet run` el
  proceso es `dotnet`, no `Core Web.exe` → mátalo por línea de comando antes de recompilar.
- **Los servicios no devuelven `IActionResult` ni reciben `ClaimsPrincipal`/`HttpContext`.**
  Reciben primitivos (`long userId`, `string ip`) y devuelven dominio. La web se queda en el
  controller.
