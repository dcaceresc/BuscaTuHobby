# Ingesta multi-tienda: de un importador de Irion a catálogo real chileno

Generalizar la importación a cualquier tienda Shopify, descubrir el catálogo por sitemap y automatizar la
ingesta para que la base deje de vivir de datos inventados. Diseño completo en `docs/ingesta-multitienda.md`.

## Specs

S1. Original request, verbatim: "lo que hay que agregar es que lo de irion deberia ser para cualqueir pagina y que sea automatizado, actulemtne la base de dato solo tiene datos inventados es necesaria llenarla con datos reales puedes buscar una lsita de sitios similares a irion para completar y generar un propuesta para automatizar ingresar su data".

S2. Scope correction, verbatim: "el sitio no es solo de gunpla busca tiendas solo chilenas". The first two sources are Chilean: `irion.cl` (already working) and `luffytoys.cl` (Shopify, 26 Gunpla URLs in its first product sitemap). Non-Chilean stores are secondary and out of the first phase.

S3. Store identity is configuration, not code. Host, currency and connector key live in the `Store` row; adding a second Shopify store must require **no code change**. `Irion` keeps working as the first configured row, not as a special class.

S4. Discovery comes from the platform's product sitemap (`sitemap.xml` → `sitemap_products_*.xml`), never from guessing URLs. Every run has an explicit request budget per store and never exceeds it.

S5. `robots.txt` is fetched once per host, cached, and obeyed: a disallowed path is skipped and recorded as skipped. The crawler identifies itself with a stable User-Agent.

S6. Politeness is enforced, not assumed: one request at a time per domain, `Retry-After` respected, exponential backoff on 429/5xx, no retries on 4xx, and no infinite loops.

S7. A run is auditable, idempotent and resumable: it records what it fetched, what it stored and what it rejected; re-running with unchanged prices writes no new price-history row (the recorder is already change-only).

S8. Real data enters only through connectors. The development seed stays Development-only demo data and is never mixed with real offers.

S9. Existing contracts stay additive: no existing endpoint changes shape or message, and the manual per-URL import keeps working unchanged during the transition.

S10. Evidence discipline: any claim about a live store must come from a real request. An environment-caused failure (the `429 local_rate_limited` seen across unrelated domains from this sandbox) is recorded as an environment limit, never as the store's behaviour.

## Tasks

- T1 [done] S3-S6,S9 | connector per platform with store configuration, `robots.txt` gate and per-domain limiter, proven with a second configured Shopify store | route: bounded worker + parent verification | Risk: item 6 | commit: b0f4df8
- T2 [pending] S4,S7 | product-sitemap discovery plus `ImportRun`/`ImportItem` with budget, idempotency and resume; first real import of `irion.cl` and `luffytoys.cl` | route: parent + bounded worker | Risk: item 1 | commit: pending
- T3 [pending] S2-S3 | generic JSON-LD connector for the Chilean stores that are not Shopify (`pasatime.cl`, `svadercoleccionables.cl`, `otakuverso.cl`, `titoys.cl`) | route: bounded worker | Risk: item 6 | commit: pending
- T4 [pending] S6-S7 | scheduled ingestion, coverage/freshness/conflict metrics and price-drop alerts derived from the existing history | route: bounded worker | Risk: item 6 | commit: pending
- T5 [pending] S3,S7 | catalog identity: GTIN plus base-model/variant modelling so two stores can be recognised as the same kit | route: parent decision first | Risk: item 1 | commit: pending

## Log

