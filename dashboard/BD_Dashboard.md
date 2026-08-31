# BD_Dashboard — Diccionario de datos

> Fuente única de verdad para la estructura física de la base de datos (SQL Server / Azure SQL `sqldb-sosgroup`) del portal SOS Group: Acceso, Admin, Reclutamiento, Remuneraciones. Los documentos de plan (`admin.md`, `Reclutamiento2.md`, `remuneraciones.md`) describen objetivo/roles/flujo de cada componente y enlazan acá para el detalle de campos, tipos e índices — no duplican la lista de campos.
>
> Para convenciones de interfaz (cómo se ven los listados, formularios, etc.) ver [Estandares-UI.md](Estandares-UI.md).

## Convenciones

- **Nombres de tabla:** PascalCase, singular (convención EF Core).
- **PK de tablas de negocio:** `int IDENTITY(1,1)`, salvo que se indique lo contrario.
- **PK de usuario:** `nvarchar(450)` — es el Id de ASP.NET Core Identity (`AspNetUsers.Id`), ya en producción.
- **Tipos de dinero:** `decimal(18,2)`.
- **Fechas:** `datetime2`.
- **Estado (texto corto tipo "Activo/Inactivo"):** `nvarchar(20)`.

**Leyenda de estado por tabla:**
- 🟢 **En producción** — ya existe en la base real.
- 🟡 **Propuesto** — campos sugeridos por mí en base a lo definido hasta ahora; pendiente de validar con la información que traiga el cliente.
- 🔴 **Reemplazada / obsoleta** — de una versión anterior del plan, ya no es la estructura vigente.

---

## Identity / Acceso

### AspNetUsers, AspNetRoles, AspNetUserRoles *(y tablas estándar de Identity)* — 🟢 En producción
No se detallan acá (son el esquema estándar de ASP.NET Core Identity, ya implementado).

**Modelo de acceso — dinámico, no hardcodeado:** los "roles" son filas de `AspNetRoles` (tabla estándar de Identity) administrables desde Admin › Perfiles; qué aplicaciones y módulos habilita cada uno se define en `PerfilAplicacion`/`PerfilModulo` (ver Admin más abajo), no en código. Los 5 roles de abajo son los que vienen creados de fábrica (seed), no una lista cerrada — Admin puede crear perfiles nuevos.

**Roles de fábrica (seed)** — reemplazan al set anterior `Administrador`/`Supervisor`/`Ejecutivo`, ya obsoleto:

| Rol | Quién | Alcance de fábrica |
|---|---|---|
| `SuperAdmin` | Administrador de AITBP (proveedor de la plataforma) | Acceso total, incluidos los módulos exclusivos de proveedor (aún no definidos en el plan — pendiente). |
| `Admin` | Administrador de SOSGroup | Acceso a Reclutamiento, Remuneración y Admin — todo salvo lo exclusivo de `SuperAdmin`. |
| `Supervisor Operaciones` | Responsable en terreno del personal externo | Solo Reclutamiento. Es el rol "Supervisor" que ya se había definido para el componente Solicitud (crea solicitudes, se le asignan clientes vía `ClienteSupervisor`). |
| `Supervisor Administrativo` | Supervisión de backoffice | Remuneración + Admin (Clientes) **y Reclutamiento** (visibilidad total, sin acotar por asignación) — necesita entrar a Solicitud para aprobar condiciones de Perfil de Cargo (ver más abajo). |
| `Reclutador` | Ejecuta las solicitudes de personal | Solo Reclutamiento, acotado a los clientes que tiene asignados vía `ClienteReclutador`. |

**Regla de visibilidad de datos:** `Supervisor Operaciones` y `Reclutador` solo ven los `Cliente`/`Solicitud` donde están asignados (`ClienteSupervisor`/`ClienteReclutador`/`Solicitud.SupervisorId`) — esto es acotamiento por asignación, independiente de la matriz app/módulo de `PerfilAplicacion`/`PerfilModulo`. `SuperAdmin`, `Admin` y `Supervisor Administrativo` ven todo dentro de las apps a las que su perfil da acceso — `Supervisor Administrativo` incluido, porque administra el directorio de Clientes como tarea de backoffice, no por asignación operativa.

> **Pendiente:** los módulos "exclusivos de SuperAdmin" mencionados arriba todavía no están definidos — por ahora `SuperAdmin` y `Admin` tienen el mismo acceso a nivel de apps en la maqueta.

### PerfilAplicacion — 🟡 Propuesto
*(origen: Admin · Perfiles y Accesos)* — Qué aplicaciones (Reclutamiento, Remuneración, Admin) habilita cada rol. Se referencia `AspNetRoles.Id` directamente en vez de crear una tabla `Perfil` paralela — "perfil" y "rol" son el mismo concepto, para no duplicar lo que Identity ya resuelve.

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `RoleId` | nvarchar(450), FK → AspNetRoles.Id | No | PK compuesta |
| `App` | nvarchar(30) | No | PK compuesta — "Reclutamiento" / "Remuneracion" / "Admin" |

### PerfilModulo — 🟡 Propuesto
*(origen: Admin · Perfiles y Accesos)* — Qué módulos, dentro de una app ya habilitada, ve cada rol. Un módulo solo puede marcarse si su `App` ya está en `PerfilAplicacion` para ese `RoleId` — se valida en la capa de aplicación, no con una FK compuesta hacia `PerfilAplicacion` (evita duplicar `RoleId`+`App` como columna extra).

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `RoleId` | nvarchar(450), FK → AspNetRoles.Id | No | PK compuesta |
| `App` | nvarchar(30) | No | PK compuesta |
| `Modulo` | nvarchar(50) | No | PK compuesta — ej. "Solicitud", "Preselección", "Clientes" |

