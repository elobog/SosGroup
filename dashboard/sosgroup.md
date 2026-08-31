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
