# SOS Group — Portal Interno · Maqueta

## Enlace de la maqueta

https://claude.ai/code/artifact/93e7bbee-3b22-434e-b97b-c84d35186fac

Maqueta clickeable (HTML/JS, sin backend). Incluye una barra gris superior (solo para revisión) que permite simular la pantalla (Login / Dashboard) y el perfil de usuario (Administrador / Supervisor / Ejecutivo).

Versión actualizada (12-08-2026) para presentación al cliente: agrega los flujos de creación que faltaban — "+ Nuevo postulante" (Reclutamiento), y en Admin "+ Nuevo cliente" (con dirección y baja lógica), "+ Nuevo usuario" (con activar/desactivar) y "+ Nuevo perfil" — alineados con los planes de desarrollo de cada módulo.

## Objetivo

Portal único de acceso a las aplicaciones internas de SOS Group. Cada aplicación es independiente y se muestra según el perfil del usuario que inició sesión.

## Look and feel

- Colores corporativos: navy `#1c244b`, rojo `#9c2b34`, con variantes claras/oscuras.
- Tipografía: Barlow (texto) / Barlow Condensed (títulos).
- Logo SOS Group en tamaño grande en la pantalla de acceso.

## Pantalla de acceso (Login)

- Ingreso con correo corporativo y contraseña.
- Flujo de recuperación de contraseña (3 pasos: solicitar enlace → confirmación de envío → volver a inicio de sesión).

## Dashboard

- **Header**: datos del usuario (nombre, perfil, avatar) a la derecha; a la izquierda, las aplicaciones disponibles para el perfil se muestran como **links en paralelo** (no dropdown). La app activa se marca con subrayado rojo.
- **Sidebar izquierdo**: módulos de la aplicación activa. Los módulos sin acceso para el perfil actual aparecen en una sección "Restringido para tu perfil", bloqueados.
- **Listas**: todas las tablas del portal son ordenables por columna (asc/desc, clic en el encabezado) y tienen barra de búsqueda con filtro en vivo.

## Aplicaciones incluidas

1. **Reclutamiento** — Resumen, Postulantes, Selección.
2. **Remuneración** — Panel de control, Base Talana, Contratos, Cálculo, Auditoría.
3. **Admin** — Clientes (también accesible a Supervisor), Usuarios, Perfiles, Accesos por perfil (estos tres, solo Administrador).

## Modelo de permisos (perfiles)

Los **perfiles** son la única fuente de verdad para el acceso:

- **Acceso a nivel de aplicación**: qué aplicaciones puede abrir cada perfil.
- **Acceso a nivel de módulo**: qué módulos, dentro de cada aplicación, puede ver cada perfil.

Ambos niveles se administran desde **Admin → Accesos por perfil**, una matriz de checkboxes (perfil × aplicación × módulo). Los cambios se reflejan de inmediato en el header y el sidebar del perfil afectado.

Perfiles definidos en la maqueta:

| Perfil | Acceso |
|---|---|
| Administrador | Todas las aplicaciones, incluida Admin |
| Supervisor | Reclutamiento y Remuneración (sin módulos de cierre exclusivos de Administrador) |
| Ejecutivo | Solo Reclutamiento, con módulos operativos acotados |

## Pendiente / próximos pasos

- Validar look and feel y flujos con el equipo.
- Definir alcance real de cada módulo (campos, reglas de negocio).
- Pasar la maqueta a implementación en el proyecto Blazor (`dashboard`), incluyendo backend de autenticación, usuarios, perfiles y permisos.
