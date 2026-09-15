# SOS Group — Portal Interno

## Estado (2026-08-31)

Este documento describía la primera maqueta (v1, un solo perfil por vez, roles Administrador/Supervisor/Ejecutivo). Esa maqueta quedó **reemplazada** por la v2 y, en paralelo, ya arrancó la construcción del proyecto real (`dashboard/`, Blazor + Identity + EF Core sobre `sqldb-sosgroup-app`). Este archivo se actualiza para reflejar eso.

## Maqueta vigente (Artifact)

https://claude.ai/code/artifact/4d1039cb-0f98-4943-8c90-856818c7e36e

Maqueta clickeable (HTML/JS, sin backend) — sigue siendo la guía de look and feel y de flujo funcional para construir la app real. Cubre Módulo 1 (Solicitud, Perfil de Cargo, Apertura) y Módulo 2 (Atracción, Preselección, Evaluación) de Reclutamiento; ver [Reclutamiento2.md](Reclutamiento2.md) para el detalle de cada componente y [BD_Dashboard.md](BD_Dashboard.md) para el modelo de datos.

## Objetivo

Portal único de acceso a las aplicaciones internas de SOS Group — **Admin**, **Reclutamiento** y **Remuneración** — conviviendo en una sola base de datos (ver estructura de schemas en [BD_Dashboard.md](BD_Dashboard.md)). Cada aplicación se muestra según el perfil del usuario que inició sesión.

## Look and feel

- Colores corporativos: navy `#1c244b`, rojo `#9c2b34`, con variantes claras/oscuras.
- Tipografía: Barlow (texto) / Barlow Condensed (títulos) — cargadas vía Google Fonts en la app real.
- Logo SOS Group en tamaño grande en la pantalla de acceso.

## Pantalla de acceso (Login)

- Ingreso con correo corporativo y contraseña.
- Flujo de recuperación de contraseña (3 pasos: solicitar enlace → confirmación de envío → volver a inicio de sesión).
- **✅ Implementada en la app real** (`Components/Account/Pages/Login.razor`), con el layout de dos paneles de la maqueta (panel navy con logo/tagline a la izquierda, tarjeta blanca de acceso a la derecha) — usa `BlankLayout` para no heredar el shell de navegación.

## Dashboard

- **Header**: datos del usuario (nombre, perfil, avatar) a la derecha; a la izquierda, las aplicaciones disponibles para el perfil se muestran como **links en paralelo** (no dropdown). La app activa se marca con subrayado rojo.
- **Sidebar izquierdo**: módulos de la aplicación activa. Los módulos sin acceso para el perfil actual aparecen en una sección "Restringido para tu perfil", bloqueados.
- **Listas**: todas las tablas del portal son ordenables por columna (asc/desc, clic en el encabezado) y tienen barra de búsqueda con filtro en vivo.
- **🔴 Pendiente en la app real:** hoy, después de iniciar sesión, se ve el `MainLayout`/`NavMenu` genéricos del scaffold de Blazor (sidebar Bootstrap por defecto) — todavía no se construyó el shell real (topbar navy + sidebar navy + selector de apps) descrito arriba.

## Aplicaciones incluidas

1. **Reclutamiento** — ver [Reclutamiento2.md](Reclutamiento2.md) para el detalle real (Solicitud, Perfil de Cargo, Apertura, Atracción, Preselección, Evaluación).
2. **Remuneración** — ver [remuneraciones.md](remuneraciones.md); *nota: ese documento todavía usa el modelo de roles v1 (Administrador/Supervisor/Ejecutivo) — pendiente de revisar con el modelo de perfiles dinámico vigente (ver abajo), no se tocó en esta pasada.*
3. **Admin** — ver [admin.md](admin.md): Clientes (+ Sucursales), Usuarios, Perfiles, Accesos por perfil, Costos IA.

## Modelo de permisos (perfiles) — vigente

