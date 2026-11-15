# Irion inventory foundation and read-only inspection checks (T1/T2)

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

Confirmation is not exposed yet. The remaining T2 boundary must re-fetch and compare the confirmed external snapshot, reject non-whole/out-of-range prices before integer conversion, compare the expected `RowVersion` before mutation and handle `DbUpdateConcurrencyException`/unique constraint errors as recoverable typed conflicts. EF uses the originally loaded rowversion automatically for SaveChanges concurrency checks; the manual DTO contracts are unchanged. The unique pair index covers every row, including disabled rows.
