# Plan de desarrollo — App Admin

Admin no es una aplicación de negocio como Reclutamiento o Remuneración: es donde vive la **administración del portal** — datos maestros compartidos entre apps, cuentas de usuario, y el modelo de permisos. Solo el perfil Administrador tiene acceso completo; Supervisor tiene acceso parcial (ver tabla de permisos abajo).

## Módulos

1. Clientes *(incluye Sucursales, ver Fase 3)*
2. Usuarios
3. Perfiles
4. Accesos por perfil
5. Costos IA *(ver Fase 4)* — monitoreo del gasto en los proveedores de IA que usa Reclutamiento (Azure OpenAI, Piamentor.cl, WHO.cl); Admin es quien ve y gestiona esta interacción, no Reclutamiento.

> "Perfil" = una fila de `AspNetRoles` (el mismo mecanismo de roles de ASP.NET Core Identity) — no es una tabla propia aparte. Los perfiles de fábrica son `SuperAdmin`, `Admin`, `Supervisor Operaciones`, `Supervisor Administrativo`, `Reclutador` (ver detalle de alcance de cada uno en [BD_Dashboard.md](BD_Dashboard.md#identity--acceso)); este módulo permite crear perfiles nuevos además de esos cinco.

## Orden sugerido de las fases

```
Fase 0 (base) → Fase 1 (Perfiles y Accesos) → Fase 2 (Usuarios) → Fase 3 (Clientes) → Fase 4 (Costos IA)
```

Perfiles y Accesos van primero porque son el mecanismo del que dependen los otros módulos (y las otras apps) para decidir qué puede ver cada usuario — conviene tenerlo resuelto antes de construir pantallas que ya asumen ese control de acceso.

---

## Fase 0 — Base común

**Objetivo:** modelo de datos y mecanismo de permisos.

**Incluye:**
- Extender el modelo de Identity ya existente (`ApplicationUser`, roles de ASP.NET Core Identity) para soportar el concepto de **Perfil** tal como se usa en el resto del sistema (Administrador, Supervisor, Ejecutivo son los tres iniciales, pero el modelo debe permitir agregar perfiles nuevos).
- Modelo de datos adicional:
  - `Cliente`, `ClienteContacto` (dato maestro compartido — ver Fase 3, pero la tabla se define acá porque otras apps la referencian desde el día uno)
  - `PerfilAplicacion` (qué aplicaciones puede abrir cada perfil — acceso a nivel de app)
  - `PerfilModulo` (qué módulos, dentro de cada aplicación, puede ver cada perfil — acceso a nivel de módulo)
- El resto del sistema (Reclutamiento, Remuneración) debe consultar estas tablas para decidir qué mostrar — no debe haber reglas de permiso hardcodeadas en otras apps.

**Listo cuando:** existen las tablas, y hay una forma de consultar "¿el perfil X tiene acceso a la app Y / al módulo Z?" desde cualquier parte del sistema.

---

## Fase 1 — Módulo Perfiles y Accesos por perfil

**Objetivo:** definir los perfiles y qué aplicaciones/módulos habilita cada uno.

**Incluye:**
- **Perfiles**: listado de perfiles existentes, con su nombre, descripción, cantidad de usuarios que lo tienen asignado, y qué aplicaciones habilita. Crear nuevos perfiles (más allá de los tres iniciales) si el negocio lo requiere.
- **Accesos por perfil**: una matriz (perfil × aplicación × módulo) donde se marca o desmarca el acceso. Reglas:
  - Si se desmarca una aplicación completa para un perfil, ese perfil deja de verla en el portal (header y sidebar).
  - Los módulos de una aplicación solo se pueden marcar si la aplicación está habilitada para ese perfil.
  - Los cambios se aplican de inmediato — un usuario con sesión activa debería ver reflejado el cambio de permisos en su próxima navegación, no requiere logout/login (a validar si es viable con Blazor Server o si conviene forzar refresco del estado del circuito).

**Listo cuando:** un cambio en la matriz de accesos se refleja correctamente en lo que ve un usuario de ese perfil, tanto a nivel de aplicación como de módulo.

---

## Fase 2 — Módulo Usuarios

**Objetivo:** gestión de las cuentas del portal — crear, modificar y bloquear usuarios.

**Incluye:**
- Listado general de usuarios (búsqueda y orden por columna): nombre, correo, celular, perfil asignado, estado (Activo/Bloqueado).
- Crear usuario: nombre, correo corporativo, número de celular y perfil asignado — los tres primeros son obligatorios (correo y celular quedan como campos requeridos en la ficha del usuario, no opcionales, porque de ellos depende la recuperación de contraseña y la verificación en dos pasos). El alta se integra con el flujo de Identity ya existente (creación de cuenta + envío de invitación o contraseña inicial).
- Editar usuario: cambiar perfil asignado, nombre, correo, celular.
- Bloquear / desbloquear usuario: acción explícita en el listado y en la ficha — un usuario bloqueado no puede iniciar sesión aunque conozca su contraseña. Se apoya en el mecanismo de lockout de Identity (`LockoutEnabled`/`LockoutEnd`), expuesto como el campo legible `Estado`.
- Este módulo depende de Fase 1 porque al crear/editar un usuario hay que elegir su perfil de una lista que ya debe existir.

**Seguridad de la cuenta (reutiliza Identity, no requiere pantallas nuevas de "configuración"):**
- **Password encriptado:** el password nunca se guarda en texto plano — Identity lo guarda hasheado (`PasswordHash`). No hay nada que construir acá, es el comportamiento de fábrica.
- **Recuperación de contraseña:** flujo estándar "olvidé mi contraseña" → correo con link/token de un solo uso → nueva contraseña. Ya viene con Identity.
- **Verificación en dos pasos periódica:** además del login normal, el sistema exige verificación por correo (código o link) si pasó más de un intervalo de política (ej. 30 días) desde la última vez que el usuario la aprobó — no es en cada login, es una revalidación periódica de la sesión/identidad. Se registra en `ApplicationUser.UltimaVerificacion2FA`; si esa fecha es null o más antigua que el intervalo, el sistema pide la verificación antes de dejarlo continuar.

**Listo cuando:** se puede crear, editar y bloquear/desbloquear un usuario, asignarle un perfil, y ese usuario puede iniciar sesión (pasando por 2FA cuando corresponda) viendo solo lo que su perfil permite.

---

## Fase 3 — Módulo Clientes

**Objetivo:** gestión centralizada de clientes, compartida entre Reclutamiento y Remuneración — crear, modificar y bloquear clientes, y administrar sus sucursales.

**Incluye:**
- Listado general de clientes (búsqueda y orden por columna): razón social, contacto, cantidad de procesos/contratos activos, estado.
- Crear / editar cliente: datos generales de encabezado (razón social, RUT) y datos de contacto (nombre, cargo, correo, teléfono).
- Bloquear cliente (baja lógica vía `Estado`, no borrado físico) — un cliente bloqueado deja de estar disponible para nuevas Solicitudes/Contratos, sin afectar los ya existentes.
- Ficha de cliente con tres secciones:
  - **Sucursales** (propio de Admin — ver componente aparte abajo).
  - **Procesos de selección** (de la app Reclutamiento — pasados y en curso).
  - **Contratos de servicio** (de la app Remuneración — activos y terminados, con su dotación).
- Este módulo reemplaza lo que en versiones anteriores del diseño estaba pensado como un módulo "Clientes" dentro de cada app: ahora es un dato maestro único.

**Listo cuando:** se puede crear/editar/bloquear un cliente, y su ficha muestra correctamente sus sucursales, procesos de selección y contratos de servicio asociados, cada uno con enlace directo a su detalle en la app correspondiente.

### Componente: Sucursales de cliente

**Objetivo:** llevar el registro de los lugares físicos donde un cliente efectivamente recibe el servicio (casa matriz, plantas, faenas, etc.) — hoy esa dirección solo existía como texto libre suelto en cada Solicitud de Reclutamiento.

**Incluye:**
- Listado de sucursales dentro de la ficha del cliente: nombre, dirección, contacto responsable, estado.
- Crear / editar sucursal: nombre (ej. "Planta Renca"), dirección, y el contacto responsable en esa sucursal (nombre, cargo, correo, teléfono) — quien recibe al personal externo en el día a día.
- Activar / desactivar sucursal (baja lógica).

**Vínculo con Reclutamiento:** al crear una Solicitud, el Supervisor Operaciones puede elegir una sucursal existente del cliente en vez de escribir la dirección de nuevo; si el cliente no tiene sucursales cargadas (o el servicio es en un lugar puntual distinto), sigue disponible el campo de dirección libre como antes.

**Listo cuando:** se puede crear/editar/desactivar sucursales de un cliente, y esa lista aparece como opción al crear una Solicitud en Reclutamiento.

---

## Fase 4 — Módulo Costos IA

**Objetivo:** que Admin (no Reclutamiento) sea quien vea y controle el gasto en los proveedores externos de IA que usa el sistema — hoy Reclutamiento genera esos costos (extracción de CV, evaluación de candidatos, Test Piamentor, Certificado WHO) pero no debe verlos ni gestionarlos.

**Incluye:**
- Listado único que junta las 3 fuentes de gasto — `InteraccionIA` (Azure OpenAI), `TestPiamentor` (Piamentor.cl) y `CertificadoWHO` (WHO.cl) — sin ser una tabla propia, es una vista que combina las tres.
- Filtros: Cliente, Solicitud, Reclutador, Postulante, Proveedor externo, Componente (Atracción / Preselección — cuál de los dos generó el gasto).
- Cada fila muestra un costo total (`CostoUsd`), sin desglose de tokens ni valor unitario.
- Este módulo reemplaza al panel de costos que originalmente vivía embebido en la ficha de Atracción de Reclutamiento (visible para cualquier Reclutador) — se movió a Admin porque el costo de proveedores externos es una decisión de gestión, no algo que el Reclutador necesite ver en su día a día.

**Listo cuando:** Admin puede ver y filtrar el gasto acumulado en IA de todo el sistema desde un solo lugar, y esa información ya no aparece en ninguna pantalla de Reclutamiento.

Ver detalle técnico en [BD_Dashboard.md](BD_Dashboard.md#panel-de-costos-ia-admin--no-es-una-tabla-nueva).

---

## Modelo de permisos de Admin (perfiles de fábrica)

| Módulo | SuperAdmin | Admin | Supervisor Administrativo | Supervisor Operaciones | Reclutador |
|---|---|---|---|---|---|
| Clientes (+ Sucursales) | ✅ | ✅ | ✅ | — | — |
| Usuarios | ✅ | ✅ | — | — | — |
| Perfiles | ✅ | ✅ | — | — | — |
| Accesos por perfil | ✅ | ✅ | — | — | — |
| Costos IA | ✅ | ✅ | — | — | — |

`Supervisor Administrativo` tiene acceso a Admin únicamente para gestionar Clientes/Sucursales (dato que necesita para su trabajo de backoffice); el resto de Admin queda reservado a `SuperAdmin`/`Admin`. `Supervisor Operaciones` y `Reclutador` no entran a Admin — esta tabla es la configuración de fábrica; al ser perfiles editables, Admin puede ajustarla vía Accesos por perfil.

## Resumen de dependencias

| Módulo | Depende de |
|---|---|
| Perfiles y Accesos | Fase 0 |
| Usuarios | Fase 0, Perfiles |
| Clientes | Fase 0 |
| Costos IA | Fase 0, Perfiles — y de que Reclutamiento ya esté generando registros en `InteraccionIA`/`TestPiamentor`/`CertificadoWHO` |

Reclutamiento (procesos de selección) y Remuneración (contratos de servicio) dependen de que Clientes exista acá — ver [plan-reclutamiento.md](plan-reclutamiento.md) y [remuneraciones.md](remuneraciones.md) — pero no lo gestionan directamente, solo lo referencian.

## Referencia

Look and feel, navegación y la matriz de accesos ya maquetada están en la maqueta — ver [sosgroup.md](sosgroup.md).
