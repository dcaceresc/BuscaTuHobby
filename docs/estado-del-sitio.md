# Estado del sitio: qué falta y qué no funciona

Auditoría de BuscaTuHobby contra la referencia de `docs/referencia-solotodo.md`. Cada hallazgo lleva
evidencia (`archivo:línea`). El documento separa tres cosas que conviene no mezclar: lo que **no
existe**, lo que existe pero **no se puede usar**, y lo que existe y **miente**.

## Qué puede hacer un visitante hoy, de principio a fin

1. Llega a `/`: el dashboard muestra categorías, tiendas, posts, "más buscados", ofertas, categorías
   populares, tiendas destacadas y actividad reciente.
2. Busca en el cajón del dashboard: máximo 10 resultados, sin paginación, sin filtros, sin orden.
3. Hace clic en una tarjeta: va a `/products/:productId`.
4. Ve características, galería, tiendas de la más barata a la más cara y el gráfico de precios.
5. Sale del sitio por el enlace de la oferta o de la tienda.

Ahí termina el recorrido. No hay forma de explorar por categoría, franquicia, serie ni fabricante; no
hay página de tienda; no hay favoritos, alertas ni opiniones; y varios caminos del menú llevan a rutas
que no existen.

## A. Funcionalidades de la referencia que no tenemos

| Funcionalidad | SoloTodo | Aquí |
| --- | --- | --- |
| Jerarquía modelo base / variante | confirmada | ausente: `Product` es plano (`src/Domain/Entities/Product.cs:24-36`) |
| `part_number` / JAN | confirmado | ausente |
| Especificaciones tipadas por categoría | confirmadas | `ProductSize` y `ProductDescription` son texto libre |
| Navegación por categoría con facetas | confirmada | ausente: no hay ruta ni endpoint público de categoría |
| Navegación por franquicia, serie o fabricante | — | ausente: solo existen como mantenimiento |
| Condición de la oferta (nuevo/reacondicionado/usado) | confirmada | ausente en `Inventory` |
| Precio por medio de pago | confirmado | un único `Price` |
| Frescura visible por oferta | confirmado | `ObservedAtUtc` existe y no se muestra |
| Alertas de bajada de precio | confirmadas | ausente; la campana del header es un `<span>` estático (`header.component.html:13`) |
| Opiniones de producto | confirmadas | `Review` es de tienda y está muerto |
| Página pública de tienda con comentarios | confirmada | ausente |
| Productos relacionados | confirmados | ausente |
| Página de artículo para los posts | — | ausente |
| API pública | confirmada | todo el consumo es interno |
| Afiliación en los enlaces de salida | confirmada | ausente |

## B. Lo que existe y no se puede usar

### Rutas y enlaces que no llevan a ninguna parte

| Elemento | Destino | Estado |
| --- | --- | --- |
| Menú "Ofertas" | `/ofertas` | ruta inexistente (`menu.component.html:14`) |
| Menú "Mis Favoritos" | `/favoritos` | ruta inexistente (`menu.component.html:15`) |
| Menú "Historial" | `#` | enlace vacío (`menu.component.html:16`) |
| Menú de usuario "Perfil" | `/profile` | ruta inexistente (`login-menu.component.html:28`) |
| Categorías del menú | `href="{{category.categorySlug}}"` | sale del SPA y cae en ruta inexistente (`menu.component.html:40`) |
| Tiendas del menú | `href="{{store.storeSlug}}"` | idem (`menu.component.html:61`) |
| "Ver todas…" de categorías y tiendas | `#` | enlace vacío (`menu.component.html:42,63`) |

No hay ruta comodín en `src/Web/WebUI/src/app/app.routes.ts`, así que cualquiera de esos destinos rompe
la navegación en lugar de mostrar una página de error.

### Código sin producto detrás

| Elemento | Estado | Evidencia |
| --- | --- | --- |
| Favoritos | `Favorite` y `FavoriteProduct` migrados; `CreateFavorite.cs` y `GetFavorites.cs` son clases **vacías sin handler**; sin módulo y sin UI | `src/Application/Favorites/**`, `ApplicationDbContext.cs:10-11` |
| Reseñas | `CreateReview`, `UpdateReview` y `GetReviews` sin lógica; sin módulo; `Review` es de tienda | `src/Application/Reviews/**`, `Review.cs:5-8` |
| `SiteStatus` | se lee de configuración y nunca se consulta: no hay modo mantenimiento | `ConfigurationInitializer.cs:35` |
| Ofertas destacadas | `BestDealDto` no expone `ProductId`, así que la fila no puede enlazar al producto | `BestDealDto.cs` |
| Posts | "Leer más" solo incrementa el contador y recarga; no hay artículo | `recent-posts.component.ts:39-49` |

