# Plan de desarrollo — App Remuneraciones

Plan por etapas para construir la app Remuneraciones sobre el proyecto Blazor Server ya existente (Identity + SQL Server). La maqueta de referencia está en [sosgroup.md](sosgroup.md).

## Contexto y supuestos

- Las remuneraciones se pagan a través de **Talana**. La empresa no calcula ni paga sueldos por fuera de Talana: su rol es preparar y validar la información antes de enviarla.
- **BaseTalana**: el listado de empleados cargados en Talana, a los que efectivamente se les paga sueldo a través de la empresa. Es la fuente de verdad de "quién está activo para efectos de pago" — no se crea manualmente, se **lee desde Talana**.
- **Clientes** ya no es un módulo de esta app: es una entidad compartida administrada en **Admin** (ver [admin.md](admin.md)). Un contrato de servicio siempre está asociado a un cliente definido ahí.
- Falta documentar antes de implementar la integración real:
  - Endpoint(s) de la API de Talana: consulta de empleados (BaseTalana), consulta de licencias médicas y vacaciones, envío del cálculo mensual.
  - Formato exacto de autenticación/credenciales de la API de Talana.
  - Qué se considera "día hábil" para efectos del calendario de auditoría (feriados, región).
- Todo el desarrollo asume el stack actual: Blazor Server (.NET 10), ASP.NET Core Identity, EF Core + SQL Server.

## Módulos

1. Base Talana
2. Contratos
3. Cálculo
4. Auditoría
5. Panel de control (landing de la app)

## Orden sugerido de las fases

```
Fase 0 (base) → Fase 1 (Base Talana) → Fase 2 (Contratos) → Fase 3 (Cálculo) → Fase 4 (Auditoría) → Fase 5 (Panel de control)
```

Base Talana va primero porque Contratos y Cálculo dependen de saber qué empleados existen y a qué contrato pertenecen. Panel de control va al final porque solo agrega valor una vez que hay datos reales de Cálculo y Auditoría para mostrar.

---

## Fase 0 — Base común

**Objetivo:** infraestructura y modelo de datos compartido por los demás módulos.

**Incluye:**
- Modelo de datos inicial (tablas/entidades EF Core):
  - `ContratoServicio` (cliente, nombre/servicio, tipo, fecha inicio, fecha término, estado)
  - `EmpleadoTalana` (BaseTalana: nombre, RUT, cliente, contrato, cargo, estado en Talana, fecha de ingreso)
  - `Calculo` (contrato, periodo, estado: Abierto / Enviado / Cerrado, fecha de envío)
  - `CalculoEmpleado` (una fila por persona dentro de un cálculo — sin acumular totales manualmente, ver `CalculoEmpleadoDia`)
  - `CalculoEmpleadoDia` (el detalle día a día: fecha, tipo de jornada — Trabajado / Libre / Ausencia / Licencia médica / Vacaciones / Permiso con goce / Permiso sin goce —, horas extra del día). Los totales por persona (días trabajados, ausencias, horas extra, etc.) se **calculan a partir de esta tabla**, no se guardan por separado, para evitar que un total quede desincronizado del detalle diario.
  - `AuditoriaEjecucion` (fecha, tipo: Automática/Manual, mes auditado, contratos revisados)
  - `AuditoriaInconsistencia` (ejecución asociada, cálculo/empleado afectado, tipo de inconsistencia, detalle, estado: Pendiente/Resuelta)
- Cliente de integración con la API de Talana (servicio único, reutilizable), con métodos separados para: sincronizar empleados, consultar licencias/vacaciones, enviar cálculo.
- Job/tarea programada (para la Fase 4, pero la infraestructura de scheduling se deja lista aquí — ej. `IHostedService` o similar en .NET).
- Migraciones EF Core aplicadas sobre la base ya existente.

**Listo cuando:** existen las tablas en la base y hay un servicio de prueba que se conecta a la API de Talana (aunque sea contra un ambiente de pruebas) y trae una respuesta.

---

## Fase 1 — Módulo Base Talana

**Objetivo:** reflejar en la base de datos propia a los empleados cargados en Talana.

**Incluye:**
- Sincronización con Talana: trae el listado de empleados activos (y su estado) y los guarda/actualiza en `EmpleadoTalana`. Botón "Sincronizar con Talana" para forzar una sincronización manual, además de que corra periódicamente.
- Aviso fijo sobre el listado con la fecha/hora de la última sincronización (ej. "Última sincronización con Talana: 08-08-2026 07:00"), para que quede claro qué tan al día está la base sin tener que sincronizar para averiguarlo.
- Listado general de Base Talana (búsqueda y orden por columna): nombre, RUT, cliente, contrato, cargo, estado en Talana.
- Este módulo es de **solo lectura** desde el punto de vista del usuario — los datos vienen de Talana, no se crean ni editan manualmente acá.

