# Página de producto y evolución de precios

La página pública de un producto vive en `/products/:productId` y se abre desde los resultados de
búsqueda y desde las tarjetas de "Productos más buscados". Muestra las características del producto,
su galería, las tiendas que lo venden ordenadas de la más barata a la más cara y un gráfico con la
evolución del precio.

## Endpoints

Ambos son públicos (sin autenticación) y aditivos: no modifican ningún contrato existente.

### `GET /api/dashboard/products/{id}`

Devuelve `ApiResponse<ProductDetailDto>`.

| Campo | Contenido |
| --- | --- |
| Características | nombre, descripción, fabricante, franquicia, serie, escala, edad recomendada, fecha de lanzamiento, si requiere base, visitas |
| `categories` | nombres de categoría |
| `imageUrls` | rutas `{FolderImage}/{productImageId}.jpg`, en el orden de las imágenes |
| `offers` | ofertas ordenadas por `Price` ascendente y, a igual precio, por nombre de tienda |
| `storeCount`, `minPrice`, `maxPrice` | agregados calculados solo sobre las ofertas listadas |

Una oferta aparece solo si se cumple todo: `Inventory.IsActive`, `Store.IsActive`,
`Product.IsActive` y `Availability != false`. Es la misma regla de elegibilidad que usan
`GetBestDeals` y `GetMostSearchedProducts`.

Si el producto no existe o está inactivo, la respuesta es `success: false` con
`"El producto no existe o no está disponible."`; nunca se lanza una excepción al cliente.

### `GET /api/dashboard/products/{id}/price-history?days=90`

Devuelve `ApiResponse<ProductPriceHistoryDto>` con un punto por día observado:

| Campo | Contenido |
| --- | --- |
| `date` | día UTC de la observación |
| `minPrice` / `maxPrice` | precio más bajo y más alto observado ese día |
| `offerCount` | cuántas ofertas distintas se observaron ese día |

`days` es opcional y se limita a `0..3650`. Un valor omitido o `0` devuelve **toda** la serie
registrada: una ventana fija escondería el único punto que tiene una oferta y mostraría un gráfico
vacío aunque exista historial.

## Historial de precios

La tabla `InventoryPriceHistories` es de solo agregado: nunca se actualiza ni se borra una fila.

- Se escribe un punto **solo** cuando cambia `Price`, `OriginalPrice`, `DiscountPercentage` o
  `Availability`. Actualizar solo `SourceUrl`, `ExternalVariantId` u `ObservedAtUtc` no escribe nada,
  de modo que una importación repetida e idéntica no infla la serie.
- La regla vive en un único lugar: `Application.Common.PriceHistory.InventoryPriceHistoryRecorder`.
  Se invoca desde `UpdateInventory`, `ConfirmIrionOffer` y `ConfirmNewIrionProductOffer`.
- Al **crear** una oferta se registra un punto base (`IsBaseline = true`) con sus valores iniciales,
  para que la serie empiece en la creación y no quede vacía hasta el primer cambio de precio.
- La migración `20260721000000_InventoryPriceHistory` siembra un punto base por cada oferta activa
  existente. El seeder de desarrollo hace lo mismo con las ofertas que inserta.
- Borrar una oferta borra su historial (`ON DELETE CASCADE`).

## Verificación

- Backend: `dotnet run --project tests/IrionOfferImport.Checks/IrionOfferImport.Checks.csproj`.
  Las suites `PriceHistoryChecks` y `ProductDetailChecks` cubren el registro por cambio, el punto
  base, el orden más barato primero y la agregación por día.
- Frontend: `cd src/Web/WebUI && pnpm test:run` (specs de `product-detail` y `price-chart`).

Advertencia de alcance: esas comprobaciones de backend usan un `DbContext` falso en memoria. No
demuestran la traducción a SQL (`ORDER BY`, `CAST(datetime2 AS date)`), ni que la migración aplique
y revierta en SQL Server, ni que el SQL de siembra sea idempotente en un servidor real. Eso exige
una base aislada accesible.
