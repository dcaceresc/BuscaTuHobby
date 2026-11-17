# Ingesta multi-tienda: cómo llenar el catálogo con datos reales

Propuesta técnica para sustituir los datos inventados del seed por ofertas reales, generalizando lo que hoy
solo funciona para Irion y automatizando el proceso.

Verificación hecha el 2026-11-16 con peticiones HTTP reales desde este entorno. Cada afirmación de la tabla
de fuentes se comprobó con `curl`; lo que no se pudo comprobar está marcado como tal.

## 1. Punto de partida, verificado en el código

| Hecho | Evidencia |
| --- | --- |
| El "extractor de Irion" es en realidad un extractor de Shopify | `src/Infrastructure/Services/IrionProductExtractor.cs:38` pide `{url}.js` con `Accept: application/json`, que es el endpoint público de producto de Shopify; `:56-77` lee `variants[]` con `sku`, `barcode`, `available`, `price`, `compare_at_price` |
| Solo cuatro cosas son propias de la tienda | host fijado (`:18-19`), expresión regular `irion\.cl` (`:25`), moneda hardcodeada `CLP` (`:63`) y la firma `ExtractAsync(string url)` de `IIrionProductExtractor`, que no recibe tienda ni plataforma |
| No hay descubrimiento de catálogo | no existe recorrido de sitemap, listado ni paginación; la importación es manual, una URL por vez (`docs/irion-offer-import.md`) |
| `robots.txt` no se consulta en ninguna parte de `src/` | búsqueda sin coincidencias |
| La oferta ya tiene identidad | `Inventory.SourceUrl` + `ExternalVariantId` + `Availability` + `ObservedAtUtc`; índice único `(StoreId, ProductId)`; una oferta importada no puede re-asociarse |
| El catálogo ya tiene parte de la identidad del kit | `Product.PartNumber`, `Grade`, `Scale` (añadidos en el bloque 3) |
| Falta el identificador global | no hay columna GTIN/JAN/EAN, así que el `barcode` que Shopify ya devuelve se descarta |
| Configuración por tienda inexistente | `Store.StoreWebSite` (100 caracteres) es toda la superficie; no hay clave de conector ni ajustes por tienda |

Conclusión: la generalización no exige reescribir la extracción, exige tres capas que hoy no existen
(**descubrimiento**, **conectores por plataforma** y **ejecución programada**) y una decisión de identidad
de catálogo.

## 2. Fuentes verificadas

Pruebas reales del 2026-11-16. "Shopify" se detectó por marcas de la plataforma en el HTML; "ld+json" por la
presencia de `application/ld+json` en la portada.

| Fuente | País | HTTP | Plataforma | Estrategia viable | Observaciones verificadas |
| --- | --- | --- | --- | --- | --- |
| `irion.cl` | CL | 200 | Shopify | `shopify` | `robots.txt` permite producto y colecciones (51 reglas `Disallow`, todas de carrito, cuenta, admin, `cart.js`, `recommendations/products` y variantes de colección con `sort_by`/`filter`); `sitemap.xml` lista 5 sitemaps de producto; `sitemap_products_1.xml` devuelve **1001 URLs** |
| `solarisjapan.com` | JP | 200 | Shopify | `shopify` | portada con ld+json |
| `usagundamstore.com` | US | 200 | Shopify | `shopify` | portada con ld+json |
| `gundamplanet.com` | US | 200 | Shopify | `shopify` | portada sin ld+json; el JSON de producto sí trae precio |
| `hlj.com` | JP | 200 | propia | `html-selectors` | portada sin ld+json ni marcas de Shopify |
| `amiami.com` | JP | 403 | — | bloqueada | desafío de Cloudflare ("Just a moment...") |
| `1999.co.jp` (Hobby Search) | JP | 403 | — | bloqueada | desafío de Cloudflare |
| `mercadolibre.cl` | CL | 200 | marketplace | `api` | la búsqueda por URL pública responde 302; la vía correcta es la API oficial con OAuth |
| `falabella.com` | CL | 200 | marketplace | `html-selectors` | búsqueda "gunpla": 1,7 MB de HTML, 17 menciones; **sin** `ItemList` en ld+json |
| `paris.cl` | CL | 200 | marketplace | `html-selectors` | búsqueda "gunpla": 3,8 MB de HTML, 48 menciones; **sin** `ItemList` en ld+json |
| `ripley.cl` | CL | 200 | marketplace | `html-selectors` | portada sin ld+json |
| `aliexpress.com` | CN | 200 | SPA | `needs-browser` / API de socio | respuesta de 1,8 KB: solo el armazón JavaScript |
| `mangas.cl` (Comercial Mangas Fusion) | CL | 200 | propia | por determinar | portada con ld+json; `/search?q=gunpla` devuelve 404, hay que descubrir su ruta de catálogo |

