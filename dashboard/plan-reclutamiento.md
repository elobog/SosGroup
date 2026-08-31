# Plan de desarrollo — App Reclutamiento

Plan por etapas para construir los dos módulos propios de Reclutamiento (Postulantes, Selección) sobre el proyecto Blazor Server ya existente (Identity + SQL Server). El módulo Clientes ya no vive acá: es un dato maestro compartido, administrado en Admin — ver [admin.md](admin.md).

## Supuestos y pendientes a confirmar antes de partir

- **Clientes**: Reclutamiento solo *referencia* la tabla `Cliente` (definida y gestionada en Admin). No hay CRUD de clientes en esta app.
- **API de antecedentes ("WHO")**: se usará un proveedor específico para obtener información legal/judicial de preseleccionados. Falta documentar: nombre exacto del proveedor, endpoint, formato de request/response, costo y credenciales de acceso. Se deja como punto de integración en la Fase 2; no bloquea el resto del desarrollo.
- **Extracción de datos de PDF (CV, certificado de antecedentes)**: se hará con la API de OpenAI/GPT. Falta definir: modelo a usar, si se sube el PDF directo o se extrae texto primero (ej. con una librería de lectura de PDF en .NET y se envía el texto a GPT), y dónde se aloja la clave de API (Azure OpenAI vs OpenAI directo, por tema de datos de candidatos).
- **Almacenamiento de archivos**: CVs, certificados y los `.txt` generados por IA necesitan un lugar de almacenamiento (disco del servidor, Azure Blob Storage, etc.). Se define en la Fase 0.
- Todo el desarrollo asume que se mantiene el stack actual: Blazor Server (.NET 10), ASP.NET Core Identity, EF Core + SQL Server.

## Orden sugerido de las fases

```
Fase 0 (base) → Fase 1 (Postulantes) → Fase 2 (Selección)
```

Postulantes va primero porque Selección lo necesita como prerrequisito (un proceso de selección incorpora postulantes desde la base general). Ambas fases asumen que el módulo Clientes de Admin (ver [admin.md](admin.md)) ya existe, al menos con datos de prueba, para poder asociar procesos a un cliente.

---

## Fase 0 — Base común

**Objetivo:** dejar la infraestructura y el modelo de datos que los dos módulos van a compartir, para no rehacerla en cada módulo.

**Incluye:**
- Modelo de datos inicial (tablas/entidades EF Core):
  - `Postulante` (header: nombre, RUT, contacto, etc.)
  - `PostulanteHistorial` (hoja de vida: experiencia, educación, eventos del postulante en el tiempo)
  - `PostulanteDocumento` (CV, certificado de antecedentes, otros — referencia al archivo + tipo + fecha)
  - `DocumentoExtraccionIA` (resultado de IA por documento: texto plano `.txt`, datos estructurados, estado de procesamiento)
  - `ProcesoSeleccion` (cliente — FK a `Cliente` de Admin —, cargo, plazo, descripción de cargo, renta fija, renta variable/comisiones, beneficios), `ProcesoSeleccionEtapa`, `ProcesoSeleccionPostulante` (relación N:N postulante–proceso, con etapa/estado)
- Servicio de almacenamiento de archivos (subir/leer/borrar), con una interfaz para poder cambiar de disco local a Blob Storage más adelante sin tocar el resto del código.
- Servicio de integración con IA (cliente único, reutilizable): recibe texto o archivo, devuelve texto/JSON. Se usa en Fase 1 (extracción) y Fase 2 (matching).
- Migraciones EF Core aplicadas sobre la base ya existente.

**Listo cuando:** existen las tablas en la base, se puede subir/descargar un archivo de prueba, y hay un endpoint/servicio de prueba que le pega al servicio de IA y devuelve una respuesta.

---

## Fase 1 — Módulo Postulantes

**Objetivo:** listado general de postulantes con su ficha (header + hoja de vida) y el pipeline de documentos con extracción por IA.

**Incluye:**
- Listado general de postulantes (búsqueda y orden por columna).
- Ficha de postulante:
  - Header: datos personales/contacto.
  - Historial / hoja de vida: experiencia laboral, educación, otros antecedentes — editable manualmente y también completable desde lo que extraiga la IA.
- Carga de documentos (CV, certificado de antecedentes, otros): el usuario sube el PDF.
- Pipeline de extracción con IA:
  1. Se sube el PDF → se guarda en el storage de la Fase 0.
  2. Se procesa con IA → se genera un `.txt` con la información extraída (queda guardado, para uso posterior por otras partes del sistema).
  3. Los datos relevantes (nombre, experiencia, educación, etc.) se usan para completar/sugerir campos en la ficha del postulante — el usuario revisa y confirma antes de guardar (no se debe sobrescribir automáticamente sin revisión).
- Estado visible del procesamiento por documento (pendiente / procesando / listo / con error), porque la llamada a IA no es instantánea.

**Listo cuando:** se puede crear un postulante, subirle un CV en PDF, ver que se generó el `.txt` de extracción, y revisar/aceptar los datos sugeridos en la ficha.

---

## Fase 2 — Módulo Selección

**Objetivo:** crear y gestionar procesos de selección asociados a un cliente, con búsqueda/ingesta de postulantes y apoyo de IA en la preselección.

**Incluye:**
- CRUD de proceso de selección: cliente asociado, plazo y etapas del proceso (configurables, ej. Postulación → Preselección → Entrevistas → Oferta → Cierre).
- Al crear un proceso, los **dos campos más relevantes** son:
  - **Descripción del cargo** (texto libre: responsabilidades, requisitos, contexto).
  - **Estructura de renta**, desglosada en tres campos: renta fija, renta variable/comisiones y beneficios (no un solo campo de texto libre).
- Asociar postulantes al proceso desde tres vías:
  - Búsqueda en la base general de postulantes (Fase 1).
  - Carga masiva desde Excel (mapeo de columnas a campos de postulante).
  - Carga desde PDF (usa el mismo pipeline de extracción de la Fase 1 para crear postulantes nuevos a partir de CVs recibidos por otra vía, ej. correo).
- Matching/preselección con IA: comparar el perfil del postulante contra la descripción de cargo y sugerir un orden o filtro de preseleccionados (el reclutador valida, no es una decisión automática).
- Integración con la API de antecedentes ("WHO") para los preseleccionados: consulta y muestra la información legal/judicial en la ficha del proceso (esta parte queda bloqueada hasta tener la documentación del proveedor — ver "Supuestos y pendientes").
- Vista de estado del proceso por etapa (cuántos postulantes hay en cada etapa).

**Listo cuando:** se puede crear un proceso para un cliente (con descripción de cargo y estructura de renta), cargar postulantes por las tres vías, correr una preselección con IA y ver el resultado, y (una vez resuelta la integración) consultar antecedentes de un preseleccionado.

> El formulario de creación de proceso (con descripción de cargo y renta desglosada) y el flujo de preselección ya están maquetados — ver la maqueta enlazada abajo, módulo Reclutamiento → Selección → "+ Nuevo proceso".

---

## Resumen de dependencias entre módulos

| Módulo | Depende de |
|---|---|
| Postulantes | Fase 0 |
| Selección | Fase 0, Postulantes (Fase 1), Clientes (Admin) |

## Referencia

Look and feel y navegación (header, sidebar, permisos por perfil) ya definidos en la maqueta — ver [sosgroup.md](sosgroup.md).