Los **perfiles** ya no son un enum fijo de 3 valores — son filas de `AspNetRoles` (Identity), administrables desde **Admin → Perfiles**. Qué aplicaciones y módulos habilita cada uno se define en `Admin.PerfilAplicacion`/`Admin.PerfilModulo` (matriz perfil × aplicación × módulo, administrada desde **Admin → Accesos por perfil**). Detalle completo en [BD_Dashboard.md](BD_Dashboard.md#identity--acceso).

Perfiles de fábrica (seed, no una lista cerrada):

| Perfil | Acceso de fábrica |
|---|---|
| SuperAdmin | Todas las aplicaciones |
| Admin | Todas las aplicaciones |
| Supervisor Administrativo | Admin (Clientes/Sucursales) + Reclutamiento + Remuneración |
| Supervisor Operaciones | Solo Reclutamiento (acotado a sus clientes asignados) |
| Reclutador | Solo Reclutamiento (acotado a sus clientes asignados) |

## Estado de implementación del proyecto real

- **Infraestructura Azure:** `rg-sosgroup-app` / `sql-sosgroup-app` / `sqldb-sosgroup-app` creados. Estructura de datos completa instalada vía migración de EF Core (no DDL a mano) — ver `dashboard/BD_Dashboard.md`.
- **Repo Git:** `github.com/elobog/SosGroup`, sincronizado en `...\GIT\V2026\SosGroup`.
- **Proyecto .NET:** Blazor Web App (Server) + ASP.NET Core Identity + EF Core sobre SQL Server, con roles habilitados (`AddRoles<IdentityRole>`).
- **Usuarios creados:** 2 cuentas `SuperAdmin` (acceso de prueba para el equipo).
- **✅ Login:** funcional, con el look and feel de la maqueta.
- **🔴 Pendiente:** shell de dashboard (topbar + sidebar + selector de apps), y todas las pantallas de contenido (Solicitud, Clientes, Usuarios, etc.).

## Pendiente / próximos pasos

- Construir el shell de dashboard (`MainLayout`/`NavMenu` reales) con el look and feel de la maqueta.
- Construir la primera pantalla de contenido real (Solicitud, Módulo 1 de Reclutamiento).
- Revisar `remuneraciones.md` con el modelo de perfiles vigente antes de construir esa app.
- **🔴 Pendiente (ver bitácora 2026-09-15):** de las 17 páginas de `Account/Pages`, solo `Login.razor` tiene `@layout BlankLayout` + CSS propio. Las otras 16 (Register, ResetPassword, ConfirmEmail, Lockout, etc.) caen en el `MainLayout` genérico con el sidebar de plantilla de Blazor (Home/Counter/Weather) — mismo bug que se corrigió puntualmente en `ForgotPassword.razor`, queda repetirlo en el resto.
- **🔴 Pendiente (ver bitácora 2026-09-15):** barra "An unhandled error has occurred" visible en pantallas de `Account` (se ve incluso en Login) — parece un error real sin manejar en el circuito de Blazor Server, no investigado todavía.

## Bitácora de sesiones

### Sesión 2026-09-15 (Ignacio, con Claude): acceso al repo/Azure, auditoría de recuperación de contraseña, y arreglo de `ForgotPassword`

**1. Acceso inicial.** El repo `elobog/SosGroup` no estaba clonado en esta máquina — se instaló y autenticó GitHub CLI (`gh`), se clonó en `OneDrive\SOS GROUP`. Confirmado: Ignacio es colaborador con permiso `push` (no admin) sobre el repo, que es público. El sitio de QA (`https://qa-sosgroup.azurewebsites.net/`) vive en la suscripción **`AITBP_APP`** (no en la suscripción `sosgroup`, que solo tiene el SQL trial) — resource group `rg-aitbp-app`, junto con las apps de Insuseg. El acceso Azure a esa suscripción se resolvió re-logueando `az` explícitamente como `ignacio.aitbp@melirrepu.com` (el login había quedado con otra cuenta, `info@aitbp.com`, sin ese acceso).

**2. Auditoría del sistema de recuperación de contraseña — conclusión: confiable.** Revisado `ForgotPassword.razor` / `ResetPassword.razor` / `CorreoSistemaService.cs` (envío real vía Microsoft Graph) / config de Identity en `Program.cs`. Hallazgo más serio candidato: `AllowedHosts: "*"` sin validar el header `Host`, que en teoría permite "password reset poisoning" (armar el link del correo con un dominio del atacante). **Probado en vivo contra `qa-sosgroup`:** Azure App Service enruta a nivel de plataforma por el header `Host` *antes* de que la request llegue al código de la app — un `Host` falso o de otra app real (probado con `app-insuseg.azurewebsites.net`) hace que Azure entregue la request a otra parte, nunca al código de SosGroup. **Conclusión: no explotable en este hosting**, aunque `AllowedHosts` debería corregirse igual como defensa en profundidad si algún día se agrega un CDN/WAF delante. Hallazgos menores (no urgentes): sin rate limiting en `ForgotPassword` (permite bombardeo de correos + canal de temporización para enumerar cuentas), token de reset con el default de 24h de Identity, `user.Nombre` sin encodear en el HTML del correo.

**3. Bug visual encontrado (capturas con Playwright, ver más abajo por qué hizo falta): sidebar de plantilla de Blazor en `/Account/ForgotPassword`.** Causa raíz: de las 17 páginas de `Account/Pages`, únicamente `Login.razor` declara `@layout BlankLayout`; el resto cae en el `MainLayout` por defecto del router (`Routes.razor`), que trae el `NavMenu` de ejemplo del scaffold de Blazor (Home/Counter/Weather/Auth Required/Register/Login) — nunca reemplazado. Le pasa a las 16 páginas restantes, no solo a esta (ver pendiente arriba).

**4. Fix aplicado a `ForgotPassword.razor` (dos commits):**
   - `9756f9e`: agrega `@layout BlankLayout` (saca el sidebar) y traduce el texto (estaba en inglés, texto por defecto del scaffold de Identity, sin tocar).
   - `17d0456`: reemplaza el HTML crudo de Bootstrap (sin ningún estilo propio, por eso quedaba todo pegado arriba a la izquierda) por una tarjeta centrada reutilizando el lenguaje visual de `Login.razor.css` (mismas variables de `app.css`: `--paper`, `--surface`, `--red`, etc.) — nuevo archivo `ForgotPassword.razor.css`. Se dejó **scoped a esta página únicamente** (no se extrajo a un CSS global reutilizable) por pedido explícito — al tocar las otras 15 páginas pendientes habrá que decidir si conviene unificar esas clases en `app.css` para no repetir el CSS 16 veces.

**5. Incidente real durante el primer deploy — sitio completo roto, ya resuelto.** El primer intento de desplegar el fix (`az webapp deploy` con un zip armado por `Compress-Archive` de PowerShell) rompió **todos los assets estáticos del sitio entero** (CSS/JS en 0 bytes, sitio sin estilos). Causa: `Compress-Archive` marca las entradas del zip como "Windows/FAT" en vez de "Unix", y el montaje de Azure App Service en Linux (`WEBSITE_RUN_FROM_PACKAGE=1`) no reconstruye bien las carpetas anidadas para esas entradas. **Este bug ya estaba documentado en `Insuseg.md`** (sesión 2026-08-19/20, mismo síntoma exacto) — se aplicó el mismo fix ahí descrito: armar el zip con Python (`zipfile`, `ZipInfo.create_system = 3`) en vez de `Compress-Archive`. Redesplegado con el zip correcto, verificado con captura de pantalla — sitio restaurado, cambio de `ForgotPassword` visible.

> **⚠️ Advertencia para cualquier deploy futuro a `qa-sosgroup` o a cualquier app de este resource group (`rg-aitbp-app`, Linux + `WEBSITE_RUN_FROM_PACKAGE=1`): nunca usar `Compress-Archive` de PowerShell ni `System.IO.Compression.ZipFile.CreateFromDirectory` para armar el zip de deploy — ambos rompen el sitio completo.** Usar Python (`zipfile`, marcando `create_system = 3` en cada entrada) — ver script usado esta sesión o el de `Insuseg.md`.

**Estado al cierre:** `ForgotPassword.razor` con layout y estilo correctos, desplegado y verificado visualmente en QA. Pendiente real para la próxima sesión: las otras 15 páginas de `Account` con el mismo bug de sidebar, y la barra de error sin manejar (ver "Pendiente" arriba).