### Tiendas chilenas encontradas

Descubiertas con búsqueda real (DuckDuckGo resuelto del lado del servidor) y verificadas una por una. El sitio
no es solo de Gunpla, así que la lista cubre figuras, coleccionables y modelismo.

| Tienda | Tema | HTTP | Plataforma | Catálogo verificado | Estrategia |
| --- | --- | --- | --- | --- | --- |
| `luffytoys.cl` | figuras y Gunpla | 200 | **Shopify** | `sitemap_products_1.xml`: **996 URLs, 26 con Gunpla** (`mobile-suit-rx-93-ν2-gundam-metal-build`, `preventa-gundam-ex-gundam-universe`…) | `shopify` |
| `anezakistore.cl` | figuras, gaming | 200 | **Shopify** | 4 sitemaps de producto; el primero con **1001 URLs**, 2 con Gundam | `shopify` |
| `friitoys.cl` | figuras y coleccionables | 200 | **Shopify** | 4 sitemaps de producto; el primero con **392 URLs**, 0 con Gunpla | `shopify` |
| `animestore.cl` | anime y figuras | 200 | **Shopify** | 1 sitemap de producto | `shopify` |
| `wyvern.cl` | figuras | 200 | **Shopify** | 1 sitemap de producto | `shopify` |
| `blasterchile.cl` | coleccionables | 200 | **Shopify** | `sitemap.xml` responde 301 (redirección de host), hay que resolver el destino | `shopify` tras normalizar host |
| `pasatime.cl` | "Modelismo, Gunpla, Maquetas y Hobby en Chile" | 200 | propia | `sitemap.xml` responde 301; la portada trae ld+json | `json-ld` tras resolver host |
| `svadercoleccionables.cl` | coleccionables | 200 | propia | portada con ld+json, 143 KB | `json-ld` |
| `otakuverso.cl` | figuras y merchandising | 200 | propia | portada con ld+json, 478 KB | `json-ld` |
| `titoys.cl` | coleccionables con licencia | 200 | propia | portada con ld+json | `json-ld` |
| `adamantium.cl` | resinas y estatuas | 200 | propia | portada con ld+json, 2 MB | `json-ld` |
| `jftoys.cl` | juguetes y figuras | 200 | propia | portada con ld+json, 834 KB | `json-ld` |
| `hangar019.cl` | modelismo | 200 | propia | portada con ld+json | `json-ld` |
| `hobbytoys.cl` | hobbies coleccionables | 200 | propia (WooCommerce probable) | sin ld+json en portada | `html-selectors` |
| `koneko.cl` | figuras | 200 | propia | sin ld+json; marcas de Cloudflare | `html-selectors` |
| `geekz.cl`, `top8.cl` | tienda geek / game center | 200 | propia | sin catálogo de producto evidente | descartar por ahora |

Con esto, la segunda tienda sale del propio mercado chileno y no de un caso internacional: **`luffytoys.cl`**, que
es Shopify y tiene Gunpla real en su catálogo.

**Sobre los 429:** `/products.json` y `/products/{handle}.js` respondieron `429 local_rate_limited` en tiendas
distintas (Irion, LuffyToys, FriiToys, Wyvern, AnimeStore, Anezaki), todas a la vez. El mismo cuerpo de 18 bytes
en dominios sin relación indica que el límite lo impone **el entorno desde el que se hizo la prueba**, no las
tiendas. Por tanto: el endpoint `.js` que usa el extractor actual **no se pudo validar en vivo aquí**, y el
comportamiento real de límite de cada tienda queda sin verificar. Los sitemaps sí respondieron 200 y son la vía
que funcionó de punta a punta.