**Listo cuando:** al sincronizar, la tabla `EmpleadoTalana` queda actualizada con los datos vigentes en Talana y el listado los muestra correctamente.

---

## Fase 2 — Módulo Contratos

**Objetivo:** CRUD de contratos de servicio, base para asociar dotación y cálculos.

**Incluye:**
- Listado general de contratos (búsqueda y orden por columna): contrato, cliente, tipo, dotación (cantidad de empleados de Base Talana asociados), estado.
- Crear / editar contrato: cliente asociado (de la lista compartida en Admin), nombre del contrato/servicio, tipo (Outsourcing / Servicios Transitorios / Selección y Reclutamiento), fecha de inicio, fecha de término.
- Ficha de contrato: datos generales, dotación asociada (empleados de Base Talana que pertenecen a este contrato) e historial de cálculos (periodos pasados y su estado).
- Un contrato no se borra: se marca como Terminado (baja lógica), para no perder el historial de cálculos asociados.

**Listo cuando:** se puede crear/editar un contrato, y su ficha muestra correctamente la dotación (desde Base Talana) y el historial de cálculos.

---

## Fase 3 — Módulo Cálculo

**Objetivo:** generar y gestionar, por cliente/contrato/mes, la plantilla de revisión que se envía a Talana.

**Incluye:**
- Crear un cálculo: se elige contrato y periodo (mes). Al crearlo, el sistema **genera automáticamente la plantilla** con la dotación vigente de ese contrato — **empleados de Base Talana asociados al contrato cuyo estado en Talana sea Activo**; un empleado que Talana marca Inactivo queda fuera de la plantilla aunque siga figurando en el contrato. La plantilla arma una fila por persona, y dentro de cada persona, un día por cada día del mes (por defecto: Trabajado en días hábiles, Libre en fines de semana — ajustable según feriados más adelante).
- La plantilla tiene **dos niveles de vista**, pensados para no saturar la pantalla con 30+ días × 7 categorías por persona:
  1. **Totales por persona** (vista por defecto): tabla ordenable/buscable con los totales ya calculados — días trabajados, ausencias, licencias, vacaciones, permisos con/sin goce, horas extra. Las columnas de licencias y vacaciones se etiquetan explícitamente "(Talana)" para dejar claro que ese dato viene de la API, no de lo cargado por el cliente.
  2. **Detalle diario** (al abrir una persona): una grilla tipo calendario del mes, con un día por celda. Cada celda tiene un selector de tipo de jornada (coloreado según el tipo, para revisar de un vistazo) y, solo si el día es "Trabajado", un campo de horas extra. Editable mientras el cálculo esté Abierto; de solo lectura en Enviado/Cerrado.
- Origen de los datos, combinado en la misma grilla diaria:
  - **Datos que entrega el cliente**: asistencia/turnos, ausencias, permisos, horas extra — editables directamente en la grilla, o cargados desde Excel (ver punto siguiente).
  - **Datos que entrega Talana**: licencias médicas, vacaciones — se obtienen vía API y se reflejan como el tipo de jornada correspondiente en el día que aplica.
- **Carga de asistencia desde Excel**: los clientes entregan esta información en Excel, pero en **formatos distintos según el sistema que usan**. Formatos identificados hoy:
  - **Formato A — Planilla estándar SOS**
  - **Formato B — Sistema de turnos**
  - **Formato C — Control de asistencia biométrico**
  
  El módulo debe permitir elegir el formato de origen antes de cargar el archivo, y mapear cada formato a la estructura interna (persona → día → tipo de jornada / horas extra). Cada formato nuevo que aparezca a futuro debería poder agregarse como un mapeo adicional, sin rehacer el resto del módulo.
- La plantilla debe ser revisable por Supervisores y Ejecutivos antes del envío (de ahí que el módulo Cálculo, a diferencia del resto de Remuneraciones, sea visible también para el perfil Ejecutivo).
- Estados del cálculo:
  - **Abierto**: en edición/revisión, incluida la grilla diaria y la carga desde Excel.
  - **Enviado**: se envió la información a Talana vía su API. Queda registrada la fecha de envío. Ya no editable.
  - **Cerrado**: cálculo cerrado, ya no editable desde este módulo (cualquier ajuste posterior pasa por Auditoría). El cierre **no es automático**: es una acción manual del usuario ("Marcar como cerrado") sobre un cálculo ya Enviado — la auditoría solo lee cálculos Cerrados, no los cierra ella misma.