L1. Original request, verbatim: "lo que hay que agregar es que lo de irion deberia ser para cualqueir pagina y que sea automatizado, actulemtne la base de dato solo tiene datos inventados es necesaria llenarla con datos reales puedes buscar una lsita de sitios similares a irion para completar y generar un propuesta para automatizar ingresar su data".
L2. Scope correction from the same turn, verbatim: "el sitio no es solo de gunpla busca tiendas solo chilenas".
L3. Repository facts before planning: the "Irion extractor" is a Shopify product-JSON extractor (fetches `{url}.js`, reads `variants[]` with `sku`, `barcode`, `available`, `price`, `compare_at_price`); store-specific code is only the pinned host, the host regex, the hardcoded `CLP` and the `IIrionProductExtractor` signature. There is no catalog discovery, no `robots.txt` handling and no per-store connector configuration anywhere in `src/`; `Store.StoreWebSite` is the only per-store surface. The offer already carries `SourceUrl` + `ExternalVariantId` + `ObservedAtUtc`, and `(StoreId, ProductId)` is unique. `Product.PartNumber/Grade/Scale` exist; there is no GTIN column, so Shopify's `barcode` is discarded.
L4. Research, verified 2026-11-16 with real requests, saved in `docs/ingesta-multitienda.md`. Chilean stores found and classified: Shopify — `luffytoys.cl` (996 product URLs in the first sitemap, 26 with Gunpla), `anezakistore.cl` (1001 URLs, 2 with Gundam), `friitoys.cl` (392 URLs), `animestore.cl`, `wyvern.cl`, `blasterchile.cl` (301 on sitemap); own platform with JSON-LD — `pasatime.cl` (Gunpla and modelismo), `svadercoleccionables.cl`, `otakuverso.cl`, `titoys.cl`, `adamantium.cl`, `jftoys.cl`, `hangar019.cl`; `html-selectors` — `hobbytoys.cl`, `koneko.cl`; discarded — `geekz.cl`, `top8.cl`. International references measured for contrast: `solarisjapan.com`, `usagundamstore.com` and `gundamplanet.com` are Shopify; `amiami.com` and `1999.co.jp` answer 403 Cloudflare; Falabella and Paris search pages are 1.7 MB and 3.8 MB of HTML with no `ItemList` JSON-LD.
L5. Environment limit recorded rather than hidden: `irion.cl` serves `robots.txt` (51 `Disallow`, all cart/account/admin/`cart.js`/`recommendations/products`/sorted-and-filtered collections) and `sitemap.xml` with 5 product sitemaps (the first with 1001 URLs), but `/products.json` and `/products/{handle}.js` answered `429 local_rate_limited`. The same 18-byte body appeared on six unrelated domains at the same time, so the limiter is this sandbox's egress, not the stores'. The live `.js` contract therefore stays **unverified from here**, and each store's real rate limit is unknown.
L6. Decision from the human: start with F1 (generalise the Shopify connector, `robots.txt` and the limiter) and prove it with a Chilean store, not an international one — hence `luffytoys.cl` as the second configured row.
L7. T1 done in `b0f4df8`. `IrionProductExtractor` became `ShopifyConnector` (key `shopify`); host, currency and request budget come from `Store.ConnectorKey`/`Store.ConnectorSettings`; `robots.txt` is fetched once per host and a disallowed path is refused without requesting the product; a per-domain limiter serialises requests, honours `Retry-After` and backs off exponentially on 429/5xx across at most three attempts, never on 4xx. Manual import keeps its routes, DTOs and messages. The gap the worker disclosed (no configured store row on a fresh database, because the seeder returned early and no row matched `irion.cl`) was closed by seeding `irion.cl` and `luffytoys.cl` as configured sources, and by inserting both rows into the local database.
L8. Verification by the parent, not only the worker's report: build 0 errors; extractor checks `PASS: 159 assertions` (from 112) and offer checks `PASS: 1260` (from 1230); migration `20261116000000_StoreConnectorConfiguration` applied, reverted and re-applied on a real SQL Server with column-level confirmation that `ConnectorKey`/`ConnectorSettings` appear and disappear; both rows configured in `BuscaTuHobby`. Live loads through the real connector: `irion.cl` returns `ADIVINA QUIÉN?` (Hasbro) with price 12990, original 19990, SKU `HAS-A5696` and barcode `6535699571882`; `luffytoys.cl` returns `[Preventa] GUNDAM EX Gundam Universe` (Bandai - Tamashii Nations) with 31990 and 39990, both with currency `CLP` taken from the row; a host without a configured store is refused without making any request. Slice boundary: the 2820-line commit is bulk because of the generated migration designer/snapshot and the rewritten extractor checks; the authored surface is the store configuration, the connector, the robots cache, the limiter and the settings parser.
L9. Deliberately left for T2 and recorded rather than implied: `Store.ConnectorSettings` accepts `requestsPerMinute` and `requestBudget` but nothing consumes the budget yet, `StoreContextProvider` is covered by fakes rather than by an EF query test, a huge `Retry-After` is honoured literally (bounded only by cancellation and the per-attempt timeout), and a transient `robots.txt` failure is treated as permitted and not recorded.
