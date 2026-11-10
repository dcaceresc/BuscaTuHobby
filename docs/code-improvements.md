# Prioritized code improvements

Start with product/category correctness and session refresh handling, then add regression coverage before broader refactoring. These are proposals, not fixes included in the dependency upgrade. Static findings need focused reproduction before implementation; no application/database mutation was used for this assessment.

## Highest priorities

| Priority | Finding and evidence | Proposed change | Acceptance check |
| --- | --- | --- | --- |
| P0 | Reversed category-link arguments in `src/Application/Maintainer/Products/Commands/UpdateProduct/UpdateProduct.cs:41`. `ProductCategory.Create` expects `(productId, categoryId)` in `src/Domain/Entities/ProductCategory.cs:17`. | Pass product and category identifiers in factory order. Confirm against a disposable database before changing production behavior. | Edit a product to add a category with a different GUID; verify the correct product/category relationship, success response, and preservation of existing categories. |
| P1 | Refresh handling in `src/Web/WebUI/src/app/core/interceptors/authorize.interceptor.ts:17,45–54` retries on 401 without excluding refresh/login or coordinating concurrent requests. `src/Web/WebUI/src/app/core/services/security/auth.service.ts:80` uses the same HttpClient for refresh. | Exclude authentication endpoints, bound retries, and use one shared in-flight refresh for simultaneous failures. | A failed refresh causes one controlled sign-out, not recursion; simultaneous protected-request 401s trigger one refresh and replay each request once. |
| P1 | Product creation saves before category assignment, then saves again in `src/Application/Maintainer/Products/Commands/CreateProduct/CreateProduct.cs:29–40`; no surrounding transaction is visible in that handler. | Validate related IDs first and use one atomic unit of work, or an explicit transaction where required. | Simulate failure during category assignment; assert a failure response and no partial product/link records in isolated storage. |
| P1 | The current frontend suite has one app-shell integration test; no .NET test project is defined in the documented solution. The runner is now functional, but coverage is intentionally minimal. | Add backend handler/integration tests and targeted Angular service/interceptor/form tests; automate builds and test runs in CI. | Pin the first three findings with public-interface regressions; run tests on a disposable database without changing developer data. |

## Maintainability and contracts

| Priority | Finding and evidence | Proposed change | Acceptance check |
| --- | --- | --- | --- |
| P2 | `src/Application/Maintainer/Products/Queries/GetProducts/GetProducts.cs:33–35` catches all exceptions without structured logging and returns English text despite the project's Spanish message convention. Similar patterns occur in create/update handlers. | Add structured diagnostics and treat cancellation separately; reconcile any handler-policy change with `AGENTS.md` first. Do not expose internal exception details to clients. | A cancelled request is not reported as a normal server failure; unexpected errors produce correlated logs and safe Spanish responses. |
| P2 | The product list query in `src/Application/Maintainer/Products/Queries/GetProducts/GetProducts.cs:14–29` materializes the whole catalog without deterministic ordering or pagination. | Design bounded paging/filtering and explicit ordering; inspect the generated SQL before removing Includes around DTO projection. | Paging is stable with duplicate names; filtering and total counts agree; empty and out-of-range pages have defined responses. This changes an API contract and needs a separate decision. |
| P2 | `src/Application/Maintainer/Products/Commands/UpdateProduct/UpdateProduct.cs:9` requires `Guid SerieId`, while the create command and `src/Domain/Entities/Product.cs:26` permit `Guid?`. | Align update API, DTO, form, and domain optionality after confirming intended behavior. | Products without a series can be created and edited without sending an all-zero GUID; invalid series remain rejected. |
| P2 | `src/Web/WebUI/src/app/core/services/security/auth.service.ts:21,106–114` parses persisted/JWT data without guarding malformed inputs and returns unvalidated role claim shapes. | Guard storage parsing, decode base64url correctly, normalize role claims, and tighten runtime/type boundaries. | Corrupted storage or malformed tokens fail safely; a string and an array role claim normalize to the expected `string[]`. Client-side decoding never substitutes for server token validation. |