**Índices:** `IX_PerfilModulo_RoleId`

---

## Admin — Compartidas

### ApplicationUser — 🟡 Propuesto (extiende AspNetUsers 🟢)
*(origen: Admin · Usuarios)* — Campos propios agregados sobre `IdentityUser` (`AspNetUsers` ya en producción); no es una tabla nueva, son columnas adicionales en la misma tabla.

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Nombre` | nvarchar(200) | No | nombre para mostrar — Identity trae `UserName`/`Email` pero no un nombre de persona |
| `Email` | nvarchar(256) | No | único — ⚠️ heredado de `IdentityUser` — ahí es nullable y no único por defecto; se exige **requerido y único** en este sistema (login, verificación en dos pasos y recuperación de contraseña dependen del correo) vía `RequireUniqueEmail` de Identity |
| `PhoneNumber` | nvarchar(30) | No | ⚠️ heredado de `IdentityUser` — ahí es nullable/opcional por defecto; se exige **requerido** en este sistema (contacto directo del usuario, aunque el 2FA de arriba sea por correo y no por SMS) |
| `Estado` | nvarchar(20) | No | Activo / Bloqueado — default 'Activo'; expone de forma legible el bloqueo que en Identity se resuelve con `LockoutEnabled`+`LockoutEnd` |
| `UltimaVerificacion2FA` | datetime2 | Sí | fecha de la última verificación de dos pasos por correo aprobada; null = nunca verificado. El sistema vuelve a exigirla si pasó más del intervalo de política (ej. cada 30 días) desde esta fecha |

**Regla de negocio:** `Nombre`, `Email` y `PhoneNumber` son obligatorios al crear un usuario desde Admin · Usuarios — no se puede dar de alta una cuenta sin los tres. `Email` y `PhoneNumber` vienen del esquema estándar de `IdentityUser` (no son columnas nuevas), pero acá se documentan explícitos porque Identity los deja nullable por defecto y este sistema los requiere.

**Reutilizado, sin campos nuevos:**
- **Password encriptado:** ya cubierto por `PasswordHash` de Identity (hash, no texto plano) — no requiere modelado adicional.
- **Recuperación de contraseña:** ya cubierto por el flujo estándar de Identity (token en `AspNetUserTokens` + link por correo) — no requiere modelado adicional.
- **Verificación en dos pasos:** usa el mecanismo estándar de Identity (`TwoFactorEnabled` + token por correo); `UltimaVerificacion2FA` de arriba es el único campo propio, para saber cuándo re-exigirla.

### Cliente — 🟡 Propuesto

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `RazonSocial` | nvarchar(200) | No | |
| `RUT` | nvarchar(20) | No | único |
| `ContactoNombre` | nvarchar(200) | Sí | ⚠️ ver nota abajo |
| `ContactoCorreo` | nvarchar(200) | Sí | ⚠️ ver nota abajo |
| `ContactoTelefono` | nvarchar(30) | Sí | ⚠️ ver nota abajo |
| `Estado` | nvarchar(20) | No | Activo / Inactivo, default 'Activo' |
| `FechaCreacion` | datetime2 | No | default getutcdate() |

**Índices:** `IX_Cliente_RUT` (único), `IX_Cliente_Estado`

⚠️ Discrepancia sin resolver (detectada al reconciliar maqueta ↔ plan de Admin): el plan original (`admin.md`) permite varios contactos por cliente vía una tabla aparte `ClienteContacto`, pero la maqueta solo modela un contacto, embebido directo en `Cliente` como tres campos (nombre/correo/teléfono en vez de un único `Contacto` de texto libre — así quedó ajustado en esta pasada para reflejar lo que realmente construye la maqueta). Si se confirma "varios contactos" al pasar a producción, estos tres campos salen de `Cliente` y pasan a `ClienteContacto` (tabla aparte, 1:N). Nota aparte: la maqueta tampoco modela una dirección de Cliente — la única dirección de la razón social; la dirección operativa real ahora vive en `ClienteSucursal` (ver abajo).

### ClienteSucursal — 🟡 Propuesto
*(origen: Admin · Clientes, "Gestión de sucursales de clientes")* — Lugares físicos de un `Cliente` donde efectivamente se presta el servicio. Un cliente puede tener 0 o varias.

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `ClienteId` | int, FK → Cliente.Id | No | |
| `Nombre` | nvarchar(150) | No | ej. "Casa Matriz", "Planta Renca", "Faena Norte" |
| `Direccion` | nvarchar(300) | No | |
| `ContactoResponsableNombre` | nvarchar(200) | Sí | |
| `ContactoResponsableCargo` | nvarchar(100) | Sí | |
| `ContactoResponsableCorreo` | nvarchar(200) | Sí | |
| `ContactoResponsableTelefono` | nvarchar(30) | Sí | |
| `Estado` | nvarchar(20) | No | Activo / Inactivo, default 'Activo' |
| `FechaCreacion` | datetime2 | No | default getutcdate() |

**Índices:** `IX_ClienteSucursal_ClienteId`

**Vínculo con Reclutamiento:** `Solicitud.SucursalId` (nuevo FK, nullable) puede apuntar a una `ClienteSucursal` en vez de repetir la dirección como texto libre en `Solicitud.DireccionServicio`; ese campo se mantiene como override para servicios sin sucursal formal o direcciones puntuales. Ver [Reclutamiento2.md](Reclutamiento2.md#componente-solicitud).

---

## Reclutamiento — v2 (vigente)

### PerfilCargo — 🟡 Propuesto
*(origen: Módulo 1 · Solicitud)* — Encabezado del perfil de cargo. Propio de cada Cliente. Describe competencias, estable entre versiones.

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `ClienteId` | int, FK → Cliente.Id | No | |
| `Nombre` | nvarchar(150) | No | |
| `Descripcion` | nvarchar(max) | Sí | responsabilidades |
| `Requisitos` | nvarchar(max) | Sí | competencias |
| `Estado` | nvarchar(20) | No | Activo / Inactivo |
| `FechaCreacion` | datetime2 | No | default getutcdate() |

**Índices:** `IX_PerfilCargo_ClienteId`

### PerfilCargoVersion — 🟡 Propuesto
*(origen: Módulo 1 · Solicitud)* — Listado de versiones de un `PerfilCargo`. Una versión se genera a partir de otra, actualizando principalmente condiciones de renta (no las competencias del encabezado).

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `PerfilCargoId` | int, FK → PerfilCargo.Id | No | |
| `NumeroVersion` | int | No | |
| `RentaFija` | nvarchar(100) | Sí | texto libre — la negociación suele ser un rango ("$1.100.000 - $1.300.000"), no un monto único; no alcanza con `decimal` |
| `RentaVariable` | nvarchar(200) | Sí | texto libre — comisiones, bonos o "Sin renta variable"; tampoco es un monto único |
| `Beneficios` | nvarchar(max) | Sí | |
| `FechaVigencia` | datetime2 | No | |
| `Estado` | nvarchar(20) | No | **Pendiente** / Vigente / Historica |
| `VersionBaseId` | int, FK → PerfilCargoVersion.Id (auto) | Sí | de qué versión se originó |

**Índices:** `IX_PerfilCargoVersion_PerfilCargoId`, único (`PerfilCargoId`, `NumeroVersion`)

**Regla de aprobación:** toda versión nace en `Pendiente` (tanto la primera versión de un `PerfilCargo` nuevo como cualquier condición agregada después) y no puede usarse en una `Solicitud` hasta pasar a `Vigente`. Solo pasa a `Vigente` cuando la aprueban **ambos** — `Supervisor Operaciones` y `Supervisor Administrativo` — momento en el que la versión `Vigente` anterior (si existía) pasa a `Historica`. Mientras una versión está `Pendiente`, la `Vigente` anterior sigue rigiendo sin interrupción.

### PerfilCargoVersionAprobacion — 🟡 Propuesto
*(origen: Módulo 1 · Solicitud, regla de aprobación agregada después de construir la maqueta)* — Registro de la aprobación de cada rol sobre una versión. En la maqueta se modela como dos campos embebidos en la versión (`aprobaciones.operaciones`, `aprobaciones.administrativo`); acá se propone como tabla aparte para no repetir la pareja de campos usuario/fecha dos veces en `PerfilCargoVersion`.

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `PerfilCargoVersionId` | int, FK → PerfilCargoVersion.Id | No | |
| `Rol` | nvarchar(30) | No | `Supervisor Operaciones` / `Supervisor Administrativo` |
| `AprobadoPorUsuarioId` | nvarchar(450), FK → AspNetUsers.Id | Sí | null mientras no se aprueba |
| `FechaAprobacion` | datetime2 | Sí | |

**Índices:** único (`PerfilCargoVersionId`, `Rol`)

### Solicitud — 🟡 Propuesto
*(origen: Módulo 1 · Solicitud)* — Encabezado. Estado **derivado** de sus `SolicitudDetalle`, no editable directamente.

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `CodigoSolicitud` | nvarchar(30) | No | único |
| `ClienteId` | int, FK → Cliente.Id | No | |
| `PerfilCargoVersionId` | int, FK → PerfilCargoVersion.Id | No | condiciones vigentes al crear la solicitud |
| `FechaInicioServicio` | datetime2 | No | |
| `SucursalId` | int, FK → ClienteSucursal.Id | Sí | *(origen: Admin · Sucursales)* lugar donde se presta el servicio, cuando el cliente tiene sucursales definidas |
| `DireccionServicio` | nvarchar(300) | Sí | override de texto libre — se usa cuando no hay `SucursalId` (cliente sin sucursales formales) o el servicio es en una dirección puntual distinta |
| `SupervisorId` | nvarchar(450), FK → AspNetUsers.Id | No | usuario de rol `Supervisor Operaciones` |
| `Estado` | nvarchar(20) | No | Activa / Cerrada — derivado |
| `FechaCreacion` | datetime2 | No | default getutcdate() |

**Índices:** `IX_Solicitud_ClienteId`, `IX_Solicitud_CodigoSolicitud` (único), `IX_Solicitud_Estado`

### SolicitudDetalle — 🟡 Propuesto
*(origen: Módulo 1 · Solicitud)* — Cada ronda de búsqueda para el mismo perfil dentro de una Solicitud.

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `SolicitudId` | int, FK → Solicitud.Id | No | |
| `ReclutadorId` | nvarchar(450), FK → AspNetUsers.Id | No | |
| `CantidadPersonalSolicitado` | int | No | |
| `FechaSolicitud` | datetime2 | No | default getutcdate() |
| `Estado` | nvarchar(20) | No | Pendiente / EnProceso / Cerrada |
| `Observaciones` | nvarchar(max) | Sí | |

**Índices:** `IX_SolicitudDetalle_SolicitudId`, `IX_SolicitudDetalle_Estado`

### ClienteReclutador — 🟡 Propuesto
*(origen: Módulo 1 · Solicitud)* — Asignación N:N.

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `ClienteId` | int, FK → Cliente.Id | No | PK compuesta |
| `UsuarioId` | nvarchar(450), FK → AspNetUsers.Id | No | PK compuesta |
| `FechaAsignacion` | datetime2 | No | default getutcdate() |

### ClienteSupervisor — 🟡 Propuesto
*(origen: Módulo 1 · Solicitud)* — Misma forma que `ClienteReclutador`, para usuarios de rol `Supervisor Operaciones`.

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `ClienteId` | int, FK → Cliente.Id | No | PK compuesta |
| `UsuarioId` | nvarchar(450), FK → AspNetUsers.Id | No | PK compuesta |
| `FechaAsignacion` | datetime2 | No | default getutcdate() |

### Apertura — 🟡 Propuesto
*(origen: Módulo 1 · Apertura)* — Encabezado, 1:1 con `Solicitud`. Se crea al "Iniciar"; requiere que la Solicitud tenga un `PerfilCargoVersion` Vigente.

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `SolicitudId` | int, FK → Solicitud.Id | No | único — 1 Apertura por Solicitud |
| `FechaInicio` | datetime2 | No | default getutcdate() |
| `IniciadoPorUsuarioId` | nvarchar(450), FK → AspNetUsers.Id | No | |
| `Estado` | nvarchar(20) | No | Iniciada |

**Índices:** único (`SolicitudId`)

### Aviso — 🟡 Propuesto
*(origen: Módulo 1 · Apertura)* — Texto/HTML del aviso de la búsqueda, versionado. Un único modelo por versión, se usa igual en todas las plataformas.

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `AperturaId` | int, FK → Apertura.Id | No | |
| `NumeroVersion` | int | No | |
| `TituloCargo` | nvarchar(150) | No | |
| `Modalidad` | nvarchar(20) | No | Presencial / Híbrido / Remoto |
| `Ubicacion` | nvarchar(300) | Sí | default = `Solicitud.DireccionServicio` |
| `CierrePostulaciones` | datetime2 | Sí | |
| `CuerpoHtml` | nvarchar(max) | No | HTML generado desde la plantilla (incluye los campos de arriba ya redactados) |
| `CreadoPorUsuarioId` | nvarchar(450), FK → AspNetUsers.Id | No | |
| `FechaCreacion` | datetime2 | No | default getutcdate() |

> En la maqueta `CierrePostulaciones` acepta texto libre ("Por definir" si se deja vacío) porque queda embebido en el HTML. Como columna `datetime2` real, debe quedar `NULL` cuando no se define — no guardar el placeholder de texto.

**Índices:** `IX_Aviso_AperturaId`, único (`AperturaId`, `NumeroVersion`)

### Publicacion — 🟡 Propuesto
*(origen: Módulo 1 · Apertura)* — Log de cada publicación de un Aviso en una plataforma.

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `AvisoId` | int, FK → Aviso.Id | No | |
| `Plataforma` | nvarchar(30) | No | Computrabajo / Trabajando.com / LinkedIn / Facebook / Instagram / TikTok |
| `UsuarioId` | nvarchar(450), FK → AspNetUsers.Id | No | quién publicó |
| `FechaHora` | datetime2 | No | default getutcdate() |
| `Metodo` | nvarchar(20) | No | API / Manual |
| `Estado` | nvarchar(30) | No | Publicado / Pendiente de carga manual / Error |
| `CantidadPostulantes` | int | No | default 0 — contador, ver nota abajo |

**Índices:** `IX_Publicacion_AvisoId`

**Nota sobre `CantidadPostulantes`:** deja de depender del conteo que reporte cada plataforma (no todas tienen API). Cada `Publicacion` tiene su propio link público de postulación (`PostulacionCandidato.PublicacionId`); el contador sube automáticamente con cada postulación real recibida por ese link, más los ajustes manuales que registre el Reclutador para postulantes que llegan por fuera del sistema (WhatsApp, correo directo).

### PostulacionCandidato — 🟡 Propuesto
*(origen: Módulo 1 · Apertura → dispara Módulo 2 · Preselección · Atracción)* — Captura mínima de quien postula desde la página pública (sin login), a través del link único de una `Publicacion`. Es el punto de entrada real de candidatos al sistema; **no reemplaza** la ficha completa de `Postulante` (hoja de vida, documentos, extracción por IA) definida en el plan v1, que sigue pendiente como desarrollo aparte y más grande — esto solo registra que alguien postuló y sus datos de contacto.

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `PublicacionId` | int, FK → Publicacion.Id | No | de qué link/plataforma vino |
| `Nombre` | nvarchar(200) | No | |
| `RUT` | nvarchar(20) | No | |
| `Correo` | nvarchar(200) | Sí | |
| `Telefono` | nvarchar(30) | Sí | |
| `FechaHora` | datetime2 | No | default getutcdate() |

**Índices:** `IX_PostulacionCandidato_PublicacionId`

**Página pública:** en la maqueta se simula con un deep-link dentro del mismo artifact (`#postular-{PublicacionId}`) que salta el login. En la implementación real esto sería una ruta pública de verdad (sin pasar por ASP.NET Core Identity), no un hash de una SPA — la maqueta lo simplifica porque es una sola página.

