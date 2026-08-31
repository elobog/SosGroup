# Reclutamiento — Plan v2

> Reemplaza a [plan-reclutamiento.md](plan-reclutamiento.md), reciclando lo ya definido ahí (modelo de datos base, servicios de storage/IA, supuestos y pendientes) sobre el nuevo flujo que envió el cliente.

## Aplicación: Reclutamiento

## Módulos

1. **Solicitud**
2. **Preselección**
3. **Selección**
4. **Ingreso**

---

## Módulo 1 — Solicitud

**Componentes:**
- Solicitud *(detallado abajo)*
- Perfil de Cargo *(detallado abajo)*
- Apertura *(detallado abajo)*

### Componente: Solicitud

**Objetivo:** gestionar las solicitudes de personal externo que requieren los clientes de SOSGroup, definidas por perfil de cargo, lugar de prestación del servicio y fecha de inicio, entre otras cosas.

**Roles que interactúan:**
- **Supervisor Operaciones**: empleado SOSGroup, responsable en terreno del personal externo que SOSGroup provee a sus clientes. Cargo operativo de terreno. Puede atender varios clientes.
- **Reclutador**: empleado SOSGroup, ejecuta la solicitud de personal de una empresa específica en base a un perfil de cargo definido. Gestiona una o más empresas; cada empresa puede tener una o más solicitudes activas o en ejecución.

