# Fidelidad con el comparador de referencia

Cerrar la brecha con la filosofía de SoloTodo en tres bloques, en el orden pedido: primero lo roto y lo
falso, después la oferta rica y el historial, después el catálogo de Gunpla.

Base de análisis: `docs/referencia-solotodo.md` (sitio verificado con peticiones reales) y
`docs/estado-del-sitio.md` (auditoría con evidencia). Branch `feat/fidelidad-comparador`; baseline
`34d492c`. Trabajo previo sin commitear preservado y nunca incluido en un commit: README.md,
`src/Infrastructure/Data/ApplicationDbContextFactory.cs`, `src/Web/WebAPI/WebAPI.csproj`,
`src/Web/WebAPI/appsettings.Development.json`, `mise.toml`.

## Specs

S1. Original request, verbatim: "la idea de este sitio es seguir la filosofia de solotodo.cl verifica el sitio y lista las funcionalidades que faltan, tambien quiero que listes las funcionalidades actuales que tiene el sitio que faltan o no funcionan correctamente".

S2. Original request, verbatim: "quiero que las las inventigaciones los guardes en archivos markdown, comienza con lo roto y falso y despues haz los otros 2". The two investigations are saved as `docs/referencia-solotodo.md` and `docs/estado-del-sitio.md`. The three blocks are implemented in that order: broken and false first, then the offer and history block, then the Gunpla catalog block.

S3. Block 1 — nothing in the interface may lead nowhere or claim something untrue:
- every visible link or button resolves to a real route, or stops being a link;
- a wildcard route answers unknown URLs instead of breaking navigation;
- no badge, percentage, counter label or ranking claim is derived from list position or from a subset of the data presented as the whole;
- the seeder is development-only and is documented as demo data, not treated as real catalog;
- application folders with no handler behind them are removed rather than left to fail at runtime;
- the price history respects the same offer eligibility as the offers list.

S4. Block 1 — security and reachability: `/security/**` is guarded in the frontend like it already is in the API, and uploaded product images are served from the configured folder instead of a working-directory-relative path.

S5. Block 2 — the offer carries what a buyer needs to compare, as SoloTodo does: product condition (new, refurbished, used), payment method label, and a visible freshness timestamp per offer. The chart shows the observed maximum as a series, not as a flat reference line, and its `days` window is reachable from the interface.

S6. Block 3 — the catalog can answer "where is this kit cheapest" without ambiguity: a Gunpla grade (HG, MG, RG, PG and similar), a structured scale and a manufacturer part number on the product, with public navigation by category and by store so the catalog can be browsed instead of only searched.

S7. Existing contracts stay additive: no existing endpoint changes shape or message, and no existing screen changes its number or date formatting.

S8. Commit messages are written in Spanish with a subject and a descriptive body.

## Tasks

- T1 [done] S2 | save both investigations as markdown | route: parent inline | Risk: none | commit: pending
- T2 [pending] S3 | honest navigation: dead links, wildcard route, admin guard | route: bounded worker | Risk: none | commit: pending
- T3 [pending] S3 | truthful widgets: search label, badges, category percentages, inactive rows, placeholder URL | route: bounded worker | Risk: none | commit: pending
- T4 [pending] S3 | remove application folders with no handler, remove dead commented blocks | route: bounded worker | Risk: none | commit: pending
- T5 [pending] S3 | price history eligibility parity with the offers list | route: bounded worker | Risk: item 3 | commit: pending
- T6 [pending] S4 | serve product images from the configured folder | route: bounded worker | Risk: item 6 | commit: pending
- T7 [pending] S3 | clickable highlighted offers | route: parent inline | Risk: none | commit: pending
- T8 [pending] S3-S4 | checks and specs for everything touched in block 1 | route: bounded worker + independent verifier | Risk: item 6 | commit: pending
- T9 [pending] S5 | offer condition, payment method and visible freshness, with migration | route: pending | Risk: item 1 | commit: pending
- T10 [pending] S5 | chart maximum as a series and a reachable day window | route: pending | Risk: none | commit: pending
- T11 [pending] S6 | Gunpla grade, structured scale and part number, with migration | route: pending | Risk: item 1 | commit: pending
- T12 [pending] S6 | public navigation by category and by store | route: pending | Risk: none | commit: pending

## Log

L1. Original request, verbatim: "la idea de este sitio es seguir la filosofia de solotodo.cl verifica el sitio y lista las funcionalidades que faltan, tambien quiero que listes las funcionalidades actuales que tiene el sitio que faltan o no funcionan correctamente". Answered read-only with a verified comparison.
L2. Original request, verbatim: "quiero que las las inventigaciones los guardes en archivos markdown, comienza con lo roto y falso y despues haz los otros 2". Authorization to implement all three blocks in that order.
L3. Reference site verified 2026-11-16 with real HTTP requests (home, a 236-result category listing with facets, a product detail page, robots, sitemap, and the public API with 182,331 products). Confirmed there: typed per-category specifications, base model and variant identity, part number, per-offer condition and payment method, normal-price strike-through, price-drop alert, product opinions, store comments route, related products, freshness timestamp, public API and affiliate links. Not confirmed: a price chart (`/products/{id}/prices/` returns 404 and no chart markup is served) and any product comparator.
L4. Own audit reclassified while writing the docs: the fake catalog, hand-set counters and fabricated posts come from `ApplicationDbContextInitialiser`, which runs only in Development (`Program.cs:24`), so they are demo data rather than a lie to production users. The real code defects are the ones that would also be wrong with real data: the search label, the position-based badges, the percentage over the top four, the unfiltered activity feed and the placeholder query string.
L5. Known environment limit, same as the previous feature: no isolated SQL Server, no `dotnet-ef` and no `sqlcmd`, so any migration in T9 and T11 can be written and reviewed structurally but cannot be applied or rolled back here. That stays an explicit unproven item.