### CredencialPlataforma — 🟡 Propuesto
*(origen: Módulo 1 · Apertura)* — Credencial de publicación por plataforma. `UsuarioId` nulo = credencial de empresa (compartida). **Simulada en la maqueta** — sin integración real; ver nota de alcance en [Reclutamiento2.md](Reclutamiento2.md#componente-apertura) sobre Instagram/TikTok.

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `UsuarioId` | nvarchar(450), FK → AspNetUsers.Id | Sí | null = credencial de empresa |
| `Plataforma` | nvarchar(30) | No | |
| `Conectado` | bit | No | |
| `NombreCuenta` | nvarchar(150) | Sí | |

**Índices:** único (`UsuarioId`, `Plataforma`) — con `UsuarioId` nulo tratado como un solo valor de empresa por plataforma.

⚠️ En producción esto NO debe guardar tokens/claves en texto plano — se integraría con OAuth de cada plataforma o un vault de secretos; esta tabla solo guardaría la referencia (`Conectado`, `NombreCuenta`), no la credencial en sí.

### LogActividad — 🟡 Propuesto
*(origen: Módulo 1 · Apertura, pero genérico — no exclusivo de Reclutamiento)* — Registro de acciones de usuario para auditoría. Usado primero para registrar el inicio de una Apertura, pensado para reutilizarse en el resto del sistema.

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `UsuarioId` | nvarchar(450), FK → AspNetUsers.Id | No | |
| `Accion` | nvarchar(200) | No | texto libre, ej. "Inicio de búsqueda" |
| `Entidad` | nvarchar(50) | No | ej. "Apertura", "Aviso", "Solicitud" (usado por Atracción: carga masiva, precalificación, activación) |
| `EntidadId` | int | No | Id del registro afectado |
| `FechaHora` | datetime2 | No | default getutcdate() |

**Índices:** `IX_LogActividad_Entidad_EntidadId`, `IX_LogActividad_FechaHora`

### Postulante — 🟡 Propuesto
*(origen: Módulo 2 · Preselección · Atracción — promovida desde `plan-reclutamiento.md` v1 y ampliada con los campos que usa la precalificación)* — Maestro global de postulantes: la misma persona puede repetirse entre distintas Solicitudes (ver `PostulanteSolicitud`).

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `Nombre` | nvarchar(200) | No | |
| `RUT` | nvarchar(20) | No | único |
| `Correo` | nvarchar(200) | Sí | reemplaza al `Contacto` genérico de v1 |
| `Telefono` | nvarchar(30) | Sí | reemplaza al `Contacto` genérico de v1 |
| `Sexo` | nvarchar(20) | Sí | |
| `Edad` | int | Sí | en la maqueta es un valor fijo; en producción evaluar si conviene derivarlo de `FechaNacimiento` en vez de guardarlo directo |
| `Comuna` | nvarchar(100) | Sí | comuna de residencia |
| `AniosExperiencia` | int | Sí | |
| `RubroExperiencia` | nvarchar(150) | Sí | texto libre en la maqueta; candidato a catálogo si se necesita filtrar por rubro de forma más estricta |
| `EstadoCivil` | nvarchar(30) | Sí | *(origen: carga masiva Computrabajo)* |
| `Nacionalidad` | nvarchar(50) | Sí | *(origen: carga masiva Computrabajo)* |
| `Discapacidad` | bit | Sí | *(origen: carga masiva Computrabajo)* null = no informado — relevante para ley de inclusión laboral |
| `RentaPretendida` | decimal(12,2) | Sí | *(origen: carga masiva Computrabajo)* permite comparar a futuro contra `PerfilCargoVersion.RentaFija` |
| `TituloCV` | nvarchar(200) | Sí | *(origen: carga masiva Computrabajo)* headline que el postulante declara en su CV — más específico que `RubroExperiencia`, que es la categoría usada por la precalificación |
| `DescripcionProfesional` | nvarchar(max) | Sí | *(origen: carga masiva Computrabajo)* resumen libre del postulante |
| `Habilidades` | nvarchar(max) | Sí | *(origen: carga masiva Computrabajo / RRSS)* texto libre — junta licencias de conducir, idiomas, conocimientos informáticos, competencias y maquinaria (ej. grúa horquilla); no se modela como catálogo estructurado por ahora, ver nota de carga masiva en [Reclutamiento2.md](Reclutamiento2.md#componente-atracción) |
| `FechaCreacion` | datetime2 | No | default getutcdate() |

**Índices:** `IX_Postulante_RUT` (único)

### PostulanteSolicitud — 🟡 Propuesto
*(origen: Módulo 2 · Preselección · Atracción)* — Postulación de un `Postulante` a una `Solicitud` puntual: historial por solicitud. Se nombra distinto de `PostulanteHistorial` (trayectoria/hoja de vida, ver abajo) para no mezclar los dos conceptos.

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `PostulanteId` | int, FK → Postulante.Id | No | |
| `SolicitudId` | int, FK → Solicitud.Id | No | |
| `Origen` | nvarchar(20) | No | Directa (vía `PostulacionCandidato`) / CargaMasiva |
| `FechaIngreso` | datetime2 | No | |
| `Puntaje` | int | Sí | 0-100, resultado de la precalificación de Atracción; null hasta que se calcula |
| `SeleccionadoPreseleccion` | bit | No | default 0 — pasa a 1 cuando el Reclutador lo activa para Preselección |
| `EtapaPreseleccion` | nvarchar(20) | Sí | *(origen: Preselección)* null (no iniciado) / Preseleccionado / Seleccionado |
| `PuntajeInicial` | int | Sí | *(origen: Preselección)* 0-100, evaluación ChatGPT tras la parte automática del motor de tareas |
| `PuntajeFinal` | int | Sí | *(origen: Preselección)* 0-100, evaluación ChatGPT tras el certificado WHO — es el que se usa para aprobar |
| `PublicacionId` | int, FK → Publicacion.Id | Sí | *(origen: carga masiva)* liga la postulación con la plataforma/Publicacion de Apertura de la que vino, cuando corresponde |
| `CanalOrigen` | nvarchar(50) | Sí | *(origen: carga masiva)* texto libre para cargas que no vienen de una `Publicacion` formal de Apertura (ej. "RRSS MRKTG" — leads de Facebook/Instagram sin pasar por un aviso publicado) |
| `SituacionActual` | nvarchar(100) | Sí | *(origen: carga masiva Computrabajo)* situación laboral del postulante al momento de la carga (ej. "Buscando empleo activamente") — snapshot por postulación, no del maestro global |
| `UltimaActualizacionCV` | datetime2 | Sí | *(origen: carga masiva Computrabajo)* fecha de última actualización del CV en el portal de origen |
| `UltimoLoginPortal` | datetime2 | Sí | *(origen: carga masiva Computrabajo)* último acceso del postulante al portal de origen — indicador de vigencia/actividad |
| `AdecuacionPortal` | int | Sí | *(origen: carga masiva Computrabajo)* % de adecuación calculado por el portal externo (ej. Computrabajo) — informativo, **no** se mezcla con `Puntaje` (que es nuestra propia precalificación) |
| `RespuestasPreguntasPortal` | nvarchar(max) | Sí | *(origen: carga masiva Computrabajo)* texto/JSON con las preguntas de filtrado del aviso y sus respuestas — varían por portal/aviso, no se normalizan en columnas propias |
| `EtapaEvaluacion` | nvarchar(20) | Sí | *(origen: Módulo 2 · Evaluación)* null (aún no llega a Evaluación) / Validado / Rechazado |
| `AprobadoPorSupervisorId` | nvarchar(450), FK → AspNetUsers.Id | Sí | *(origen: Evaluación)* usuario `Supervisor Operaciones` que aprobó o rechazó |
| `FechaEvaluacion` | datetime2 | Sí | *(origen: Evaluación)* fecha de la aprobación/rechazo |
| `ComentarioEvaluacion` | nvarchar(500) | Sí | *(origen: Evaluación)* motivo del rechazo, opcional |

**Índices:** `IX_PostulanteSolicitud_SolicitudId`, `IX_PostulanteSolicitud_PublicacionId`, único (`PostulanteId`, `SolicitudId`)

**Precalificación (Atracción):** el puntaje se calcula contra un valor objetivo por criterio (no contra completitud de datos), definido por el Reclutador en un panel con 5 selectores — Sexo, Edad, Comuna, Años de experiencia, Rubro — cada uno con un peso que debe sumar 100 entre todos. La configuración de pesos/objetivos por Solicitud no tiene todavía tabla propia en este diccionario (en la maqueta vive en memoria, `CONFIG_PRECALIFICACION` indexado por `SolicitudId`) — pendiente de modelar como tabla (`SolicitudCriterioPrecalificacion` o similar) si se lleva a producción.

### InteraccionIA — 🟡 Propuesto
*(origen: Módulo 2 · Preselección · Atracción)* — "Maestro de costos ChatGPT": registra cada interacción con IA (ej. extracción de datos desde un CV en PDF) y su costo asociado, por Cliente-Solicitud-Postulante.

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `ClienteId` | int, FK → Cliente.Id | No | |
| `SolicitudId` | int, FK → Solicitud.Id | No | |
| `PostulanteId` | int, FK → Postulante.Id | No | |
| `ReclutadorId` | nvarchar(450), FK → AspNetUsers.Id | No | *(origen: Costos IA)* quién generó la interacción — para poder filtrar el panel de costos por Reclutador |
| `Proveedor` | nvarchar(50) | No | *(origen: Costos IA)* "Azure OpenAI" por ahora — único proveedor de esta tabla, pero se deja explícito para no asumirlo del `Modelo` |
| `Proposito` | nvarchar(100) | No | ej. "Extracción de CV" (Atracción), "Evaluación inicial de candidato" / "Evaluación final de candidato" (Preselección) |
| `Modelo` | nvarchar(50) | No | ej. "gpt-4o-mini" |
| `TokensEntrada` | int | No | |
| `TokensSalida` | int | No | |
| `CostoUsd` | decimal(10,4) | No | |
| `FechaHora` | datetime2 | No | default getutcdate() |

**Índices:** `IX_InteraccionIA_SolicitudId`, `IX_InteraccionIA_ClienteId`

> **Nota de alcance:** en la maqueta, la carga masiva (Excel/PDF) y la extracción por IA están **simuladas** — no hay parseo real de archivos ni llamada real a la API de OpenAI. Los botones de carga insertan un lote de ejemplo fijo; la carga por PDF además simula una fila en `InteraccionIA` con un costo de ejemplo. Ver nota equivalente para Apertura en [Reclutamiento2.md](Reclutamiento2.md#componente-apertura).

### PreseleccionTarea — 🟡 Propuesto
*(origen: Módulo 2 · Preselección · Preselección)* — "Maestro de tareas de preselección" del enunciado: una fila por cada una de las 9 tareas del motor (Inicio, Envío de correo, Upload de documentos, Test Piamentor, Validación ChatGPT inicial, Entrevista, Certificado WHO, Validación ChatGPT final, Validación final), por `PostulanteSolicitud`.

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `PostulanteSolicitudId` | int, FK → PostulanteSolicitud.Id | No | |
| `Tipo` | nvarchar(30) | No | Inicio / EnvioCorreo / UploadDocumentos / TestPiamentor / ValidacionIA1 / Entrevista / CertificadoWHO / ValidacionIA2 / Validacion |
| `Estado` | nvarchar(20) | No | Pendiente / Completado |
| `Fecha` | datetime2 | Sí | null hasta que se completa |
| `Detalle` | nvarchar(300) | Sí | texto libre — ej. "puntaje inicial 72", "correo enviado (simulado)" |

**Índices:** `IX_PreseleccionTarea_PostulanteSolicitudId`, único (`PostulanteSolicitudId`, `Tipo`)

**Listado con columnas de estado:** el listado de preseleccionados muestra 5 columnas de ícono-estado (Correo/Certificados/Test/Entrevista/Antecedentes WHO) — excepción documentada al estándar de listados, ver [Estandares-UI.md](Estandares-UI.md#excepción-tableros-de-seguimiento-multi-etapa). "Certificados" no sale de `PreseleccionTarea` sino de si existe al menos un `DocumentoLegal` para ese postulante-solicitud.

### DocumentoLegal — 🟡 Propuesto
*(origen: Módulo 2 · Preselección · Preselección)* — Documento PDF o imagen del preseleccionado (ej. contrato, cédula, certificado de antecedentes), subido por el Reclutador desde su perfil o por el propio Postulante desde el módulo de documentos.

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `PostulanteSolicitudId` | int, FK → PostulanteSolicitud.Id | No | |
| `NombreArchivo` | nvarchar(200) | No | |
| `TipoArchivo` | nvarchar(10) | No | PDF / Imagen |
| `Origen` | nvarchar(20) | No | Reclutador / Postulante |
| `RutaArchivo` | nvarchar(500) | No | referencia al storage — estructura propuesta: `/postulantes/{PostulanteId}/solicitudes/{SolicitudId}/legales/{archivo}` |
| `FechaCarga` | datetime2 | No | default getutcdate() |

**Índices:** `IX_DocumentoLegal_PostulanteSolicitudId`

### TestPiamentor — 🟡 Propuesto
*(origen: Módulo 2 · Preselección · Preselección)* — "Maestro de Piamentor" del enunciado: resultado del test con IA de un preseleccionado.

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `PostulanteSolicitudId` | int, FK → PostulanteSolicitud.Id | No | |
| `ReclutadorId` | nvarchar(450), FK → AspNetUsers.Id | No | *(origen: Costos IA)* |
| `Proveedor` | nvarchar(50) | No | *(origen: Costos IA)* "Piamentor.cl" |
| `CostoUsd` | decimal(10,4) | No | *(origen: Costos IA)* costo plano por test — en la maqueta un valor fijo de ejemplo, no hay tarifa real de Piamentor.cl |
| `Puntaje` | int | No | 0-100 |
| `Fecha` | datetime2 | No | default getutcdate() |

**Índices:** `IX_TestPiamentor_PostulanteSolicitudId`

### CertificadoWHO — 🟡 Propuesto
*(origen: Módulo 2 · Preselección · Preselección)* — Resultado del certificado de antecedentes solicitado al sistema WHO tras validar la entrevista.

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `PostulanteSolicitudId` | int, FK → PostulanteSolicitud.Id | No | |
| `ReclutadorId` | nvarchar(450), FK → AspNetUsers.Id | No | *(origen: Costos IA)* |
| `Proveedor` | nvarchar(50) | No | *(origen: Costos IA)* "WHO.cl" |
| `CostoUsd` | decimal(10,4) | No | *(origen: Costos IA)* costo plano por certificado — en la maqueta un valor fijo de ejemplo, no hay tarifa real de WHO.cl |
| `Resultado` | nvarchar(200) | No | ej. "Sin observaciones" |
| `Fecha` | datetime2 | No | default getutcdate() |

**Índices:** `IX_CertificadoWHO_PostulanteSolicitudId`

### EntrevistaPreseleccion — 🟡 Propuesto
*(origen: Módulo 2 · Preselección · Preselección)* — Checklist, preguntas/respuestas y cierre de la entrevista por video, 1:1 con la tarea "Entrevista" de `PreseleccionTarea`.

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `PostulanteSolicitudId` | int, FK → PostulanteSolicitud.Id | No | único — 1 entrevista por preseleccionado |
| `FechaHora` | datetime2 | No | fecha/hora agendada |
| `Checklist` | nvarchar(max) | Sí | JSON — lista de checkpoints con su marca (ver `EntrevistaChecklistItem` si se normaliza) |
| `Preguntas` | nvarchar(max) | Sí | JSON — lista de pregunta/respuesta (ver `EntrevistaPregunta` si se normaliza) |
| `Cierre` | nvarchar(max) | Sí | comentario de cierre del Reclutador |
| `Cerrada` | bit | No | default 0 |
| `ReclutadorId` | nvarchar(450), FK → AspNetUsers.Id | Sí | quién la registró |

**Índices:** único (`PostulanteSolicitudId`)

> En la maqueta `Checklist` y `Preguntas` quedan embebidos como arreglos en memoria (no JSON persistido) por simplicidad; si se lleva a producción, evaluar si conviene normalizar a tablas `EntrevistaChecklistItem`/`EntrevistaPregunta` (1:N) en vez de JSON, sobre todo si se necesita reportar sobre checklist estándar entre entrevistas.

> **Nota de alcance (Preselección):** además de lo anterior, quedan **simulados** en la maqueta: envío de correo automático, Test Piamentor, ambas evaluaciones ChatGPT, certificado WHO, y agenda de calendario para la entrevista — mismo criterio que Apertura/Atracción, sin integración real. Ver nota equivalente en [Reclutamiento2.md](Reclutamiento2.md#componente-preselección).

### Panel de Costos IA (Admin) — no es una tabla nueva

*(origen: Admin > Costos IA, 2026-08-26)* — Control independiente para monitorear el gasto en los 3 proveedores externos que hoy generan costo: **Azure OpenAI** (`InteraccionIA`), **Piamentor.cl** (`TestPiamentor`) y **WHO.cl** (`CertificadoWHO`). No es una tabla propia — es una vista que junta filas de las 3 tablas de arriba (cada una sigue siendo la fuente de verdad de su propio flujo) para mostrarlas y filtrarlas juntas.

- **Acceso:** módulo aparte dentro de Admin, visible solo para `SuperAdmin` y `Admin` (mismo patrón de `roles` a nivel de módulo que Usuarios/Perfiles/Auditoría — `Supervisor Administrativo` no lo ve, a diferencia del resto de Admin).
- **Antes vivía embebido** en la ficha de Atracción (`panelCostosIA`, visible para cualquier Reclutador) — se sacó de ahí porque contradecía "solo para admin"; ahora Atracción no muestra costos en absoluto.
- **Filtros:** Cliente, Solicitud, Reclutador, Postulante, Proveedor externo, Componente (Atracción / Preselección — cuál de los dos generó el gasto, derivado de `Proposito` para `InteraccionIA` y fijo "Preselección" para Piamentor/WHO, que hoy solo se usan ahí).
- **Presentación:** un valor total por fila (`CostoUsd`), sin desglose de tokens ni valor unitario — a diferencia de la ficha vieja de Atracción, que sí mostraba tokens.

---

## Reclutamiento — v1, aún vigentes (pendiente de revisar en Módulo 2)

*(De `plan-reclutamiento.md`. `Postulante` ya se promovió a la sección v2 de arriba, ampliada para la precalificación de Atracción. Lo que queda acá — hoja de vida, documentos, extracción por IA de CV — sigue pendiente como desarrollo aparte, más grande, dentro de Módulo 2 · Preselección.)*

### PostulanteHistorial — 🟡 Propuesto

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `PostulanteId` | int, FK → Postulante.Id | No | |
| `Tipo` | nvarchar(50) | No | Experiencia / Educacion / Otro |
| `Descripcion` | nvarchar(max) | Sí | |
| `FechaDesde` | datetime2 | Sí | |
| `FechaHasta` | datetime2 | Sí | |
| `Origen` | nvarchar(20) | No | Manual / IA |

### PostulanteDocumento — 🟡 Propuesto

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `PostulanteId` | int, FK → Postulante.Id | No | |
| `Tipo` | nvarchar(50) | No | CV / CertificadoAntecedentes / Otro |
| `RutaArchivo` | nvarchar(500) | No | referencia al storage (Fase 0) |
| `FechaCarga` | datetime2 | No | |
| `EstadoProcesamiento` | nvarchar(20) | No | Pendiente / Procesando / Listo / Error |

### DocumentoExtraccionIA — 🟡 Propuesto

| Campo | Tipo | Null | Notas |
|---|---|---|---|
| `Id` | int, PK | No | |
| `PostulanteDocumentoId` | int, FK → PostulanteDocumento.Id | No | |
| `TextoExtraido` | nvarchar(max) | Sí | `.txt` generado por IA |
| `DatosEstructurados` | nvarchar(max) | Sí | JSON |
| `FechaProcesamiento` | datetime2 | Sí | |
| `Estado` | nvarchar(20) | No | Pendiente / Procesando / Listo / Error |

---

## Reemplazadas / obsoletas (v1 → v2)

- **`ProcesoSeleccion`** 🔴 — reemplazada por `Solicitud` + `PerfilCargo`/`PerfilCargoVersion`. Los campos cliente/cargo/plazo/renta desglosada de v1 ahora viven repartidos entre esas tres tablas.
- **`ProcesoSeleccionEtapa`** 🔴 — sin reemplazo todavía. No se lleva a la estructura activa; se redefine cuando detallemos Módulo 2 (Preselección) / Módulo 3 (Selección).
- **`ProcesoSeleccionPostulante`** 🔴 — igual que la anterior: pendiente de redefinir junto con Módulo 2/3, no forma parte de la estructura activa por ahora.

---

## Pendiente de otras apps

- **Remuneraciones** (`remuneraciones.md`): sin tablas detalladas acá todavía — se agregan si corresponde cuando se revise esa app con este mismo método.