> Resuelto: el modelo de roles de Identity/Acceso quedó definido como `SuperAdmin` (AITBP, proveedor), `Admin` (SOSGroup), `Supervisor Operaciones`, `Supervisor Administrativo` y `Reclutador` — ver [BD_Dashboard.md](BD_Dashboard.md#identity--acceso). El "Supervisor" de este componente es específicamente `Supervisor Operaciones`; `Supervisor Administrativo` no tiene acceso a Reclutamiento (queda del lado de Remuneración/Admin).

**Acciones:**
- Nueva Solicitud
- Nuevo Cliente
- Nuevo Perfil de Cargo
- Ver/Modificar Solicitud
- Ver/Modificar Cliente
- Ver/Modificar Perfil de Cargo
- Asignar Reclutador a Cliente
- Asignar Supervisor Operaciones a Cliente

**Flujo:**
1. El Supervisor Operaciones crea una Solicitud nueva, o agrega una nueva ronda de búsqueda (`SolicitudDetalle`) a una Solicitud existente cuando se requiere más personal.
2. El Reclutador asignado al Cliente recibe aviso de la nueva solicitud o ronda.
3. El Reclutador inicia el proceso (la ronda pasa a "En proceso") — esto dispara el Módulo 2 (Preselección).
4. Al completarse una ronda, su estado pasa a "Cerrada". Si no quedan rondas abiertas, la Solicitud pasa a "Cerrada". Si el Supervisor Operaciones abre una nueva ronda, la Solicitud vuelve a "Activa" automáticamente.

**Regla de negocio clave:** el estado de la Solicitud es derivado, no se edita directamente — depende del estado de sus rondas (`SolicitudDetalle`).

**Entidades relacionadas:** `Cliente`, `PerfilCargo`, `PerfilCargoVersion`, `Solicitud` (ahora con `SucursalId`, opcional — ver `ClienteSucursal` en Admin), `SolicitudDetalle`, `ClienteReclutador`, `ClienteSupervisor` — ver [Estructura de datos](#estructura-de-datos).

### Componente: Perfil de Cargo

**Objetivo:** gestionar los perfiles de cargo de cada cliente y su historial de condiciones (renta), separando lo que es estable (competencias) de lo que se actualiza en el tiempo (renta fija/variable/beneficios).

**Acciones:**
- Nuevo Perfil de Cargo — se puede iniciar desde el listado de Perfiles de Cargo, o directamente desde el formulario de Nueva Solicitud (si el cliente elegido todavía no tiene perfiles, o simplemente para crear uno nuevo sin salir del flujo). En ese segundo caso, al guardar se vuelve automáticamente a Nueva Solicitud — con el perfil recién creado seleccionado solo si ya tuviera una condición aprobada, lo cual no es el caso al crearlo (ver regla de aprobación abajo).
- Ver ficha de Perfil de Cargo: encabezado (competencias, no cambia entre versiones) + historial completo de condiciones (todas las versiones, con su estado Pendiente/Vigente/Histórica).
- Agregar nueva condición: crea una versión nueva marcada "Pendiente" — no reemplaza a la vigente hasta que se apruebe (ver regla de aprobación).
- Aprobar condición: cada uno de los dos roles aprobadores confirma la condición pendiente desde la ficha del perfil, con su propia sesión.

**Regla de negocio — aprobación:** tanto crear un Perfil de Cargo como agregarle una nueva condición generan una versión en estado **Pendiente**, que requiere la aprobación de **Supervisor Operaciones** *y* **Supervisor Administrativo** antes de quedar Vigente. Mientras está pendiente, la condición Vigente anterior (si existía) sigue rigiendo sin interrupción — y si es la primera versión del perfil, el perfil simplemente no está disponible para usar en una Solicitud todavía. Solo cuando aprueban los dos, la versión pendiente pasa a Vigente y la anterior pasa a Histórica. Esto es lo que motivó que `Supervisor Administrativo` (antes sin acceso a Reclutamiento) ahora entre al módulo Solicitud — ver [BD_Dashboard.md](BD_Dashboard.md#identity--acceso).

**Regla de negocio — snapshot:** al crear una Solicitud, se congela automáticamente la versión Vigente del perfil en ese momento (`Solicitud.PerfilCargoVersionId`). Si el perfil recibe una condición nueva (aprobada) después, las Solicitudes ya creadas **no se actualizan solas** — siguen mostrando la versión con la que fueron creadas. Es un snapshot, no una referencia viva.

**Entidades relacionadas:** `PerfilCargo`, `PerfilCargoVersion`, `PerfilCargoVersionAprobacion` — ver [Estructura de datos](#estructura-de-datos).

### Componente: Apertura

**Objetivo:** formalizar el inicio de la búsqueda una vez que el Perfil de Cargo de la Solicitud tiene una condición Vigente (aprobada por Supervisor Operaciones y Supervisor Administrativo). Registra el inicio en el Log de Actividad del sistema y simula el aviso por correo a los supervisores que aprobaron. Es la tercera pestaña del Módulo 1, junto a Solicitudes y Perfiles de cargo.

**Roles que interactúan:**
- **Reclutador**: prepara el aviso (texto único, un modelo para todas las plataformas), lo publica y hace seguimiento del log de publicaciones y postulantes.

**Acciones:**
- Iniciar: selecciona una Solicitud ya con Perfil de Cargo aprobado y arranca su Apertura (1 Apertura por Solicitud).
- Redactar aviso: editor con campos prellenados desde el Perfil de Cargo (descripción, requisitos, renta) + campos propios (título, modalidad, ubicación, cierre de postulaciones) — genera un único HTML de aviso. Se pueden crear nuevas versiones del aviso sobre la misma Apertura.
- Copiar aviso: copia el HTML generado (con reintento manual si el navegador bloquea el portapapeles).
- Publicar aviso: selecciona plataformas y publica — simulado como "vía API" en las plataformas donde el usuario (o la empresa) tiene una credencial conectada, o como "pendiente de carga manual" en las que no. Cada plataforma publicada genera su propio **link único de postulación**.
- Vista previa: el Reclutador puede ver el aviso + el formulario de postulación exactamente como lo va a ver el candidato, antes (o después) de publicar.

**Flujo:**
1. Inicio — arranca con la aprobación del Perfil de Cargo (componente anterior); no hay un paso de aprobación adicional para el aviso.
2. Texto Aviso — se construye el aviso en la plataforma, un único modelo para las 6 plataformas.
3. Publicación — el sistema publica vía API en cl.computrabajo.com, Trabajando.com, LinkedIn, Facebook, Instagram y TikTok, según la credencial de cada reclutador o de la empresa. Cada publicación queda con su propio link de postulación.
4. Una vez publicada, se pueden hacer nuevas publicaciones para la misma Solicitud con avisos nuevos.

**Línea de tiempo:** Inicio / Texto Aviso / Publicación / Última Publicación — se muestra con la fecha de cada hito una vez ocurre.

**Log de Publicaciones:** Usuario, Plataforma, Fecha-hora, Método (API/Manual), Estado, Cantidad de postulantes, link de postulación (con botón copiar).

**Página pública de postulación (sin login):** el link de cada publicación no apunta a la plataforma externa — apunta a una página propia de SOS Group donde el candidato ve el aviso y postula con un formulario corto (Nombre, RUT, Correo, Teléfono). Esto es lo que resuelve contar postulantes sin depender de que la plataforma tenga API: **todas** las postulaciones entran por el mismo lado, controlado por SOS Group. Cada postulación recibida queda listada en "Postulantes recibidos" dentro de la ficha de Apertura, y suma automáticamente al contador de esa Publicación (el ajuste manual sigue disponible para postulantes que llegan por fuera, ej. WhatsApp).

> **Nota de alcance:** en la maqueta la integración con las 6 plataformas y las credenciales de publicación quedan **simuladas** — no hay conexión real vía API ni descarga de archivos real (el entorno de la maqueta no permite disparar descargas). Instagram y TikTok en particular no tienen API pública de publicación de empleos hoy — para una implementación real, esas dos casi con certeza quedarían en el flujo manual (descarga + carga a mano) en vez de vía API. La página pública también está simulada: en la maqueta es un deep-link dentro del mismo artifact (`#postular-{id}`) que salta el login; en producción sería una ruta pública real, sin pasar por el login interno. Y esta captura de postulación es **mínima** (solo contacto) — la ficha completa de Postulante (hoja de vida, CV, extracción por IA) del plan v1 sigue pendiente como desarrollo aparte, más grande, dentro de Módulo 2 · Preselección.

**Entidades relacionadas:** `Apertura`, `Aviso`, `Publicacion`, `CredencialPlataforma`, `LogActividad`, `PostulacionCandidato` — ver [Estructura de datos](#estructura-de-datos).

---

## Módulo 2 — Preselección

**Componentes:**
- Atracción *(detallado abajo)*
- Preselección *(detallado abajo)*
- Evaluación *(detallado abajo)*
- Validación *(pendiente)*

### Componente: Atracción

**Objetivo:** ejecutar la estrategia de búsqueda de postulantes inscritos en los distintos portales activados en Apertura (cl.computrabajo.com, Trabajando.com, LinkedIn, Facebook, Instagram), cargarlos al sistema (de forma directa o masiva), precalificarlos con un puntaje ponderado y enviar a Preselección a los que el Reclutador seleccione.

**Roles que interactúan:**
- **Reclutador**: revisa postulantes inscritos, hace la carga masiva, define los parámetros de precalificación y selecciona a quiénes activar para Preselección.

**Línea de tiempo:** Atracción / Preselección / Evaluación / Validación — 4 etapas del Módulo 2, con Atracción marcada como actual. La ficha además muestra los días restantes desde hoy hasta la fecha de inicio de servicio de la Solicitud.

**Acciones:**
- Revisar postulantes inscritos en la Solicitud (directos, vía formulario público de postulación de Apertura).
- Carga masiva: cargar candidatos desde Excel o CV en PDF. El PDF se procesa con IA (extracción de datos del CV); el Excel tiene formato variable según el portal de origen. Ambos flujos alimentan el maestro de Postulantes (que puede repetirse entre Solicitudes distintas).
- Precalificación: panel de control con 5 criterios (Sexo, Edad, Comuna, Años de experiencia, Rubro de experiencia), cada uno con un peso que debe sumar 100 entre todos, y un valor objetivo definido por el Reclutador (ej. rango de edad ideal, comuna preferida, rubro requerido). El sistema calcula un puntaje 0-100% por postulante según qué tan cerca está del objetivo de cada criterio, ponderado por su peso.
- Preselección: el Reclutador ordena la lista (asc/desc por columna), filtra por un rango de puntaje (selector 0%-100%), selecciona postulantes con checkbox (individual o "seleccionar todos" en el encabezado) y los activa para Preselección.
- Todas las acciones (carga, cálculo de precalificación, activación) quedan registradas en el Log de Actividad.

**Flujo:**
1. El Reclutador entra a Atracción desde la Solicitud y ve la lista de postulantes inscritos.
2. Carga nuevos postulantes de forma masiva (Excel o PDF).
3. Define pesos y valores objetivo en el panel de precalificación; el sistema calcula el puntaje ponderado de cada postulante.
4. Ordena y filtra la lista por puntaje, selecciona a los postulantes que le interesan.
5. Activa la selección para Preselección (componente siguiente).

> **Nota de alcance:** en la maqueta, la carga masiva está **simulada** — no hay parseo real de archivos Excel/PDF ni llamada real a la API de OpenAI/ChatGPT (el entorno de la maqueta no permite peticiones de red externas). Los botones de carga insertan un lote de ejemplo fijo, y la carga por PDF además simula una entrada en el maestro de costos de IA. El puntaje de precalificación se calcula contra un **valor objetivo por criterio** que el Reclutador define (no contra completitud de datos) — decisión confirmada con el cliente.

**Plan de carga real (para producción):** basado en revisar archivos reales que envía el cliente (export PDF y Excel de Computrabajo para un mismo aviso, y una base de leads de RRSS/Facebook-Instagram) — los tres traen columnas distintas entre sí, confirmando que el mapeo tiene que ser por origen, no genérico.

1. **Mapeo por origen:** una tabla de mapeo columna→campo por cada formato conocido (Computrabajo-PDF, Computrabajo-Excel, RRSS Lead Ads), extensible cuando aparezca un formato nuevo (Trabajando.com, LinkedIn, etc.).
2. **Al cargar:** el Reclutador indica la `Publicacion` de origen si el archivo corresponde a un aviso ya publicado en Apertura (queda en `PostulanteSolicitud.PublicacionId`); si no corresponde a ningún aviso formal (ej. un lote de leads de RRSS suelto), queda con `PostulanteSolicitud.CanalOrigen` como texto libre.
3. **Por fila:** normaliza el RUT → busca `Postulante` existente por RUT (upsert, sin pisar campos ya llenos con valores vacíos del archivo nuevo) → crea o actualiza el `PostulanteSolicitud` de esa Solicitud (sin duplicar si ya existía la postulación) → si el archivo trae experiencia/educación en texto libre, se manda a extracción IA (reutiliza `InteraccionIA`) para poblar `PostulanteHistorial` → si el archivo indica que hay CV disponible, crea un `PostulanteDocumento` pendiente de carga.
4. **Errores:** filas con RUT inválido u otro problema quedan en un log de errores de esa carga, visible al Reclutador, sin bloquear el resto del lote.
5. **Trazabilidad:** cada carga masiva registra en `LogActividad` cuántas filas se crearon/actualizaron/fallaron.

**Entidades relacionadas:** `Postulante` (ampliada con `EstadoCivil`, `Nacionalidad`, `Discapacidad`, `RentaPretendida`, `TituloCV`, `DescripcionProfesional`, `Habilidades`), `PostulanteSolicitud` (ampliada con `PublicacionId`, `CanalOrigen`, `SituacionActual`, `UltimaActualizacionCV`, `UltimoLoginPortal`, `AdecuacionPortal`, `RespuestasPreguntasPortal`), `InteraccionIA`, `LogActividad` — ver [Estructura de datos](#estructura-de-datos) y el diccionario completo en [BD_Dashboard.md](BD_Dashboard.md). `PostulanteSolicitud` es la postulación de un Postulante a una Solicitud puntual (registro por solicitud); se nombra distinto de un futuro `PostulanteHistorial` (trayectoria/hoja de vida del plan v1) para no mezclar los dos conceptos.

### Componente: Preselección

**Objetivo:** filtrar y priorizar a los postulantes que Atracción activó, ejecutando un motor de tareas por postulante (documentos, test, evaluación IA, entrevista, certificado de antecedentes) hasta aprobar su paso a Seleccionado. El Reclutador controla el avance de cada preseleccionado; el sistema lleva el registro de etapa y puntaje.

**Roles que interactúan:**
- **Reclutador**: realiza las entrevistas, gestiona documentos legales, aprueba o no cada preseleccionado.

**Línea de tiempo:** mismas 4 etapas del Módulo 2 (Atracción / Preselección / Evaluación / Validación), con Preselección marcada como actual.

**Motor de tareas (9 pasos, en orden):** Inicio → Envío de correo automático → Upload de documentos (postulante, vía módulo de documentos tipo chat) → Test Piamentor → Validación ChatGPT (inicial) → Entrevista por video → Certificado WHO → Validación ChatGPT (final) → Validación final (aprobar). Las primeras 4 son la "parte automática" del enunciado; en la maqueta cada una tiene un botón que simula su cumplimiento (no hay integración real con ningún proveedor).

**Acciones:**
- Iniciar proceso: selecciona uno o más postulantes activados desde Atracción (checkbox + "seleccionar todos") y los pasa a etapa Preseleccionado — dispara la tarea Inicio para cada uno.
- Listado de preseleccionados con columnas de estado por dimensión (Correo / Certificados / Test / Entrevista / Antecedentes WHO) — **excepción documentada al estándar de listados**, ver [Estandares-UI.md](Estandares-UI.md#excepción-tableros-de-seguimiento-multi-etapa). Cada ícono abre el módulo correspondiente o repite la acción (el de Correo reenvía el recordatorio).
- Ficha del preseleccionado: motor de tareas completo, gestión de documentos legales (PDF/imagen, sube el Reclutador o el propio postulante; el nombre del archivo es un link que abre un visor en pantalla — simulado, ver nota de alcance), módulo de entrevista (checklist + preguntas/respuestas + cierre) y aprobación final.
- Módulo de documentos del postulante: página pública (sin login, deep-link propio) con interfaz de chat donde el postulante "adjunta" sus documentos — misma familia de páginas públicas que la postulación de Apertura.
- Entrevista: el Reclutador completa un checklist de verificación, escribe las respuestas del postulante a un set de preguntas guía, y cierra la entrevista con un comentario. Al cerrarla, se habilita el botón **"Validado"**, que dispara la solicitud del certificado WHO.
- Evaluación ChatGPT: se ejecuta dos veces — la primera tras completarse la parte automática (asigna `PuntajeInicial`), la segunda tras el certificado WHO (asigna `PuntajeFinal`, el que se usa para aprobar).
- Aprobar preselección: cambia la etapa del postulante a Seleccionado. Disponible solo cuando la validación ChatGPT final está completa.

**Flujo:**
1. El listado de preseleccionados parte con los postulantes que Atracción activó; sigue creciendo mientras el ciclo de búsqueda esté abierto (Atracción puede activar más en cualquier momento).
2. El Reclutador selecciona filas y usa "Iniciar proceso" — cada postulante pasa a etapa Preseleccionado.
3. Se simulan en orden: envío de correo, upload de documentos (vía el módulo de chat del postulante), Test Piamentor.
4. Completada la parte automática, se calcula la evaluación ChatGPT inicial (puntaje 0-100).
5. El Reclutador agenda y registra la entrevista (checklist + preguntas/respuestas + cierre) y presiona "Validado" — esto solicita el certificado WHO (simulado).
6. Con el certificado WHO, se calcula la evaluación ChatGPT final.
7. El Reclutador aprueba la preselección — el postulante pasa a Seleccionado.

> **Nota de alcance:** todo lo que requeriría integración real queda **simulado** en la maqueta, mismo criterio que en Apertura/Atracción: envío de correo, Test Piamentor, ambas evaluaciones ChatGPT, certificado WHO, agenda de calendario para la entrevista, y almacenamiento real de archivos (los documentos son solo metadata, con una ruta simulada `/postulantes/{PostulanteId}/solicitudes/{SolicitudId}/legales/{archivo}`). El visor que abre al presionar el nombre de un documento tampoco muestra el archivo real — es una vista simulada (líneas de texto genéricas para PDF, ícono genérico para Imagen) que deja claro en pantalla que es una simulación. El "análisis de analítica IA" sobre el video de entrevista que menciona el enunciado queda fuera de esta primera vuelta — se lee como una posibilidad a evaluar, no un requisito confirmado.

**Entidades relacionadas:** `PostulanteSolicitud` (ampliada con `EtapaPreseleccion`, `PuntajeInicial`, `PuntajeFinal`), `PreseleccionTarea`, `DocumentoLegal`, `TestPiamentor`, `CertificadoWHO`, `EntrevistaPreseleccion`, `InteraccionIA` (reutilizada, nuevos valores de `Proposito`), `LogActividad` — ver [Estructura de datos](#estructura-de-datos).

### Componente: Evaluación

**Objetivo:** que el Supervisor Operaciones del cliente dé el visto bueno de negocio a cada postulante que ya superó Preselección (etapa `Seleccionado`), antes de que avance a Módulo 3 · Selección. Es un solo paso de aprobación — el filtro de calidad ya lo hizo el Reclutador al cerrar Preselección ("Aprobar preselección" en el componente anterior), así que Evaluación no repite esa revisión.

**Roles que interactúan:**
- **Supervisor Operaciones**: revisa y aprueba/rechaza a los postulantes `Seleccionado` de sus Solicitudes.

**Línea de tiempo:** mismas 4 etapas del Módulo 2 (Atracción / Preselección / Evaluación / Validación), con Evaluación marcada como actual.

**Acciones:**
- Listado de Solicitudes en curso con contador de postulantes pendientes de aprobación.
- Ficha de Solicitud: lista de postulantes en etapa `Seleccionado`, con botón Aprobar/Rechazar por fila (individual, sin necesidad de checkbox masivo dado que es una decisión uno a uno).
- Rechazar: requiere comentario breve (queda en el registro de la decisión).

**Flujo:**
1. El postulante llega a esta cola automáticamente al quedar `Seleccionado` en Preselección.
2. El Supervisor Operaciones revisa la ficha (documentos, entrevista, puntajes) y decide.
3. Aprobado → etapa `Validado`, queda listo para Módulo 3 · Selección.
4. Rechazado → etapa `Rechazado` (terminal para esta Solicitud).

**Entidades relacionadas:** `PostulanteSolicitud` (ampliada con `EtapaEvaluacion`, `AprobadoPorSupervisorId`, `FechaEvaluacion`, `ComentarioEvaluacion`), `LogActividad` — ver [Estructura de datos](#estructura-de-datos).

---

## Módulo 3 — Selección

*(Pendiente para desarrollo futuro.)*

---

## Módulo 4 — Ingreso

*(Pendiente para desarrollo futuro.)*

---

## Estructura de datos

> El detalle de campos, tipos e índices vive en [BD_Dashboard.md](BD_Dashboard.md) (diccionario de datos único para toda la base). Acá solo se listan los nombres de las entidades que surgen de cada componente, como índice rápido.

- **Compartida (Admin):** `Cliente`
- **Módulo 1 · Solicitud:** `PerfilCargo`, `PerfilCargoVersion`, `PerfilCargoVersionAprobacion`, `Solicitud`, `SolicitudDetalle`, `ClienteReclutador`, `ClienteSupervisor`, `Apertura`, `Aviso`, `Publicacion`, `CredencialPlataforma`, `LogActividad`, `PostulacionCandidato`
- **Módulo 2 · Preselección — Atracción:** `Postulante`, `PostulanteSolicitud`, `InteraccionIA`
- **Módulo 2 · Preselección — Preselección:** `PreseleccionTarea`, `DocumentoLegal`, `TestPiamentor`, `CertificadoWHO`, `EntrevistaPreseleccion`
- **Módulo 2 · Preselección — Evaluación:** sin entidades nuevas, extiende `PostulanteSolicitud`

---

## Roles y accesos

> Modelo completo de roles de Identity/Acceso (aplica a todo el portal, no solo Reclutamiento) documentado en [BD_Dashboard.md](BD_Dashboard.md#identity--acceso).

- **Supervisor Operaciones** (Solicitud): cargo operativo de terreno. Acceso solo a Reclutamiento; ve únicamente los clientes/solicitudes donde está asignado.
- **Reclutador** (Solicitud): gestiona clientes y ejecuta solicitudes. Acceso solo a Reclutamiento; ve únicamente los clientes/solicitudes donde está asignado.
- **Admin** y **SuperAdmin**: ven todo dentro de Reclutamiento, sin filtro por asignación.

## Pendientes / a confirmar

- Definir los módulos "exclusivos de SuperAdmin" (por ahora `SuperAdmin` y `Admin` tienen el mismo acceso a nivel de apps).
- Detallar componente Perfil de Cargo (pantallas, quién puede crear/editar versiones).
- Detallar componente Apertura (Módulo 1).
- Detallar componente Validación (Módulo 2) — Atracción, Preselección y Evaluación ya están detallados arriba.
- Módulos 3 (Selección) y 4 (Ingreso): sin definir, desarrollo futuro.
