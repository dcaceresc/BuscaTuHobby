# Referencia: SoloTodo.cl

Análisis del sitio de referencia declarado para BuscaTuHobby. Sirve como marco de comparación: qué
hace un comparador de precios chileno que funciona, y qué de eso nos falta.

Verificado el **2026-11-16** con peticiones HTTP reales al sitio y a su API. Cada afirmación de este
documento está marcada como **confirmada** (con la URL o el dato observado) o **no verificada** (no se
pudo observar). No se completó nada de memoria: lo que no se vio, se dice.

## Cómo se verificó

| Petición | Resultado observado |
| --- | --- |
| `GET https://solotodo.cl/` | 410 kB de HTML; solo 5 enlaces servidos en el HTML, el resto se renderiza en cliente |
| `GET https://www.solotodo.cl/robots.txt` | `User-agent: *` / `Disallow: /search` |
| `GET https://solotodo.cl/sitemap.xml` | índice con 10 sitemaps de productos y uno de "others" |
| `GET https://www.solotodo.cl/celulares` | 2,7 MB de HTML con contenido completo: 236 resultados, facetas, orden y paginación |
| `GET https://www.solotodo.cl/products/264814-samsung-galaxy-s25-ultra-…` | 500 kB; ficha técnica, ofertas, alerta de precio, opiniones y relacionados |
| `GET https://www.solotodo.cl/stores/9/comments` | 200, pero el contenido se renderiza en cliente: no se pudo leer |
| `GET https://api.solotodo.com/products/?limit=1` | 200; `count: 182331` |
| `GET https://api.solotodo.com/products/264814/` | 200; claves `part_number`, `specs`, `last_updated`, `instance_model_id`, `picture_url` |
| `GET https://api.solotodo.com/entities/?limit=1` | 200; `count: 1724641` |
| `GET /los_mas_vistos`, `/recomendados_para_ti` | 200 (contenido en cliente) |

## Filosofía, en cinco principios

1. **El dato es tipado, no texto.** Cada categoría define sus especificaciones y una plantilla
   (`browse_result_template`) que decide qué líneas de spec muestra cada tarjeta del listado. Por eso
   los resultados son comparables entre sí y no una lista de descripciones libres. **Confirmado** en
   `GET api.solotodo.com/categories/`, donde cada categoría trae su plantilla.
2. **Tres niveles de identidad.** Categoría → modelo base (`base_model_id`) → variante concreta
   (`product` con `part_number`). La ficha muestra "variantes" del mismo modelo base. **Confirmado**
   en el JSON del producto y en el texto "Cargando variantes ..." de la ficha.
3. **La oferta es un registro rico, no un precio.** Tienda + condición del producto + medio de pago +
   precio normal + disponibilidad + frescura. **Confirmado** en la sección "Elige tu tienda": cada fila
   trae tienda, etiqueta de medio de pago, precio, opcionalmente "Precio normal" y la etiqueta de
   condición ("Liberado", "Reacondicionado", "Usado").
4. **El precio pasado es un activo vendible.** Alertas de bajada y orden por precio de oferta.
   **Confirmado** el control "Avísame cuando baje de precio" y el selector "Ordenar por: Precio oferta".
5. **Abre el dato y monetiza la salida.** API REST pública con 182.331 productos y 1,7 M de entidades,
   más enlaces salientes con `utm_source=SOLOTODO&utm_campaign=…`. **Confirmado** en ambos casos.

## Inventario de funcionalidades

| Funcionalidad | Estado | Evidencia |
| --- | --- | --- |
| Navegación por categoría con listado | Confirmada | `/celulares`: "236 resultados", "1–20 de 236", "Items por pág. 20" |
| Facetas por especificación | Confirmada | Filtros agrupados: "Datos generales", "Pantalla", "Avanzado", "Accesorios", con rangos y recuentos por valor |
| Filtro de precio y de palabras clave | Confirmada | "Precio $40.306 – $4.135.359", "Palabras clave", "Borrar filtros" |
| Orden configurable | Confirmada | "Ordenar por" |
| Tarjeta de producto con specs comparables | Confirmada | Cada tarjeta muestra 5 líneas (pantalla, almacenamiento, RAM, cámara, color, procesador) + "Desde: $X" + "Ver precios" |
| Ficha técnica agrupada | Confirmada | "Características", "Características avanzadas", "Accesorios", con notas de advertencia al comprador |
| Agrupación de variantes | Confirmada | "Cargando variantes ..." |
| Tabla de ofertas por tienda | Confirmada | "Elige tu tienda" |
| Condición por oferta | Confirmada | "Liberado", "Reacondicionado", "Usado" |
| Medio de pago por oferta | Confirmada | "Con todo medio de pago", "Tarjeta CMR" |
| Precio normal tachado | Confirmada | "Precio normal $969.990" |
| Alerta de bajada de precio | Confirmada | "Avísame cuando baje de precio" |
| Opinión del producto por compradores | Confirmada | "¿Lo compraste? ¡danos tu opinión!" |
| Comentarios por tienda | Confirmada (ruta) | `/stores/{id}/comments`; contenido no legible sin JS |
| Productos relacionados | Confirmada | "También te puede interesar" |
| Contenido editorial (pros/contras) | Confirmada | Bloque "Pros" / "Contras" en la descripción del producto |
| Part number / código de fabricante | Confirmada | `part_number: "SM-S938BZKJLTL"` y en el título entre corchetes |
| Frescura del dato | Confirmada (API) | `last_updated` en el recurso de producto |
| Imágenes en CDN | Confirmada | `picture_url: https://media.solotodo.com/media/products/...webp` |
| API pública | Confirmada | `api.solotodo.com`, Django REST, `?format=json` |
| Monetización por afiliación | Confirmada | Enlaces salientes con `utm_source=SOLOTODO` |
| Páginas de "más vistos" y "recomendados para ti" | Confirmada (ruta) | `/los_mas_vistos`, `/recomendados_para_ti` |
| **Historial o gráfico de precios** | **No verificada** | No hay ninguna marca de gráfico ni la cadena "histor" en el HTML servido de la ficha; `/products/{id}/prices/` y `/entities/{id}/prices/` responden 404 en la API |
| Comparador entre productos | No verificada | No se observó control ni ruta |
| Foro | No verificada | No apareció en la home ni en la ficha |
| Búsqueda por texto | No verificada | `robots.txt` prohíbe `/search`, así que no se consultó |
| Puntaje o reputación de tienda | No verificada | La página de tienda es solo cliente |

## Lectura para nuestro nicho

SoloTodo compite por cobertura y frescura en electrónica masiva. Para Gunpla y modelismo la ventaja no
está en tener más tiendas, sino en la **jerarquía del catálogo**: grado (HG, MG, RG, PG), escala (1/144,
1/100), número de kit y edición. Eso es exactamente lo que su principio 1 y 2 resuelven, y es donde un
catálogo plano como el nuestro no puede responder "¿dónde está más barato el RG de este año?" sin
duplicados ni ambigüedad.

Lo que **no** conviene copiar de entrada: la API pública (no hay terceros que la consuman todavía) y la
agrupación de variantes a tres niveles (es cara; sirve cuando hay volumen y conflictos de duplicados
reales).

Lo que sí, en este orden: veracidad de lo mostrado, oferta rica (condición, medio de pago, frescura) y
specs tipadas para Gunpla.