Si se despachara cualquiera de los comandos vacíos, `RequestDispatcher` lanzaría desde
`GetRequiredService` y la respuesta sería un 500, no un fallo controlado.

### Defectos en lo que sí funciona

| Defecto | Impacto | Evidencia |
| --- | --- | --- |
| El historial de precios ignora la elegibilidad de la oferta: filtra solo por `Inventory.ProductId`, así que incluye ofertas inactivas y tiendas cerradas | Alto: el gráfico no coincide con la tabla de ofertas | `GetProductPriceHistory.cs:36-38` |
| El parámetro `days` está muerto: el frontend siempre envía `0` | Bajo: código y validación sin uso | `product-detail.component.ts:106`, `DashboardModule.cs:45` |
| El gráfico dibuja solo el precio mínimo; el máximo es una línea de referencia plana | Medio: se lee como si el máximo fuese constante | `price-chart.component.html:47-59` |
| Sin backfill, un producto de precio estable muestra un solo punto y el mensaje de "recopilando" | Medio | `InventoryPriceHistoryRecorder.cs:14-17` |
| No se siembra ningún `ProductImage`, y las URL se arman como `{FolderImage}/{id}.jpg` sobre una ruta relativa al cwd, sin `wwwroot` en el repo | Alto: las fotos reales podrían no servirse | `UtilityService.cs:72-81`, `Program.cs:18-19` |
| `PUT /api/dashboard/products/{id}/view` y `/posts/{id}/view` no están autenticados y mutan contadores persistentes | Medio: cualquiera infla el ranking | `DashboardModule.cs:26,28` |
| `/security/**` no tiene `AdminGuard` en el frontend | Bajo: el API sí protege | `app.routes.ts:14-17` |
| Clave de firma JWT en `appsettings.Development.json` | Alto si ese archivo llega a producción | `appsettings.Development.json:9` |
| Sin pruebas de búsqueda, widgets del dashboard ni guards | Medio | solo hay specs de `AppComponent`, `NotificationService`, `ProductDetail`, `PriceChart` e Irion |

## C. Lo que miente

El seed de `src/Infrastructure/Data/ApplicationDbContextInitialiser.cs` (4 productos, 6 tiendas inventadas
como Amazon, Linio, Ripley y París, 24 ofertas con precios inventados, contadores escritos a mano) corre
**solo en Development**: `Program.cs:24` envuelve `InitialiseDatabaseAsync()` en
`app.Environment.IsDevelopment()`. No es una mentira al usuario final, es data de demo. Lo que sí es un
defecto de código, porque afectaría también a datos reales, es lo que se calcula mal:

| Afirmación mostrada | Realidad | Evidencia |
| --- | --- | --- |
| "{{ n }} búsquedas" en Más Buscados | No hay registro de búsquedas: es `ProductViewCount` | `most-searched-products.component.html:20`, `GetMostSearchedProducts.cs:15-20` |
| Badges "TRENDING" y "POPULAR" | Son la posición en la lista: `index < 2` | `most-searched-products.component.ts:52-54` |
| "Categorías populares" con porcentajes | Los porcentajes se calculan sobre **solo el top 4**, así que siempre suman ~100 % | `GetPopularCategories.cs:26-31` |
| "Actividad reciente" | Incluye productos, ofertas y tiendas inactivos porque no filtra `IsActive` | `GetRecentActivity.cs:15-60` |
| Tarjetas de producto con `placeholder.svg?height=200&width=300` | El query string no hace nada: es un SVG fijo de 120×120 | `most-searched-products.component.html:8` |

Y lo que sí es contenido de demo que conviene no confundir con datos reales cuando se muestre en un
entorno compartido: los contadores 1234/987/856/745 y 3245/2892/4128/5673 y las cuatro "Guías y
Comparativas", que son prosa inventada (`ApplicationDbContextInitialiser.cs:112-140,185-188`).

## D. Lo que no se pudo determinar con el código

- Si las imágenes subidas se sirven en un despliegue real: no hay `wwwroot` en el repositorio y
  `FolderImage` se configura desde la base de datos.
- Si el correo de confirmación se envía: solo se observó la interfaz `IEmailService`.
- Si el importador de Irion es la fuente prevista de ofertas reales: es el único camino por el que
  entran datos de tiendas reales, y hoy es solo de mantenimiento.
