# Guardar una oferta de Irion con producto existente o nuevo

Guía para `Administrator` y `SuperAdmin`. La importación es manual: visualizar y revisar no guardan; **Guardar oferta de Irion** conserva el flujo de producto existente. Para uno nuevo, **Crear producto y guardar oferta** confirma explícitamente ambas entidades. No crea tiendas ni referencias de catálogo, rastrea catálogos, programa consultas, mantiene historial de precios ni descarga imágenes en el backend.

## Antes de arrancar: el bootstrap escribe en la base

**No inicies la API para ejecutar las comprobaciones de esta guía.** En Development, [Program.cs](../src/Web/WebAPI/Program.cs) llama a `InitialiseDatabaseAsync()`. En [ApplicationDbContextInitialiser.cs](../src/Infrastructure/Data/ApplicationDbContextInitialiser.cs), esa extensión ejecuta `InitialiseAsync()` y luego `SeedAsync()`: primero `Database.MigrateAsync()` aplica migraciones pendientes; después las semillas pueden insertar roles, usuario administrativo y datos de demostración, con `SaveChangesAsync()`. Los errores se registran y se propagan; no hay una aprobación interactiva previa a la migración.

La migración [20260720000000_IrionOffer](../src/Infrastructure/Data/Migrations/20260720000000_IrionOffer.cs) está **preparada, no aplicada en esta entrega**. Antes de cualquier aplicación, una persona debe revisar y autorizar un destino **SQL Server aislado**, su respaldo, el SQL y los pares `(StoreId, ProductId)` duplicados, incluidos inventarios inactivos. La migración detecta duplicados y aborta antes de alterar el esquema: no borra, fusiona ni adopta filas automáticamente. Resolverlos requiere una decisión explícita fuera de este flujo.

No uses el arranque como atajo para aplicar o verificar la migración. No se solicitan ni deben pegarse credenciales. Esta guía no cambia el bootstrap ni autoriza acceso a una base real.

## Flujo en la pantalla

Solo después de la preparación operativa autorizada, abre **Administración → Vista previa de Irion** (`/maintainer/irion-preview`):

1. Pega una URL de producto de Irion y pulsa **Visualizar producto**. Revisa variantes, precios y disponibilidad; la descripción HTML se muestra como texto. Visualizar no escribe ofertas.
2. Selecciona un **producto activo existente**, una **tienda activa de Irion** y una **variante explícita**, incluso si hay una sola. Ninguno se elige por defecto. La tienda se valida por su URL raíz HTTPS de `irion.cl` (puerto predeterminado), no por su nombre. Si faltan registros o falla la carga, revisa el catálogo o pulsa **Recargar listas**; no se crean automáticamente.
   Si el producto no existe, elige **Crear producto nuevo**. Solo se propone el título como texto editable: completa fabricante, franquicia, serie opcional de esa franquicia, base **Sí/No**, edad, tamaño, descripción como texto, fecha de lanzamiento y al menos una categoría activa. No hay fecha, base ni referencias predeterminadas. Las opciones activas se cargan al elegir este modo; **Recargar opciones** permite recuperarse de fallos sin bloquear la consulta o el modo existente. Las imágenes son opcionales y se pueden agregar después; no se importan imágenes externas ni cambia el formulario manual.
3. Pulsa **Revisar vinculación**. Esta inspección no consulta Irion ni escribe:

   | Resultado | Qué revisar |
   |-----------|-------------|
   | `Create` | Se creará la única oferta de esa pareja producto/tienda y, en modo nuevo, el producto con sus categorías. |
   | `Update` | Coinciden URL canónica y variante con la vinculación existente; se conserva ID y activación administrativa. En modo nuevo también deben coincidir todos los metadatos y categorías del UUID estable. |
   | `Conflict` | Hay una oferta manual sin vinculación, una identidad diferente o, en modo nuevo, ID/nombre ocupado incompatible; el guardado queda bloqueado, sin adopción ni sobrescritura. |

