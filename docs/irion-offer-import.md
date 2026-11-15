# Guardar una oferta de Irion en un producto existente

Guía para `Administrator` y `SuperAdmin`. La importación es manual: visualizar y revisar no guardan; **Guardar oferta de Irion** es una acción separada. No crea productos ni tiendas, rastrea catálogos, programa consultas, mantiene historial de precios ni descarga imágenes en el backend.

## Antes de arrancar: el bootstrap escribe en la base

**No inicies la API para ejecutar las comprobaciones de esta guía.** En Development, [Program.cs](../src/Web/WebAPI/Program.cs) llama a `InitialiseDatabaseAsync()`. En [ApplicationDbContextInitialiser.cs](../src/Infrastructure/Data/ApplicationDbContextInitialiser.cs), esa extensión ejecuta `InitialiseAsync()` y luego `SeedAsync()`: primero `Database.MigrateAsync()` aplica migraciones pendientes; después las semillas pueden insertar roles, usuario administrativo y datos de demostración, con `SaveChangesAsync()`. Los errores se registran y se propagan; no hay una aprobación interactiva previa a la migración.

La migración [20260720000000_IrionOffer](../src/Infrastructure/Data/Migrations/20260720000000_IrionOffer.cs) está **preparada, no aplicada en esta entrega**. Antes de cualquier aplicación, una persona debe revisar y autorizar un destino **SQL Server aislado**, su respaldo, el SQL y los pares `(StoreId, ProductId)` duplicados, incluidos inventarios inactivos. La migración detecta duplicados y aborta antes de alterar el esquema: no borra, fusiona ni adopta filas automáticamente. Resolverlos requiere una decisión explícita fuera de este flujo.

No uses el arranque como atajo para aplicar o verificar la migración. No se solicitan ni deben pegarse credenciales. Esta guía no cambia el bootstrap ni autoriza acceso a una base real.

## Flujo en la pantalla

Solo después de la preparación operativa autorizada, abre **Administración → Vista previa de Irion** (`/maintainer/irion-preview`):

1. Pega una URL de producto de Irion y pulsa **Visualizar producto**. Revisa variantes, precios y disponibilidad; la descripción HTML se muestra como texto. Visualizar no escribe ofertas.
2. Selecciona un **producto activo existente**, una **tienda activa de Irion** y una **variante explícita**, incluso si hay una sola. Ninguno se elige por defecto. La tienda se valida por su URL raíz HTTPS de `irion.cl` (puerto predeterminado), no por su nombre. Si faltan registros o falla la carga, revisa el catálogo o pulsa **Recargar listas**; no se crean automáticamente.
3. Pulsa **Revisar vinculación**. Esta inspección no consulta Irion ni escribe:

   | Resultado | Qué revisar |
   |-----------|-------------|
   | `Create` | Se creará la única oferta de esa pareja producto/tienda. |
   | `Update` | Coinciden URL canónica y variante con la vinculación existente; se conserva ID y activación administrativa. |
   | `Conflict` | Hay una oferta manual sin vinculación o una identidad diferente; el guardado queda bloqueado, sin adopción ni sobrescritura. |

4. Revisa el resumen de la variante, no el precio ni la disponibilidad global del producto. Pulsa **Guardar oferta de Irion** por separado. El servidor vuelve a consultar Irion una vez y valida identidad, selección, precios, disponibilidad y versión antes de guardar. No acepta precios editados en el navegador; solo CLP enteros positivos dentro de Int32. Original ausente se guarda como `0` y el descuento se calcula en servidor.

Cambiar selección o URL, recargar listas o volver a visualizar invalida la revisión. Durante las operaciones se bloquean controles incompatibles y envíos repetidos. **Cancelar borrador** limpia la selección sin escribir; no revierte una oferta ya guardada.

### Resultado del guardado

| Resultado | Siguiente acción |
|-----------|------------------|
| `Saved` | La oferta quedó guardada. |
| `AlreadySaved` | La misma identidad y valores ya estaban guardados: no crea duplicado, no escribe otra vez ni cambia la fecha de observación. |
| `RefreshRequired` | No se guardó: cambió la instantánea o desapareció la variante. La pantalla usa la vista actualizada si está disponible y borra la variante seleccionada; selecciona explícitamente de nuevo, revisa la vinculación y confirma otra vez. |
| `Conflict` | Cambió la vinculación o versión, o hubo un conflicto de persistencia: vuelve a revisar; nunca se sobrescribe ni se reintenta automáticamente. |