- Toda la información del cálculo (encabezado, totales y detalle diario por persona) queda **grabada en base de datos**, no solo enviada a Talana — es el registro histórico propio de la empresa.
- Listado general de cálculos (búsqueda y orden): cliente, contrato, periodo, estado, cantidad de empleados.

**Listo cuando:** se puede crear un cálculo (con su plantilla autogenerada), cargar asistencia del cliente desde al menos uno de los tres formatos de Excel, revisar/editar el detalle diario de cualquier persona, ver los totales recalculados automáticamente, ver los datos que entrega Talana, enviarlo a Talana (pasa a Enviado) y marcarlo manualmente como cerrado (pasa a Cerrado), quedando todo grabado en la base de datos.

> El formato de la plantilla (totales + detalle diario tipo calendario, editable) y el panel de carga con selector de formato de Excel ya están maquetados — ver la maqueta enlazada abajo, módulo Remuneración → Cálculo → abrir cualquier cálculo → "Ver detalle diario".

---

## Fase 4 — Módulo Auditoría

**Objetivo:** detectar inconsistencias entre lo que se cerró en Cálculo y lo que realmente hay registrado en Talana.

**Incluye:**
- Proceso de auditoría: toma los cálculos con estado Cerrado del mes abierto, y cruza su información contra la que entrega la API de Talana para ese mismo periodo.
- Si detecta diferencias (ej. vacaciones no coinciden, licencia médica no reflejada, horas extra fuera de rango), genera un registro de **inconsistencia** con el detalle, asociado al cálculo/empleado afectado.
- Ejecución:
  - **Automática**: todas las noches, durante los **últimos 7 días hábiles del mes abierto** (requiere definir qué calendario de días hábiles se usa).
  - **Manual**: cualquier usuario con perfil Administrador puede ejecutarla a demanda ("Ejecutar auditoría ahora").
- Las inconsistencias encontradas deben quedar visibles en el **Panel de control central** (Fase 5), no solo dentro de este módulo.
- Listado de inconsistencias (búsqueda y orden): fecha de detección, cliente, contrato, empleado, tipo de inconsistencia, estado (Pendiente / Resuelta).

**Listo cuando:** existe un job que corre la auditoría automáticamente en la ventana de días hábiles definida, un botón que la ejecuta manualmente, y las inconsistencias quedan registradas y visibles.

---

## Fase 5 — Panel de control central

**Objetivo:** vista única del estado de avance de todos los contratos activos en el mes, y de las inconsistencias detectadas.

**Incluye:**
- Indicadores generales, como tarjetas separadas: contratos activos, cálculos abiertos, cálculos enviados, cálculos cerrados (los cuatro del mes en curso) e inconsistencias pendientes.
- Tabla de avance del cálculo por contrato activo del mes (cliente, contrato, estado del cálculo, cantidad de empleados), con acceso directo a cada cálculo.
- Tabla de inconsistencias detectadas por auditoría, con acceso directo al cálculo afectado.
- Es la pantalla de entrada de la app para Administrador y Supervisor.

**Listo cuando:** el panel refleja en tiempo real el estado real de los cálculos del mes y las inconsistencias pendientes, sin tener que entrar módulo por módulo.

---

## Modelo de permisos

| Módulo | Administrador | Supervisor | Ejecutivo |
|---|---|---|---|
| Panel de control | ✅ | ✅ | — |
| Base Talana | ✅ | ✅ | — |
| Contratos | ✅ | ✅ | — |
| Cálculo | ✅ | ✅ | ✅ (revisión operativa) |
| Auditoría | ✅ | — | — |

Este modelo de permisos se gestiona desde **Admin → Accesos por perfil** (ver [admin.md](admin.md)), no está fijo en el código de cada módulo.

## Resumen de dependencias

| Módulo | Depende de |
|---|---|
| Base Talana | Fase 0, API de Talana |
| Contratos | Fase 0, Base Talana, Clientes (Admin) |
| Cálculo | Contratos, Base Talana, API de Talana |
| Auditoría | Cálculo (cerrados), API de Talana |
| Panel de control | Contratos, Cálculo, Auditoría |

## Referencia

Look and feel, navegación y el flujo completo maquetado (incluyendo la ficha de cálculo, el formulario de nuevo contrato/cálculo y la matriz de accesos) están en la maqueta — ver [sosgroup.md](sosgroup.md).