4. Revisa el resumen de la variante, no el precio ni la disponibilidad global del producto. Pulsa **Guardar oferta de Irion** por separado, o **Crear producto y guardar oferta** en modo nuevo. Ese botón envía una sola llamada a la API de confirmación, no dos guardados independientes. El servidor vuelve a consultar Irion una vez y valida identidad, selección, precios, disponibilidad y versión antes de guardar. Para el producto nuevo prepara `Product`, `ProductCategories` e `Inventory` y llama una sola vez a `SaveChangesAsync`; la atomicidad relacional prevista aún requiere prueba SQL Server en T6. No acepta precios editados en el navegador; solo CLP enteros positivos dentro de Int32. Original ausente se guarda como `0` y el descuento se calcula en servidor.

Cambiar selección o URL, recargar listas o volver a visualizar invalida la revisión. Durante las operaciones se bloquean controles incompatibles y envíos repetidos. **Cancelar borrador** limpia la selección sin escribir; no revierte una oferta ya guardada.

### Resultado del guardado

| Resultado | Siguiente acción |
|-----------|------------------|
| `Saved` | La oferta quedó guardada. |
| `AlreadySaved` | La misma identidad y valores ya estaban guardados: no crea duplicado, no escribe otra vez ni cambia la fecha de observación. |
| `RefreshRequired` | No se guardó: cambió la instantánea o desapareció la variante. La pantalla usa la vista actualizada si está disponible y borra la variante seleccionada; selecciona explícitamente de nuevo, revisa la vinculación y confirma otra vez. |
| `Conflict` | Cambió la vinculación o versión, o hubo un conflicto de persistencia: vuelve a revisar; nunca se sobrescribe ni se reintenta automáticamente. |

Ante 429, timeout u otro fallo de extracción no se guarda. Respeta la espera indicada para 429; no hay reintentos automáticos. Si se pierde la respuesta al guardado, no supongas éxito ni fallo: conserva la selección, vuelve a revisar y confirma manualmente. La confirmación idéntica puede responder `AlreadySaved`. En modo nuevo se conserva el UUID del borrador y todos los datos humanos durante errores, cambios de modo y revisión/reintento manual; no se genera otro UUID por edición o reintento. **Cancelar borrador** descarta esos datos y permite comenzar otro UUID, sin revertir guardados confirmados. Una repetición en modo nuevo exige el mismo UUID lógico, todos los metadatos y el conjunto de categorías, además de la identidad tienda/URL/variante y los valores de la oferta; un título coincidente no permite adoptar otro producto. La restricción única existente de `ProductName` sigue siendo autoridad: un nombre ocupado exige **usar existente o revisar nombre**.

## Datos e identidad

- `SourceUrl` conserva la URL canónica `/products/{handle}`, sin seguimiento; `ExternalVariantId` identifica la variante elegida. Una oferta importada no se puede reasociar silenciosamente a otro producto, tienda, URL o variante.
- `Availability` refleja la disponibilidad externa de **esa variante**, no cantidad de stock ni garantía de compra. `ObservedAtUtc` es la fecha UTC del servidor al guardar. Ambos son independientes de `IsActive`, la habilitación administrativa: actualizar una oferta deshabilitada no la reactiva.
- Los registros antiguos conservan metadatos nulos, sin inventar origen ni fecha. El comparador excluye inventarios, tiendas o productos inactivos y `Availability == false`; mantiene el comportamiento de disponibilidad desconocida (`null`).
- La pareja tienda/producto tiene índice único, incluso para filas manuales o inactivas. `RowVersion` de SQL Server protege actualizaciones optimistas; una carrera se trata como conflicto recuperable, no como sobrescritura.
- Aviso de procedencia: una edición manual de precios en la misma pareja conserva identidad y metadatos de la última observación. Esa fecha no prueba que el precio editado proceda de Irion; no hay historial de modificaciones en este alcance.