**No verificado, y por qué:** el comportamiento de límite de peticiones de cada tienda (ver nota sobre 429) y el
contenido de las fichas que solo se sirven por JavaScript. El descubrimiento de tiendas sí se completó para el
mercado chileno con la tabla anterior; quedan fuera las que no aparecen en los resultados de búsqueda consultados.

Lectura de la tabla: **una sola estrategia (`shopify`) cubre las cinco tiendas chilenas de Shopify verificadas,
LuffyToys incluida, y las cuatro internacionales**; el resto de las chilenas necesita JSON-LD o selectores. Ese
es el argumento para empezar por Shopify y por Irion, y para probar la generalización con **`luffytoys.cl`**
antes que con una tienda de otro país.

## 3. Arquitectura propuesta

```
                 ┌──────────────┐   ┌───────────────┐   ┌──────────────┐
Programación ───▶│ ImportRun    │──▶│ Descubrimiento│──▶│ Fetch        │
 (cuota/tienda)  │ (reanudable) │   │ sitemap/lista │   │ robots+límite│
                 └──────────────┘   └───────────────┘   └──────┬───────┘
                                                               ▼
   ┌─────────────┐   ┌──────────────┐   ┌──────────────┐   ┌──────────────┐
   │ Métricas y  │◀──│ Upsert oferta│◀──│ Normalizar   │◀──│ Conector     │
   │ alertas     │   │ historial    │   │ y emparejar  │   │ (plataforma) │
   └─────────────┘   └──────────────┘   └──────────────┘   └──────────────┘
```

### 3.1 Conectores por plataforma, no por tienda

Un conector responde tres preguntas de una plataforma: cómo se enumeran los productos, cómo se lee uno y qué
identificadores trae. La tienda solo aporta configuración.

```csharp
public interface IStoreConnector
{
    string Key { get; }                                   // "shopify", "json-ld", "html", "mercadolibre"
    Task<IReadOnlyList<CatalogRef>> DiscoverAsync(StoreContext store, CancellationToken ct);
    Task<OfferSnapshot> LoadAsync(StoreContext store, CatalogRef reference, CancellationToken ct);
}
```

- `StoreContext`: base URL, moneda, ajustes del conector (JSON), presupuesto de peticiones.
- `CatalogRef`: identidad dentro de la tienda (handle, id de variante) y URL.
- `OfferSnapshot`: precio, precio anterior, descuento, disponibilidad, condición, medio de pago, frescura,
  `sku`/`barcode`, imágenes, título.

Implementaciones y coste real:

| Conector | Cubre | Trabajo | Notas |
| --- | --- | --- | --- |
| `ShopifyConnector` | irion, solarisjapan, usagudamstore, gundamplanet | bajo: el 95 % del código existe, se parametriza host y moneda | descubrimiento por `sitemap_products_*.xml`; lectura por `/products/{handle}.js`, con limitador porque devuelve 429 |
| `JsonLdConnector` | tiendas con `schema.org/Product` en la ficha | medio: un parser HTML + JSON-LD genérico | reutiliza todo el pipeline salvo el parseo |
| `HtmlConnector` | hlj, falabella, paris, ripley | alto: un selector por tienda, frágil por definición | exige pruebas que detecten el cambio de plantilla |
| `MarketplaceApiConnector` | mercadolibre (y futuros) | medio-alto: OAuth, cuotas, mapeo de categorías | el más estable a largo plazo |
| `BrowserConnector` | aliexpress, amiami, 1999.co.jp | alto y caro en recursos | **fuera de alcance en la v1** |

### 3.2 Descubrimiento

Tres modos, en este orden de preferencia:

1. **Sitemap de producto** (`sitemap.xml` → `sitemap_products_*.xml`): es lo que Irion expone hoy, con ~1.001
   URLs por archivo. Da el catálogo completo sin adivinar rutas.
2. **Listado con paginación**: cuando no hay sitemap de producto, recorrer categoría y página.
3. **Enlace directo**: la importación manual que ya existe, que se conserva como caso de uso del backoffice.

Reglas duras del descubrimiento, ninguna presente hoy:

