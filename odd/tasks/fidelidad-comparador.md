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

S9. Requirement correction, verbatim: "los comentarios en el codigo deben estar en español agrego en el agent.md". Every code comment in every language and file type (C#, TypeScript, SCSS, HTML) is written in Spanish, and the rule is recorded in AGENTS.md. Comments introduced by this feature work are translated; code behaviour, identifiers, test names and user-facing strings are untouched. Commented-out code blocks are deleted rather than translated.

## Tasks

- T1 [done] S2 | save both investigations as markdown | route: parent inline | Risk: none | commit: f86d26c
- T2 [done] S3 | honest navigation: dead links, wildcard route, admin guard scoped like the API | route: bounded worker + parent correction | Risk: none | commit: pending
- T3 [done] S3 | truthful widgets: search label, badges, category percentages, inactive rows, placeholder URL | route: bounded workers | Risk: none | commit: pending
- T4 [done] S3 | remove application folders with no handler, remove dead commented blocks | route: bounded workers | Risk: none | commit: pending
- T5 [done] S3 | price history eligibility parity with the offers list | route: bounded worker | Risk: item 3 | commit: pending
- T6 [done] S4 | serve product images from the configured folder and publish `/images/{id}.jpg` | route: bounded worker + parent | Risk: item 6 | commit: pending
- T7 [pending] S3 | clickable highlighted offers | route: parent inline | Risk: none | commit: pending
- T8 [pending] S3-S4,S9 | checks, specs and Spanish comments for everything touched in block 1 | route: bounded workers + parent | Risk: item 6 | commit: pending
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
L6. Requirement correction, verbatim: "los comentarios en el codigo deben estar en español agrego en el agent.md". Recorded as S9 and added to AGENTS.md as a project exception to the English default for technical artifacts. The comments introduced by this feature work are translated; the code stays byte-identical apart from comment text.
L7. Block 1 workers returned. Backend: percentages now divide by every active category, activity filters each source, price history applies the offers eligibility predicate, `BestDealDto` gained `ProductId`, `GetProductById` publishes `/images/{id}.jpg`, `Program.cs` maps the configured folder at `/images`, and the five handler-less Favorites/Reviews stubs were deleted after a whole-solution grep. Frontend: dead menu entries and both commented hardcoded blocks removed, categories and stores became non-interactive rows, `Perfil` removed, a wildcard route renders a new not-found page, and the most-searched card says "vistas" with no position-based badge.
L8. Two worker findings the parent corrected before committing: the security guard was applied to the whole shell including `auth`, which would have blocked the anonymous admin-login and confirm-email paths, so it now guards only `roles` and `users`, matching the API; and `GetProductDetail` still published the physical folder name while the new middleware serves `/images`, so it now publishes `/images/{id}.jpg` and its check follows.
L9. Accepted consequence of T5: filtering the series by the offer's current eligibility also removes points recorded while an offer was still active but is now disabled or unavailable. It is deliberate parity with the offers table, so the chart never shows a cheaper price the table refuses to list; it is documented as a visible data consequence instead of a hidden filter.