Los cuerpos y resultados de los endpoints están en el README: [inspección existente](../README.md#inspección-de-una-oferta-de-irion-solo-lectura), [preparación de producto nuevo](../README.md#crear-un-producto-nuevo-y-guardar-su-oferta-de-irion) y [confirmación nueva](../README.md#confirmar-producto-nuevo-y-oferta-api-administrativa). Usa resultados tipados, no interpretes mensajes para decidir el flujo.

## Comprobaciones deterministas sin API ni SQL Server

Desde la raíz del repositorio, con SDK .NET y dependencias **ya disponibles**, sin restaurar ni instalar paquetes:

```bash
timeout 180s dotnet run --project tests/IrionOfferImport.Checks/IrionOfferImport.Checks.csproj --no-restore
timeout 180s dotnet run --project tests/IrionProductExtractor.Checks/IrionProductExtractor.Checks.csproj --no-restore
(cd src/Web/WebUI && timeout 180s pnpm test:run)
timeout 180s dotnet build src/Web/WebAPI/WebAPI.csproj --no-restore
(cd src/Web/WebUI && timeout 180s pnpm build)
```

Usa pnpm **12.18.0**, las dependencias instaladas y el HOME habitual del proyecto; no un HOME limpio que pueda aprovisionar herramientas. Si faltan dependencias, detente: estos pasos no autorizan instalaciones. Los ejecutables de consola no son `dotnet test`: usan transporte falso, orquestación simulada y generación estructural de SQL/modelo, sin abrir conexión. La suite Angular usa Vitest/jsdom. Los builds compilan, no arrancan la API. No ejecutes `dotnet run` sobre WebAPI, `pnpm start`, herramientas EF ni consultas a Irion o a la base para estos checks.

T5 ejecutó una sola secuencia integrada del estado actual, en primer plano y con límite de 180 segundos por comando: **980** aserciones de ofertas, **112** del extractor y **62** pruebas frontend en **5** archivos; los cinco comandos finalizaron con salida 0. El resultado enfocado previo de T4 fue **56** pruebas en **3** archivos, no repetido por separado aquí. Los anteriores 602/39 pertenecen al alcance histórico de producto existente, no son la prueba actual. El build API informó 0 advertencias y 0 errores; el frontend advirtió sobre el bundle inicial (625.68 kB frente a 500 kB) y SCSS de registro (2.26 kB frente a 2 kB). No son pruebas de navegador real.

Logs completos ignorados (evidencia local, no archivos para commit):

- `tests/IrionOfferImport.Checks/obj/t5-new-product-offer.log`
- `tests/IrionProductExtractor.Checks/obj/t5-new-product-extractor.log`
- `src/Web/WebUI/.angular/t5-new-product-frontend-tests.log`
- `src/Web/WebAPI/obj/t5-new-product-api-build.log`
- `src/Web/WebUI/.angular/t5-new-product-frontend-build.log`

No se modificó código ni pruebas, ni se arrancó la API o aplicó una migración. Ante fallo o timeout, detener la secuencia y reportar: no corregir producción ni repetir checks automáticamente con esta autorización.

## Evidencia pendiente: no es aprobación de producción

- **T1/T3:** aceptación local determinista/estructural preservada.
- **T2 producto nuevo:** revisiones independientes locales de código aprobadas para inspección `0c2b28e` y confirmación `db83c19`; revisión de confirmación `mwiew189-w-9foz` sin defectos. No repitieron pruebas de ejecución.
- **T4 producto nuevo:** `85226c6`, revisión independiente local de código `mwifc7hy-y-6moj` aprobada sin defectos, sin repetir pruebas de ejecución. La recuperación de cuota del antiguo flujo existente es historia, no aprobación del alcance nuevo. Ninguna revisión es prueba de navegador, autenticación real ni SQL Server.
- **T5:** hito local de documentación e integración completado; código revisado sin cambios, sin nueva revisión independiente necesaria. Los presupuestos de corrección T2/T4 siguen agotados y aceptados, no se reiniciaron.
- **T6:** bloqueado hasta disponer de un destino SQL Server aislado accesible y autorizado. Faltan migración y rollback, conservación de datos y rechazo de duplicados sin borrarlos/fusionarlos, unicidad real de ID y nombre, rowversion, creación concurrente de producto nuevo y persistencia conjunta de producto/categorías/inventario sin huérfanos. La generación de SQL y los dobles de prueba no demuestran esas propiedades relacionales.

No hay prueba real de base de datos, aprobación de toda la funcionalidad, autorización de migración ni declaración de preparación para producción. No se intentaron verificaciones adicionales de SQL, autorización real o navegador. Las pruebas relacionales siguen siendo obligatorias antes de cerrar la entrega completa.
