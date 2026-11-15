# BuscaTuHobby

**BuscaTuHobby** es una aplicación que te permite buscar y comparar precios de Gunplas y maquetas de distintas tiendas en línea. Esta aplicación utiliza **.NET** para el backend y **Angular** para el frontend, y sigue la estructura propuesta por **Jason Taylor** en su Clean Architecture (https://github.com/jasontaylordev/CleanArchitecture).

## Funcionalidades Principales (En Proceso)

- **Búsqueda de Productos:** Encuentra tus Gunplas y maquetas favoritas utilizando palabras clave y filtros. :construction_worker:
- **Comparación de Precios:** Compara los precios de un producto en diferentes tiendas en línea. :soon:
- **Detalles del Producto:** Obtén información detallada sobre cada producto, incluyendo descripción, reseñas y especificaciones. :construction_worker:
- **Gestión de Favoritos:** Guarda tus productos favoritos para acceder a ellos fácilmente más tarde. :soon:
- **Autenticación y Perfiles:** Crea una cuenta, inicia sesión y personaliza tu perfil. :construction_worker:
- **Comentarios y Valoraciones:** Comparte tus opiniones sobre los productos y lee las de otros usuarios. :soon:

## Configuración del Proyecto

> **Antes de arrancar la API:** en Development, [Program.cs](src/Web/WebAPI/Program.cs) llama a `InitialiseDatabaseAsync`; [ApplicationDbContextInitialiser.cs](src/Infrastructure/Data/ApplicationDbContextInitialiser.cs) ejecuta `Database.MigrateAsync()` y después semillas con escrituras. Arrancar puede aplicar automáticamente migraciones pendientes, incluida `20260720000000_IrionOffer`, aunque solo quieras visualizar. Esa migración está preparada, no aplicada en esta entrega. No arranques contra una base real: primero una persona debe revisar y autorizar un destino SQL Server aislado, su respaldo, los pares duplicados y la migración. No hay deduplicación automática. Consulta la [guía operativa y comprobaciones sin API ni base de datos](docs/irion-offer-import.md).

Asegúrate de tener instaladas las siguientes herramientas antes de comenzar:

- .NET Core 10: https://dotnet.microsoft.com/download
- Node.js y npm: https://nodejs.org/
- Angular CLI: https://angular.io/cli

### Configuración del Proyecto 

1. Clona este repositorio: `git clone https://github.com/dcaceresc/BuscaTuHobby.git` 
2. Navega al directorio del proyecto y abre una terminal.
3. Ejecuta `dotnet restore` para restaurar las dependencias del proyecto.
4. Diriguete al directorio `/src/Web/WebUI`
5. Ejecuta `npm install` para instalar los paquetes de node
6. Regresa al directorio `/src/Web` y ejecuta `dotnet run`


## Vista previa de productos de Irion

[Guía de importación manual de ofertas](docs/irion-offer-import.md): flujo de pantalla, seguridad de arranque y comprobaciones deterministas. T1–T4 aceptados dentro de su alcance local y de revisión independiente de código; las comprobaciones integradas pasan. La revisión de T4 concluyó tras la recuperación de cuota autorizada. T6 SQL Server sigue bloqueado: no es una entrega completamente verificada ni lista para producción.

Desde **Administración → Vista previa de Irion** (`/maintainer/irion-preview`), pega la URL de un producto de Irion y selecciona **Visualizar producto**. Disponible para `Administrator` y `SuperAdmin`.

La pantalla muestra nombre, marca, imágenes, etiquetas, disponibilidad y precios por variante. La descripción original se muestra como texto, sin ejecutar HTML. Los datos ausentes se indican explícitamente y los errores permiten volver a consultar sin perder la URL.

La consulta extrae **un producto por URL**, sin crear productos ni modificar la base de datos. El navegador muestra las imágenes desde sus direcciones públicas; el backend no las descarga ni almacena. Guardar una oferta es una acción separada y explícita; no incluye rastreo del catálogo.

Con la API en ejecución y un token de `Administrator` o `SuperAdmin`:

```bash
curl -X POST 'https://localhost:7091/api/maintainer/products/preview/irion' \
  -H "Authorization: Bearer $TOKEN" \
  -H 'Content-Type: application/json' \
  --data '{"url":"https://irion.cl/products/transformers-classic-class-28-optimus-prime-energon-universe"}'
```

La respuesta usa `ApiResponse`: comprueba `success` antes de leer `data`. Incluye nombre, marca, etiquetas, descripción HTML **no sanitizada**, enlaces de imágenes, disponibilidad y variantes con SKU, código de barras y precios en CLP. No insertes `descriptionHtml` como HTML confiable. Las fechas de lanzamiento y la inclusión de base no se deducen ni se inventan.

- Admite `https://irion.cl/products/{handle}` y `https://irion.cl/collections/{collection}/products/{handle}`, con letras ASCII y números separados por guiones en ambos identificadores. Los parámetros de seguimiento y fragmentos se eliminan. Ambas rutas hacen una sola consulta a `https://irion.cl/products/{handle}.js`; la URL de origen devuelta no incluye `.js`.
- Obtiene el JSON público de Shopify; no sigue redirecciones. Límite de respuesta: 1 MiB; tiempo máximo de consulta: 15 segundos, además de la cancelación del cliente.
- Convierte los precios de Shopify dividiendo por 100. Si no hay precio original, `originalPrice` y `discountPercentage` son `null`. La disponibilidad no es una cantidad de stock ni una garantía de compra.
- Una URL no admitida devuelve `success: false`, `data: null` y `Introduce una URL válida de Irion: https://irion.cl/products/nombre-del-producto o https://irion.cl/collections/coleccion/products/nombre-del-producto`, sin consultar la tienda.
- Los demás errores de extracción devuelven `success: false` y el mensaje `No se pudo obtener la vista previa del producto de Irion.` dentro de una respuesta HTTP 200, siguiendo el patrón existente. La autenticación y autorización conservan sus respuestas HTTP habituales.
- Si Irion responde HTTP 429, devuelve `success: false`, `data: null` y `Irion está limitando las consultas (HTTP 429). Inténtalo más tarde.` (también dentro de HTTP 200). La pantalla muestra ese mensaje sin perder la URL. Si `Retry-After` contiene segundos válidos (hasta 2147483647), indica la espera mínima; si contiene una fecha futura válida, indica cuándo volver a consultar en UTC. Si falta, está mal formado, excede ese límite o contiene una fecha pasada, muestra el mensaje anterior. No se realizan reintentos automáticos ni consultas alternativas: espera lo indicado y pulsa de nuevo **Visualizar producto**. La espera no garantiza que la tienda haya levantado el límite. Admitir ambas rutas no elimina los límites impuestos por la tienda.
- No realiza consultas periódicas. Revisa las condiciones de reutilización de la tienda antes de publicar su catálogo y respeta sus límites de acceso.

Pruebas del frontend (desde `src/Web/WebUI/`):

```bash
pnpm exec ng test --watch=false --include='**/irion-preview/*.spec.ts'
pnpm test:run
pnpm build
```

Pruebas deterministas del backend, sin red ni base de datos (ejecutable de comprobación, no suite de `dotnet test`):

```bash
dotnet run --project tests/IrionProductExtractor.Checks/IrionProductExtractor.Checks.csproj --no-restore
```

## Inspección de una oferta de Irion (solo lectura)

`POST /api/maintainer/inventories/irion/inspect`, disponible para `Administrator` y `SuperAdmin`, comprueba el producto, la tienda y la vinculación seleccionados. **No guarda datos ni consulta Irion.**

Cuerpo JSON (sustituye los UUID por un producto y una tienda activos existentes):

```json
{
  "productId": "11111111-1111-1111-1111-111111111111",
  "storeId": "22222222-2222-2222-2222-222222222222",
  "sourceUrl": "https://irion.cl/products/model-1",
  "externalVariantId": "123"
}
```

Usa la URL canónica devuelta por la vista previa y elige explícitamente una variante; no envíes precios. La URL debe ser HTTPS, host exacto `irion.cl`, ruta `/products/{handle}` ASCII con guiones simples, sin parámetros, fragmentos, puerto explícito ni codificación. El identificador de variante es una cadena decimal positiva dentro de Int64. La tienda se valida por su sitio web (URL raíz HTTPS de `irion.cl`, puerto predeterminado), no por su nombre.

La respuesta mantiene HTTP 200 y `ApiResponse`: valida `success` antes de leer `data`. Los errores devuelven `data: null` y un mensaje seguro en español. En una inspección válida, `data.outcome` tiene un valor estable:

| Resultado | Significado |
|-----------|-------------|
| `Create` | No existe la pareja producto/tienda; `inventoryId`, `rowVersion` e `isActive` son `null`. |
| `Update` | Coinciden URL y variante; incluye `inventoryId`, `rowVersion` en base64 e `isActive`. Si está deshabilitado, seguirá deshabilitado. |
| `Conflict` | Existe una oferta antigua sin vinculación o una vinculación diferente; incluye ID, versión y estado actuales. No adopta ni reemplaza la oferta. |

El DTO también devuelve `productId`, `storeId`, `sourceUrl`, `externalVariantId` y un `message` explicativo opcional; no interpretes ese mensaje para decidir el resultado. Inspeccionar o cancelar no escribe; la confirmación vuelve a consultar y validar la oferta antes de guardar. No hay importación automática ni cambios de catálogo.

Comprobaciones deterministas (sin SQL Server ni HTTP):

```sh
dotnet run --project tests/IrionOfferImport.Checks/IrionOfferImport.Checks.csproj --no-restore
```

## Preparar un producto nuevo de Irion (solo lectura)

Esta frontera API aún no crea productos: la confirmación atómica producto + oferta y la pantalla nueva quedan pendientes. El flujo de producto existente sigue disponible.

- `GET /api/maintainer/inventories/irion/new-product/options`: mismos roles administrativos; devuelve `manufacturers`, `franchises`, `categories` como `{id,label}` y `series` como `{id,label,franchiseId}`. Solo referencias activas; excluye series de franquicias inactivas.
- `POST /api/maintainer/inventories/irion/new-product/inspect`: cuerpo `{product,storeId,sourceUrl,externalVariantId}`. `product` contiene `newProductId` (UUID estable generado por el cliente para el borrador), `productName`, `manufacturerId`, `franchiseId`, `serieId` opcional, `productHasBase` explícito (`false` es válido), `productTargetAge`, `productSize`, `productDescription`, `productReleaseDate` (fecha ISO obligatoria, no mínima) y `categoryIds` (lista no vacía, sin UUID vacíos ni duplicados). Nombre máximo 200; edad y tamaño máximo 100. No se inventan fecha, base ni referencias.

No exige ni descarga imágenes; el formulario manual existente no cambia. Valida referencias activas, serie/franquicia, tienda raíz Irion, URL canónica y variante positiva Int64. Devuelve el mismo DTO de inspección `Create`/`Update`/`Conflict`, siempre sin guardar ni consultar Irion. `Update` exige que el ID estable ya tenga exactamente los mismos campos y conjunto de categorías y la misma oferta tienda/URL/variante; imágenes posteriores no se comparan. Un ID ocupado, producto inactivo o nombre de otro ID produce `Conflict`, sin adopción ni sobrescritura. La igualdad de nombre usa la regla existente; la colación y unicidad reales de SQL Server siguen pendientes de T6. Cancelar o inspeccionar nunca crea un producto huérfano.

La futura confirmación debe reutilizar `NewIrionProductValidation.InspectAsync` antes y después de consultar Irion, comprobar instantánea y versión, construir mediante `Product.CreateWithProvidedId` y `AssignCategory`, y guardar producto/categorías/oferta una sola vez. Nunca debe encadenar el handler manual `CreateProduct` (dos guardados).

## Confirmar una oferta de Irion

`POST /api/maintainer/inventories/irion/confirm` requiere el mismo rol administrativo. Primero visualiza el producto, selecciona explícitamente una variante e inspecciona la pareja producto/tienda. Confirma únicamente el resumen que acabas de revisar:

```bash
curl -X POST 'https://localhost:7091/api/maintainer/inventories/irion/confirm' \
  -H "Authorization: Bearer $TOKEN" \
  -H 'Content-Type: application/json' \
  --data '{
    "productId":"11111111-1111-1111-1111-111111111111",
    "storeId":"22222222-2222-2222-2222-222222222222",
    "sourceUrl":"https://irion.cl/products/model-1",
    "externalVariantId":"123",
    "expectedInventoryId":null,
    "expectedRowVersion":null,
    "expectedOffer":{"currency":"CLP","price":199,"originalPrice":200,"available":false}
  }'
```

Para crear, ambos campos `expectedInventoryId` y `expectedRowVersion` son `null`. Para actualizar, copia el ID y la versión de la inspección `Update`; la versión debe ser base64 canónico de **8 bytes**. No confirmes una inspección `Conflict`. `expectedOffer` es la instantánea de la variante seleccionada, no el precio global ni la primera variante; conserva `originalPrice: null` cuando falte.

Las respuestas de aplicación mantienen HTTP 200. **Lee `data.outcome` también cuando `success` sea `false`**; nunca interpretes mensajes para decidir el flujo:

| `data.outcome` | `success` | Acción |
|----------------|-----------|--------|
| `Saved` | `true` | Guardado; `inventoryId` identifica la oferta. |
| `AlreadySaved` | `true` | La misma identidad y valores actuales del servidor ya están guardados; devuelve el ID sin otra escritura ni modificar la fecha. También admite repetir una creación ya guardada. |
| `RefreshRequired` | `false` | La variante desapareció o cambió la instantánea; muestra `freshPreview` y la nueva `inspection`, vuelve a seleccionar/revisar y pide **otra confirmación explícita**. No se guardó. |
| `Conflict` | `false` | Cambió el inventario, su versión o vinculación, o falló la persistencia por concurrencia/duplicado; vuelve a inspeccionar. No reintentes automáticamente. |

Los errores de selección, extracción o precio devuelven `success: false`, `data: null` y un mensaje seguro en español. Los errores de selección conservan los mensajes de inspección; los errores de URL y HTTP 429 del extractor conservan los mensajes específicos documentados arriba. Los demás fallos devuelven `No se pudo confirmar la oferta de Irion.` La cancelación del cliente se propaga; la autorización conserva sus respuestas HTTP habituales.

Seguridad del guardado:

- El servidor valida producto y tienda activos e identidad antes y después de **una sola extracción segura**. No hay reintentos, consultas alternativas ni escrituras con precios del navegador. Compara URL canónica, variante, moneda, precio, precio original y disponibilidad exactos contra la consulta actual.
- Solo acepta CLP enteros positivos hasta Int32; el original debe ser entero en ese rango, cero/ausente o al menos el precio. Original ausente se guarda como `0`; el descuento se calcula en servidor, redondeando mitades hacia fuera de cero. La fecha de observación es UTC del servidor.
- Actualiza solo la misma vinculación y conserva ID y activación administrativa, incluso deshabilitada. No adopta inventarios manuales ni reasocia ofertas. La disponibilidad pertenece a la variante seleccionada, independientemente del stock global.
- EF conserva la versión leída para la actualización optimista y el índice único protege la pareja. Un conflicto no se resuelve sobrescribiendo ni reintentando. Las comprobaciones deterministas simulan fallos y distinguen estado seguido de estado guardado; **no prueban atomicidad ni carreras reales de SQL Server** (pendiente T6). No se aplican migraciones con este flujo de comprobación.

### Guardar desde la pantalla de administración

1. Pulsa **Visualizar producto**. Esta acción sigue siendo de solo lectura.
2. Selecciona un producto activo existente, una tienda activa con URL raíz de Irion y una variante explícita, incluso si solo hay una. No se crean productos ni tiendas.
3. Pulsa **Revisar vinculación**: verás si se creará o actualizará la oferta, o un conflicto que impide guardarla. Un inventario deshabilitado permanecerá deshabilitado.
4. Revisa los precios y disponibilidad de la variante y pulsa **Guardar oferta de Irion**. No se guardan precios editados manualmente ni se repiten envíos automáticamente.

Cambiar la selección o visualizar otra vez invalida la revisión. **Cancelar borrador** limpia la selección sin guardar; no revierte operaciones ya confirmadas. Durante una consulta, revisión o guardado los controles incompatibles quedan bloqueados. Si Irion cambia los datos, se muestra la vista previa actualizada y debes seleccionar, revisar y confirmar de nuevo. Ante errores o conflictos se conserva la URL y la selección válida para revisión manual, sin reintentos automáticos.

**Requisito operativo:** antes de cualquier aplicación de la migración, una persona debe revisar y autorizar un SQL Server aislado, respaldo y revisión de duplicados. La interfaz no aplica migraciones, pero **el arranque de la API en Development sí puede hacerlo automáticamente**, como se advierte arriba. La prueba real de migración y concurrencia SQL Server sigue bloqueada (T6); la revisión independiente de T4 está aprobada para su alcance de código, pero ni esa revisión ni las pruebas de pantalla sustituyen la validación relacional. No hay importación programada ni creación automática de productos.

## Contribución

¡Estamos abiertos a contribuciones! Si deseas colaborar en el proyecto, sigue estos pasos:

1. Realiza un fork del repositorio.
2. Crea una rama para tu contribución: `git checkout -b feature/nueva-funcionalidad`.
3. Desarrolla y realiza tus cambios.
4. Envía un pull request a la rama principal del proyecto.

## Problemas

Si tiene problemas, hágamelo saber planteando un [nuevo problema](https://github.com/dcaceresc/BuscaTuHobby/issues).

## Licencia

Este proyecto se distribuye bajo la licencia [MIT](LICENSE).



¡Gracias por utilizar BuscaTuHobby!