## Build and developer experience

| Priority | Observed evidence | Proposed change | Acceptance check |
| --- | --- | --- | --- |
| P2 | Production build passes but reports an initial bundle of **703.47 kB**, above its **500 kB warning threshold**; registration styles are **2.25 kB**, above the **2 kB warning threshold**. It also flags the SweetAlert2 CommonJS import in `src/Web/WebUI/src/app/core/services/notification.service.ts`. | Analyze bundle composition, duplicate imports, lazy-loaded boundaries, and component CSS. Check the documented ESM entry point before changing imports. | Compare production artifacts and behavior; meet existing budgets without raising or disabling them. |
| P2 | Bootstrap/Sass compilation reports deprecated Sass APIs and color combinations that cannot reach **4.5:1** contrast against `#6c757d`. Global styles are in `src/Web/WebUI/src/styles.scss`. | Modernize app-owned Sass and review design tokens/contrast; distinguish upstream Bootstrap warnings from application code. | Keep visual/behavior checks, measure contrast, and reduce warnings without silencing the compiler. |
| P2 | `src/Web/WebUI/aspnetcore-https.js` exports certificates without ensuring the target directory exists; this caused the reported first-run failure. | Create only the intended certificate directory with restrictive permissions, handle export errors, and preserve existing certificate files. | In isolated temporary HOME/APPDATA directories, first run succeeds; failure is explicit; existing keys/certificates remain unchanged. Never use real developer keys in tests. |
| P2 | `src/Web/WebUI/pnpm-workspace.yaml` contains a scoped Oxc WASM runtime compatibility override, required by the current published dependency peers. The installed WASM loader emits Node's experimental WASI warning. | Monitor upstream parser/runtime alignment and remove only this temporary override when a compatible release exists. | Strict frozen install, direct WASM parsing, Angular tests, production build, and audit pass without the override. |

## Verified upgrade baseline

- Backend remains `net10.0`: Microsoft/EF packages `10.0.15`, Swashbuckle `10.3.1`; Carter `10.0.0`, Scrutor `7.0.0`, and GuardClauses `5.0.0` were already current in the queried registries.
- Frontend: Angular `22.2.5`, CLI/build `22.2.6`, TypeScript `6.0.3`, Vitest `5.1.0`, jsdom `31.0.0`. TypeScript 7 is deliberately excluded by Angular's peer contract.
- Full solution build and frontend production build pass. Vitest runs one app-shell DOM test. The earlier clean Infrastructure compilation reported three nullable-property warnings in `src/Application/Dashboard/Queries/GetMenuCategories/MenuCategoryDto.cs`.
- Backend vulnerable-package inventory and frontend audit report no known vulnerabilities from their configured sources after the compatible transitive refresh. This is a point-in-time advisory check, not a complete security audit.
- API startup, database migrations/seeding, end-to-end browser flows, and backend functional tests were not executed. SDK/runtime installation was not changed: updating the local .NET 10 tools/runtime remains an environment maintenance task, separate from project package upgrades.

## Independent verification limitation

Structural compatibility and preservation checks passed. A separately authorized isolated functional pass compiled .NET successfully (three nullable warnings), but both frontend commands timed out after Sass AOT snapshot/runtime errors and executed zero tests. The Oxc probe loaded the WASM target and parsed valid syntax without diagnostics, then failed in its AST assertion before testing invalid syntax. These results do not reproduce a defect in the original working candidate, whose frontend checks passed, but independent frontend/parser verification remains incomplete. No automatic retries or code corrections were performed. The temporary environment is retained; its tool first-use/install side effects are recorded in the feature task log.

## Suggested order

1. Reproduce and fix category assignment in a disposable environment.
2. Add refresh-flow regression tests and fix bounded/single-flight renewal.
3. Make product creation atomic and align optional series behavior.
4. Add CI and expand regression coverage before query/API or styling work.
