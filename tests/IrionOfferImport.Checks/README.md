# Irion existing/new-product import, inventory and dashboard checks

Run from the repository root with installed dependencies and the normal HOME; do not provision a clean HOME or install/restore missing dependencies for T5:

```sh
timeout 180s dotnet run --project tests/IrionOfferImport.Checks/IrionOfferImport.Checks.csproj --no-restore
```

If assets or dependencies are unavailable, stop and report the limitation. T5 runs this command once, not a production/test correction cycle or permission to retry failures.

This dependency-free console suite checks public inventory behavior, manual create/update handlers using async query fakes, EF SQL Server model metadata, migration operations, generated Up/Down SQL and snapshot/target consistency. It never opens a connection or performs HTTP requests. Fake save counters are not persistence or concurrency proof. SQL Server migration application/rollback, data and duplicate preservation, real ID/name uniqueness, rowversion, concurrent new-product imports and product/category/inventory persistence without orphans still require T6's accessible, authorized isolated relational fixture. No API startup, migration application, live HTTP or database connection is part of these checks; Development API startup automatically migrates and seeds (see the [operational warning](../../docs/irion-offer-import.md#antes-de-arrancar-el-bootstrap-escribe-en-la-base)).

Current T5 integration evidence: **980** offer assertions, **112** extractor assertions, **62** frontend tests in **5** files and both builds exit 0. Earlier T4 focused evidence is **56** tests in **3** files, not separately rerun in T5. API build: 0 warnings/errors; frontend build: initial bundle 625.68 kB and register SCSS 2.26 kB budget warnings. Full commands and five ignored `t5-new-product-*.log` paths are in the [integration guide](../../docs/irion-offer-import.md#comprobaciones-deterministas-sin-api-ni-sql-server). This passive documentation update changes no source or tests. T2/T4 new-product independent source reviews are accepted without runtime reruns; T5's local milestone does not close T6 or establish production readiness.

The supported UI keeps the existing-product flow and adds explicit **Crear producto nuevo** → **Revisar vinculación** → **Crear producto y guardar oferta**. Only the editable title is prefilled. Active manufacturer/franchise, matching optional series, categories, release date, explicit base choice, age, size and description come from human input, never invented defaults or reference IDs. Images are optional, never automatically downloaded; the old manual image workflow is unchanged. A stable GUID survives uncertain responses and manual reinspection; retries require full metadata/category/offer identity equality, never adoption by name. Existing unique `ProductName` remains authoritative: choose an existing product or review the name on conflict. Confirmation stages Product, ProductCategories and Inventory with one save; fake checks do not prove actual SQL atomicity. There is no automatic save/retry, crawl, new catalog-reference creation or price history.

The migration and snapshot were prepared manually because dotnet-ef was unavailable. Its conventional frozen designer was produced offline from the prepared snapshot:

```sh
dotnet run --project tests/IrionOfferImport.Checks/IrionOfferImport.Checks.csproj --no-restore -- --prepare-metadata
```

That explicit maintenance mode only writes `src/Infrastructure/Data/Migrations/20260720000000_IrionOffer.Designer.cs` and refuses to overwrite different contents. It is not part of normal checks and must not be used to refresh an existing migration after future model changes. Review the migration before any application. Down removes the newly added metadata; it does not remove inventory rows.

`PriceHistoryChecks` covers the price-history slice: an offer whose price, original price, discount or availability changes appends exactly one non-baseline point with the post-change values, while an identical observation, an activation toggle or an import that only refreshes metadata appends none, and creating an offer appends none. It also pins the model metadata (entity, `(InventoryId, ObservedAtUtc)` index, cascading relationship) and the hand-written `20260721000000_InventoryPriceHistory` migration (table, index, idempotent baseline seed for active offers, table-dropping `Down`, generated SQL, target model). The same explicit maintenance mode freezes any newer hand-written migration by passing its id and class: `-- --prepare-metadata 20260721000000_InventoryPriceHistory InventoryPriceHistory`; with no arguments it still targets `IrionOffer`. These checks use fakes and model metadata only, so they do not prove SQL translation, that the seed SQL runs on a real server, or that the migration applies and rolls back against SQL Server.

`FidelityChecks` covers the truthful-dashboard slice. Popular-category percentages divide by every active
category, so the four shown rows still add up to less than 100 and the first row divides by the whole instead
of by those four rows (five non-empty active categories), and a product linked to two categories yields two
rows, each below 100. The activity feed applies the offers eligibility, including `Availability != false`,
so an unavailable offer is never announced. The price-history series keeps every recorded point of the
product — from an offer that is now disabled, from a closed store and with `Availability == false` — trimmed
only by the `days` window and never by another product's observations; that asymmetry with the offers table
is deliberate and the interface must explain it. The same file pins the single image-folder resolution:
`SiteConfig.ResolveFolderImage` resolves a relative folder against the content root and keeps an absolute one
as is, the configuration load is the only place that resolves it, and `UtilityService.SaveImagen` writes into
that absolute folder instead of the process working directory (asserted by writing a real file into a
temporary folder outside the repository from a different working directory). These checks use fakes, offline
SQL generation and that temporary folder; they cannot prove that a real HTTP request serves an image, nor
that a real deployment's content root differs from its working directory.

`CatalogNavigationChecks` covers the Gunpla catalog slice: the seven integer grade values and their Spanish
facet labels, the entity defaults (`None`, null scale, null part number), the exact rejection messages for an
undefined grade, a blank scale/part number and both length caps, `Update` requiring the three arguments with no
default, the EF model columns, the hand-written `20260723000000_ProductGradeScaleAndPartNumber` migration
(with its `Down` and generated SQL), the frozen designer, the maintainer write side (grade range and lengths
rejected with the exact Spanish message and nothing persisted, every existing field and message preserved) and
both public listings against an in-memory fixture: the offer eligibility rule applied to items, prices, store
counts and store aggregates, the unknown/inactive/blank slug failure on both endpoints, the paging clamps and
the pre-paging total, each sort order with offerless products last, every filter and their combination, and
the three facet lists with their counts. The fixture is LINQ-to-Objects, so these checks do not prove that
SQL Server translates or executes the price-sorted join, the facet grouping or the projection.

`IngestionChecks` covers the catalog-identity slice (T2b): the nullable `Product.Gtin` with its hand-written
`20261116000002_ProductGtin` migration (single additive column, column-dropping `Down`, generated SQL, no index by
the documented decision that a barcode may legitimately repeat, newest-target and snapshot consistency), title
normalization, grade/scale parsing (including that `[Preventa] GUNDAM EX Gundam Universe` declares neither), the
create/reuse/park decision with its exact Spanish reasons, the shared on-demand `Sin clasificar` franchise, and the
`ImportRunProcessor` pass over queued items: SKU, barcode and title reuse, creation of manufacturer/product/offer/GTIN,
the change-only price recorder, parked items that publish nothing, second-pass idempotency, budget and 429/robots
stops, and cancellation. Fakes conflate tracked and stored state, so cancellation is asserted against a JSON snapshot
taken at the last save; no SQL server, HTTP request or database is used.

`ClassificationChecks` covers the T2d classification: the fixed Spanish vocabulary (four existing categories with
their current name, icon and order, plus `mangas`, `juegos-de-mesa` and `libros`), the explicit precedence levels
(store product type, then tags, then title, then a single declared fallback), the generic categories that never hide a
concrete signal, multiple categories only when the same winning level declares several, accent/case normalization,
and that no input yields an undeclared slug. It also prints a dry classification of the 20 products (40 offers)
recorded from the local catalog: only the title is available for rows already stored, because tags and product type
were never persisted, so the four manga volumes stay in the fallback until their sheets are read again. The ingestion
side is exercised with fakes: created products get their category, reused products without one get it, a human
category is never removed or completed, existing categories are reused by slug without renaming or reordering, a
declared category whose order is taken uses the next free one, only declared categories are created, and a second
pass adds no duplicate `ProductCategory` links. No SQL, HTTP or database is used.

Current full-suite evidence: **1513** assertions and exit 0 for
`dotnet run --project tests/IrionOfferImport.Checks/IrionOfferImport.Checks.csproj`; the extractor suite reports
**208** assertions (previously 1440 and 205).

`Inventory.CreateImportedOffer` and `UpdateImportedOffer` accept already server-validated whole integer prices
and a canonical HTTPS source identity. Import updates require an existing matching source and variant; unlinked legacy offers cannot be adopted. Availability is independent of administrative activation. Strict numeric validation applies only to import methods; manual create/update preserve baseline numeric semantics, including zero prices and inconsistent reference/discount tuples. Public-handler preservation checks cover both accepted responses and exact row/save effects. Legacy/manual metadata remains null; manual same-pair price edits preserve import metadata and activation, while imported association changes are rejected atomically.

T2 inspection public-handler checks cover active/missing product and store, exact root Irion store URLs, canonical source and positive Int64 variant metadata, typed Create/Update/Conflict results, legacy and external identity conflicts, disabled inventory preservation, base64 rowversion, safe errors and cancellation propagation. Every path asserts unchanged stored fields and save counts; accepted inspections perform exactly one pair lookup. The handler has no extractor/HTTP dependency. Existing manual successful behavior checks remain unchanged.

`NewProductChecks` isolates the new-product options/inspection branch from the original fixtures. It covers explicit base/date and complete required metadata, boundaries, active catalog references and series/franchise matching, unique categories, stable-ID/full-metadata/category-set retry conflicts, same source/store/variant identity, inactive products, images added later, current inventory version and disabled preservation. Options return only active references with labels and usable series/franchise IDs. Both handlers are read-only with no extractor dependency; every inspection checks tracked state, separate serialized persisted product/category/inventory snapshots and zero saves. The fixture explicitly demonstrates that tracked product/category edits do not change its stored snapshot. This is not a SQL translation, collation, authorization-runtime or atomic-save proof. Existing `Product.Create` permissive fields and generated-ID semantics remain pinned. New-product confirmation reuses `NewIrionProductValidation.InspectAsync` before/after extraction and checks the tracked product before staging.

`NewProductConfirmationChecks` exercises the public new-product handler: all three entity groups staged at the first/only save, exact manual fields/stable ID/category links, no images, selected-not-first server values and UTC, rounding/boundaries, identical retry, disabled update without product/category mutation, metadata/category/name/occupied-ID conflicts, invalid input and references, external snapshot/identity changes and vanished variants, safe 429/timeout/generic failures, cancellation and catalog changes during extraction. Simulated database/concurrency failures make one save attempt and leave the separate serialized persisted graph unchanged even though tracked entities remain staged. `NewProductContext` captures products/categories/inventory together only after a successful fake save. This is orchestration evidence, not relational atomicity/race proof; T6 remains required. No network, filesystem image writes or database connections are used.

T2 confirmation public-handler checks cover create/update, disabled preservation, selected-not-first variant values, server UTC metadata, whole CLP boundaries and discount rounding, exact snapshot refreshes, missing variants, active selections and identity/version changes around external I/O, repeated confirmations, invalid selection/identity/version/price, 429/URL/timeout/cancellation, token forwarding and simulated concurrency/database save failures. Every rejection checks the response and extraction/save counters; the fake captures a separate serialized persisted snapshot only on successful SaveChanges. A failing save may leave tracked objects changed: the unchanged fake persisted snapshot proves orchestration only, not SQL atomicity. There are no HTTP or database connections.

T3 exercises all four public dashboard handlers and ApiResponse envelopes with bidirectional product/store/inventory graphs. Only administratively active inventory for an active product and active store with `Availability != false` contributes prices, discounts, counts or store names. Null availability retains historical eligibility. Checks cover ordinary available and legacy offers, sold-out offers, disabled inventory/store/product, exact aggregates and names, ordering and existing limits, blank/non-matching search, unchanged descriptive fields, no writes and empty-offer catalog fallbacks. Store counts use distinct store IDs; no new shared production helper changes unrelated query families. Consequently no old/new differential for an unrequested command is required. The async query fixture supports ordered projections and enumeration, but uses LINQ-to-Objects: it does not prove SQL translation or execution of the dashboard projections. The suite's SQL/model assertions concern migration structure only; dashboard SQL execution remains part of T6's blocked relational proof.

The confirmation DTO exposes Saved/AlreadySaved with outer success true and RefreshRequired/Conflict with outer success false and non-null typed data. Validation/extraction errors have data null. EF uses the originally loaded rowversion automatically for SaveChanges concurrency checks; the manual DTO contracts are unchanged. The unique pair index covers every row, including disabled rows. Real SQL race, migration and authorization-runtime proof remain outside this runner.
