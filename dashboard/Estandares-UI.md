# Estándares de UI — Portal SOS Group

> Convenciones de diseño de interfaz que aplican a **todas** las apps del portal (Reclutamiento, Remuneración, Admin), no a un módulo en particular. Se van registrando acá a medida que se definen, para no repetir la discusión en cada listado nuevo. Referencia visual viva: la maqueta clickeable — ver [reference_sos_group_maqueta.md] en memoria del proyecto.

## Listados (tablas)

**Ancho mínimo, máxima información visible.**
- La tabla nunca se estira más de lo que su contenido necesita (no se fuerza a ocupar el 100% del panel). Si el contenido es angosto, la tabla queda angosta.
- Si el contenido no entra en el ancho disponible, la tabla no comprime columnas ni envuelve texto en varias líneas por defecto — crece más allá del panel y aparece scroll horizontal solo en ese contenedor (el resto de la página no se mueve).
- Excepción: columnas de texto libre largo (observaciones, descripciones) sí pueden envolver texto en vez de forzar una tabla enorme — se marcan explícitamente para eso, columna por columna.

**Sin columna de acciones ni botones de texto.**
- No se agrega una columna aparte "Acciones" con botones tipo "Ver ficha" / "Abrir". Eso gasta una columna completa y ensancha la fila sin aportar información.
- En vez de eso, el campo que identifica la fila (código, nombre, periodo — lo que el usuario usaría para reconocer el registro) **es directamente el link** que lleva al detalle. Un solo elemento hace las dos cosas: identifica y navega.
- Acciones que no son "ver detalle" (reasignar, editar en línea, etc.) no se agregan como decoración si todavía no están conectadas a algo real — se documentan como pendientes en vez de dejar un botón que no hace nada.
- Caso especial: filas que expanden detalle en la misma tabla (acordeón) — el campo identificador es el que togglea la fila, con un indicador pequeño (chevron) pegado al texto, no una columna aparte.

**Color como señal de estado, no decoración.**
- El texto del link se pinta del mismo color que ya usa el pill de "Estado" de esa fila cuando ese estado es el positivo/vigente del ciclo de vida de la entidad (`ok` → verde). No se inventa un color nuevo por tabla.
- Qué cuenta como "positivo" depende de la entidad, no es siempre "Activo": para Solicitud es "Activa", para Cliente/Contrato/Perfil de Cargo es "Activo", para Cálculo (Remuneración) es "Cerrado" — porque ese ya es el mapeo de color que define `calculoEstadoPill`. La regla es: **reusar el color que la entidad ya tiene definido para su propio pill de estado**, nunca definir uno aparte solo para el link.
- Si la entidad no tiene un concepto de estado (ej. un Perfil de rol en Admin > Perfiles), el link no lleva color — queda con el color de texto normal.

**Link vs. botón — que se note a simple vista cuál es cuál, sin pasar el mouse.**
- Todo texto que navega a una ficha (código, nombre, periodo — el patrón de arriba) lleva **subrayado permanente** (`.row-code`), no solo al pasar el mouse. Aplica también a los links sueltos fuera de tablas (`.link-btn`: "+ Nuevo perfil de cargo", "¿Olvidaste tu contraseña?", etc.) — mismo criterio en toda la maqueta.
- Cuando el campo identificador todavía no existe y la acción es **crear/iniciar algo nuevo** (ej. "Iniciar" en la columna Apertura cuando la Solicitud no tiene apertura todavía), no se ve como link — se ve como **botón chico** (`.row-action-btn`, fondo rojo sólido). Esto distingue "navegar a algo que ya existe" de "arrancar algo que no existe todavía", que son acciones de naturaleza distinta aunque vivan en la misma columna.
- En código: `codeLink(texto, onclick, activo)` da el link subrayado (con color si `activo`); `codeLink(texto, onclick, activo, "button")` da el botón chico. Un solo helper, un cuarto parámetro opcional — no crear una función nueva por caso.

## Excepción: tableros de seguimiento multi-etapa

- La regla "sin columna de acciones ni íconos" asume que cada fila tiene **una sola** acción relevante (ver el detalle). Cuando una fila representa un proceso con **varias dimensiones de estado independientes que hay que monitorear de un vistazo** (ej. Preselección: Correo / Certificados / Test / Entrevista / Antecedentes WHO, cada uno con su propio pendiente/completado), sí se permite una columna por dimensión — no es un "ver más" genérico, cada columna informa algo distinto.
- Cada celda es un ícono de estado clickeable (`iconEstado(estado, onclick, label)`): tilde verde si está completado, círculo si está pendiente. El click abre el módulo correspondiente o repite la acción (ej. reenviar el correo).
- El nombre/identificador de la fila (ej. el postulante) sigue siendo el único link a la ficha completa — la excepción es solo para las columnas de estado, no reemplaza el patrón de link-por-fila.
- No usar esta excepción por default: solo aplica cuando de verdad hay varias dimensiones de estado que un Reclutador necesita monitorear en paralelo sin entrar a cada ficha. Para una sola acción, seguir usando `codeLink`.

## Por qué

Esto surgió después de probar dos alternativas más pesadas (botones de texto, luego íconos en columna aparte) que terminaban ensanchando las listas sin agregar información. El objetivo es que una lista se pueda escanear rápido: el ojo va al color para saber el estado, y clickear el nombre/código es la única acción que hace falta aprender.