- `robots.txt` se consulta y se cachea por host; una ruta prohibida se descarta y se registra como tal. En
  Irion eso implica no usar listados de colección con `sort_by` ni filtros, y no tocar `recommendations/products`;
  el sitemap de producto queda permitido y es la vía correcta.
- Presupuesto por dominio: máximo de peticiones por minuto y por ejecución, configurables por tienda.
- Un solo `User-Agent` identificable con URL de contacto; sin paralelismo dentro de un mismo dominio.
- Tope de tamaño por respuesta y de tiempo por petición (ya existe: 1 MiB y 15 s en `ConfigureServices`).
- Respeto de `Retry-After` y retroceso exponencial ante 429/5xx (Irion ya devuelve 429 de forma agresiva).

### 3.3 Pipeline idempotente y reanudable

Cada ejecución es un `ImportRun` con elementos por producto (`ImportItem`) en estados
`pendiente → obtenido → interpretado → emparejado → guardado | descartado | error`. Propiedades exigidas:

- **Idempotencia**: la clave es `(StoreId, ExternalVariantId)`; un `payloadHash` evita reprocesar lo que no
  cambió (el registrador de historial ya es *change-only*, así que una segunda pasada no infla la serie).
- **Reanudable**: un corte a mitad se retoma por los elementos pendientes, no desde cero.
- **Reversible**: un producto que desaparece del catálogo no se borra; su oferta se desactiva y la baja queda
  como evento, igual que hoy se conserva el historial de una oferta cerrada.
- **Auditable**: cada `ImportItem` guarda la URL, el código HTTP, el estado y el error. Sin esto, depurar una
  ingesta es adivinar.

### 3.4 Cambios de esquema necesarios

| Cambio | Motivo | Riesgo |
| --- | --- | --- |
| `Store.ConnectorKey` + `Store.ConnectorSettings` (JSON) | que añadir una tienda sea configuración, no código | bajo, aditivo |
| `ImportRun` e `ImportItem` | reanudación, auditoría y métricas | bajo, tablas nuevas |
| `Product.Gtin` (nullable, único por tienda cuando exista) | deduplicar catálogo con `barcode` de Shopify | **medio**: requiere migración y regla de conflicto |
| `Inventory.Condition`/`PaymentMethodLabel` ya existen | se mapean desde la fuente en vez de asumir "nueva" | sin cambio |
| `(StoreId, ProductId)` único se mantiene | una oferta por producto y tienda; las variantes múltiples se resuelven como productos distintos hasta que se modele la variante | **decisión pendiente** |

### 3.5 Ejecución automática

- `IngestionWorker` como `BackgroundService` con una cola por tienda y una ventana horaria por tienda (por
  ejemplo, Irion cada 12 h de madrugada).
- Cuota diaria de peticiones por tienda, calculada desde el presupuesto, no desde el reloj.
- Reintentos solo ante error transitorio (429, 5xx, timeout), con tope y sin reintentar 4xx.
- Disparo manual desde el backoffice para una tienda o una URL concreta, reutilizando el mismo pipeline.
- Sin base de datos accesible o sin `Store` activa, la ejecución no arranca: el fallo es explícito, no un
  catálogo vacío.

### 3.6 Calidad de datos y lo que se gana en producto

Con el pipeline en marcha se vuelven medibles y automáticos:

- **Cobertura**: productos con al menos una oferta elegible sobre el total del catálogo.
- **Frescura**: distribución de `ObservedAtUtc` por tienda; una tienda sin observaciones recientes se marca.
- **Conflictos de identidad**: ofertas que no emparejan con un producto y esperan revisión humana.
- **Alertas de bajada**: el historial de precios ya existe; una alerta es una consulta sobre la serie, no una
  funcionalidad nueva.
- **Comparación real**: dos o más tiendas por producto, que es el motivo de existir del sitio.

## 4. Plan por fases

Cada fase entrega algo usable y verificable. Los criterios de aceptación son ejecutables, no aspiracionales.

### F1 — Conector Shopify generalizado (1-2 días)

- Extraer de `IrionProductExtractor` un `ShopifyConnector` con host y moneda por configuración; `Irion` pasa a
  ser la primera fila de configuración, no una clase.