Ante 429, timeout u otro fallo de extracción no se guarda. Respeta la espera indicada para 429; no hay reintentos automáticos. Si se pierde la respuesta al guardado, no supongas éxito ni fallo: conserva la selección, vuelve a revisar y confirma manualmente. La confirmación idéntica puede responder `AlreadySaved`.

## Datos e identidad

- `SourceUrl` conserva la URL canónica `/products/{handle}`, sin seguimiento; `ExternalVariantId` identifica la variante elegida. Una oferta importada no se puede reasociar silenciosamente a otro producto, tienda, URL o variante.
- `Availability` refleja la disponibilidad externa de **esa variante**, no cantidad de stock ni garantía de compra. `ObservedAtUtc` es la fecha UTC del servidor al guardar. Ambos son independientes de `IsActive`, la habilitación administrativa: actualizar una oferta deshabilitada no la reactiva.
- Los registros antiguos conservan metadatos nulos, sin inventar origen ni fecha. El comparador excluye inventarios, tiendas o productos inactivos y `Availability == false`; mantiene el comportamiento de disponibilidad desconocida (`null`).
- La pareja tienda/producto tiene índice único, incluso para filas manuales o inactivas. `RowVersion` de SQL Server protege actualizaciones optimistas; una carrera se trata como conflicto recuperable, no como sobrescritura.
- Aviso de procedencia: una edición manual de precios en la misma pareja conserva identidad y metadatos de la última observación. Esa fecha no prueba que el precio editado proceda de Irion; no hay historial de modificaciones en este alcance.

Los cuerpos y resultados de los endpoints `inspect` y `confirm` están en el [README](../README.md#inspección-de-una-oferta-de-irion-solo-lectura). Usa resultados tipados, no interpretes mensajes para decidir el flujo.

## Comprobaciones deterministas sin API ni SQL Server

Desde la raíz del repositorio, con SDK .NET y dependencias **ya disponibles**, sin restaurar ni instalar paquetes:

```bash
dotnet run --project tests/IrionOfferImport.Checks/IrionOfferImport.Checks.csproj --no-restore
dotnet run --project tests/IrionProductExtractor.Checks/IrionProductExtractor.Checks.csproj --no-restore
(cd src/Web/WebUI && pnpm test:run)
dotnet build src/Web/WebAPI/WebAPI.csproj --no-restore
(cd src/Web/WebUI && pnpm build)
```

Usa pnpm **12.18.0**, las dependencias instaladas y el HOME habitual del proyecto; no un HOME limpio que pueda aprovisionar herramientas. Si faltan dependencias, detente: estos pasos no autorizan instalaciones. Los ejecutables de consola no son `dotnet test`: usan transporte falso, orquestación simulada y generación estructural de SQL/modelo, sin abrir conexión. La suite Angular usa Vitest/jsdom. Los builds compilan, no arrancan la API. No ejecutes `dotnet run` sobre WebAPI, `pnpm start`, herramientas EF ni consultas a Irion o a la base para estos checks.

Resultados locales de referencia: **602** aserciones de ofertas, **112** del extractor y **39** pruebas frontend en **4** archivos; ambos builds con salida 0. El build frontend advierte sobre presupuestos de bundle inicial y SCSS de registro; no son prueba de funcionamiento en navegador real.

## Evidencia pendiente: no es aprobación de producción

- **T1–T3:** aceptados localmente para su alcance determinista/estructural.
- **T4:** implementación y pruebas locales aprobadas; revisión independiente **no disponible por cuota**, sin veredicto ni aceptación. Las comprobaciones integradas normales no la sustituyen.
- **T6:** bloqueado hasta disponer de SQL Server aislado autorizado. Falta ejecutar migración y rollback, comprobar conservación de filas y rechazo de duplicados, persistencia real de unicidad/rowversion y carreras de importación. La generación de SQL y los dobles de prueba no demuestran esas propiedades relacionales.

No hay prueba real de base de datos, aprobación de toda la funcionalidad, autorización de migración ni declaración de preparación para producción. La revisión independiente pendiente y las pruebas relacionales deben resolverse antes de cerrar la entrega.
