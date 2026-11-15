# Irion inventory and dashboard checks (T1/T2/T3)

Run from the repository root:

```sh
dotnet run --project tests/IrionOfferImport.Checks/IrionOfferImport.Checks.csproj --no-restore
```

For a new runner assets file, use the existing package cache without configured package sources:

```sh
dotnet run --project tests/IrionOfferImport.Checks/IrionOfferImport.Checks.csproj -p:RestoreSources= -p:NuGetAudit=false
```

This dependency-free console suite checks public inventory behavior, manual create/update handlers using async query fakes, EF SQL Server model metadata, migration operations, generated Up/Down SQL and snapshot/target consistency. It never opens a connection or performs HTTP requests. Fake save counters are not persistence or concurrency proof. SQL Server migration application, rollback, duplicate preservation and simultaneous writes still require T6's isolated relational fixture.

The migration and snapshot were prepared manually because dotnet-ef was unavailable. Its conventional frozen designer was produced offline from the prepared snapshot:

```sh
dotnet run --project tests/IrionOfferImport.Checks/IrionOfferImport.Checks.csproj --no-restore -- --prepare-metadata
```

That explicit maintenance mode only writes `src/Infrastructure/Data/Migrations/20260720000000_IrionOffer.Designer.cs` and refuses to overwrite different contents. It is not part of normal checks and must not be used to refresh an existing migration after future model changes. Review the migration before any application. Down removes the newly added metadata; it does not remove inventory rows.

`Inventory.CreateImportedOffer` and `UpdateImportedOffer` accept already server-validated whole integer prices and a canonical HTTPS source identity. Import updates require an existing matching source and variant; unlinked legacy offers cannot be adopted. Availability is independent of administrative activation. Strict numeric validation applies only to import methods; manual create/update preserve baseline numeric semantics, including zero prices and inconsistent reference/discount tuples. Public-handler preservation checks cover both accepted responses and exact row/save effects. Legacy/manual metadata remains null; manual same-pair price edits preserve import metadata and activation, while imported association changes are rejected atomically.

T2 inspection public-handler checks cover active/missing product and store, exact root Irion store URLs, canonical source and positive Int64 variant metadata, typed Create/Update/Conflict results, legacy and external identity conflicts, disabled inventory preservation, base64 rowversion, safe errors and cancellation propagation. Every path asserts unchanged stored fields and save counts; accepted inspections perform exactly one pair lookup. The handler has no extractor/HTTP dependency. Existing manual successful behavior checks remain unchanged.

`NewProductChecks` isolates the new-product options/inspection branch from the original fixtures. It covers explicit base/date and complete required metadata, boundaries, active catalog references and series/franchise matching, unique categories, stable-ID/full-metadata/category-set retry conflicts, same source/store/variant identity, inactive products, images added later, current inventory version and disabled preservation. Options return only active references with labels and usable series/franchise IDs. Both handlers are read-only with no extractor dependency; every inspection checks tracked state, separate serialized persisted product/category/inventory snapshots and zero saves. The fixture explicitly demonstrates that tracked product/category edits do not change its stored snapshot. This is not a SQL translation, collation, authorization-runtime or atomic-save proof. Existing `Product.Create` permissive fields and generated-ID semantics remain pinned. Atomic new-product confirmation is the next boundary, not implemented here; its reusable read-only entry point is `NewIrionProductValidation.InspectAsync`.

T2 confirmation public-handler checks cover create/update, disabled preservation, selected-not-first variant values, server UTC metadata, whole CLP boundaries and discount rounding, exact snapshot refreshes, missing variants, active selections and identity/version changes around external I/O, repeated confirmations, invalid selection/identity/version/price, 429/URL/timeout/cancellation, token forwarding and simulated concurrency/database save failures. Every rejection checks the response and extraction/save counters; the fake captures a separate serialized persisted snapshot only on successful SaveChanges. A failing save may leave tracked objects changed: the unchanged fake persisted snapshot proves orchestration only, not SQL atomicity. There are no HTTP or database connections.

T3 exercises all four public dashboard handlers and ApiResponse envelopes with bidirectional product/store/inventory graphs. Only administratively active inventory for an active product and active store with `Availability != false` contributes prices, discounts, counts or store names. Null availability retains historical eligibility. Checks cover ordinary available and legacy offers, sold-out offers, disabled inventory/store/product, exact aggregates and names, ordering and existing limits, blank/non-matching search, unchanged descriptive fields, no writes and empty-offer catalog fallbacks. Store counts use distinct store IDs; no new shared production helper changes unrelated query families. Consequently no old/new differential for an unrequested command is required. The async query fixture supports ordered projections and enumeration, but uses LINQ-to-Objects: it does not prove SQL translation or execution of the dashboard projections. The suite's SQL/model assertions concern migration structure only; dashboard SQL execution remains part of T6's blocked relational proof.

The confirmation DTO exposes Saved/AlreadySaved with outer success true and RefreshRequired/Conflict with outer success false and non-null typed data. Validation/extraction errors have data null. EF uses the originally loaded rowversion automatically for SaveChanges concurrency checks; the manual DTO contracts are unchanged. The unique pair index covers every row, including disabled rows. Real SQL race, migration and authorization-runtime proof remain outside this runner.