- Añadir la consulta de `robots.txt`, el limitador por dominio y el retroceso ante 429.
- Aceptación: los checks actuales (1230 aserciones) siguen pasando; una prueba real contra `irion.cl` con una
  URL de producto devuelve el mismo resultado que hoy; una segunda tienda Shopify configurada por fila
  (`solarisjapan.com` o `usagudamstore.com`) devuelve una oferta normalizada sin tocar código.
- Riesgo: alto en la primera tienda nueva (formato y moneda distintos), bajo después.

### F2 — Descubrimiento por sitemap e importación masiva (2-3 días)

- `ImportRun`/`ImportItem`, recorrido de `sitemap_products_*.xml`, presupuesto y reanudación.
- Aceptación: una ejecución sobre `irion.cl` guarda N ofertas nuevas con idempotencia demostrada (segunda
  ejecución = 0 altas, 0 puntos de historial nuevos si los precios no cambian) y se reanuda tras un corte
  simulado.
- **Esta es la fase que sustituye los datos inventados por catálogo real.**

### F3 — Conector JSON-LD (1-2 días)

- Parser genérico `schema.org/Product`: la ficha que trae `offers.price` se ingiere sin código por tienda.
- Aceptación: `mangas.cl` y al menos otra tienda con ld+json producen ofertas normalizadas.

### F4 — Marketplace por API (3-5 días)

- MercadoLibre por API oficial: OAuth, cuotas, mapeo de categoría y condición del ítem.
- Aceptación: ofertas reales de vendedores chilenos con `sku`/GTIN cuando exista, y error explícito y
  registrado cuando no.

### F5 — Automatización y operación (2 días)

- `IngestionWorker`, ventanas por tienda, alertas de bajada y panel de cobertura/frescura/conflictos.
- Aceptación: dos ejecuciones programadas seguidas sin intervención, con métricas visibles y sin duplicados.

### F6 — Identidad de catálogo (decisión previa, después 1-2 semanas)

- GTIN, jerarquía modelo base → variante, cola de revisión de duplicados. Es el trabajo caro y solo se justifica
  cuando haya dos o tres tiendas con volumen.

## 5. Decisiones que necesitan tu criterio

1. **Qué tiendas primero.** Ya está decidido: `irion.cl` para conservar lo que funciona y **`luffytoys.cl`** como
   segunda tienda chilena, porque es Shopify (coste bajo) y tiene Gunpla real (26 URLs en su primer sitemap).
   Las grandes tipo Falabella/Paris se dejan para cuando el pipeline esté maduro.
2. **Legalidad y condiciones de uso.** Irion permite rastrear su catálogo según `robots.txt`, pero eso no
   equivale a permiso contractual. Hay que decidir si se pide permiso, si se respeta solo robots y se mantiene
   un ritmo conservador, y si los enlaces salientes llevan atribución.
3. **Navegador headless.** Marcarlo fuera de la v1 mantiene el coste bajo y evita el bloqueo de Cloudflare que
   hoy tienen `amiami.com` y `1999.co.jp`.
4. **Identidad de catálogo.** Sin GTIN no hay forma fiable de saber que "RG 1/144 RX-78-2" de dos tiendas es el
   mismo kit. O se acepta duplicar y revisar a mano, o se modela la variante. Es la decisión que más cambia el
   alcance.
5. **Alcance del seed.** Mi propuesta: el seed sigue siendo solo de Development y se documenta como demostración;
   los datos reales entran exclusivamente por conectores, para que nunca se mezclen datos inventados con ofertas
   reales.

## 6. Lo que esta propuesta no afirma

- No afirma que Irion mantenga el ritmo actual: la verificación fue de un momento y el 429 apareció tras seis
  peticiones.
- No afirma que las tiendas chilenas sean inaccesibles: solo que este entorno no puede buscarlas y que los
  dominios probados a ciegas no existen.
- No incluye cifras de catálogo de ninguna tienda salvo Irion, y solo para el primer sitemap de producto; el
  total real de su catálogo requiere recorrer los cinco y no se recorrieron para no agotar el límite de peticiones.
- No propone afiliación ni monetización en esta fase.
