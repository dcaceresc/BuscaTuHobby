# Web client

Angular 22 standalone, zoneless frontend. Use the repository's pnpm version; do not mix npm/yarn lockfiles.

## Quick start

Requirements: Node.js `^22.22.3`, `^24.15.0`, or `^26.0.0`, and pnpm `12.18.0` (see `package.json`). Angular 22 requires TypeScript 6.0.x; TypeScript 7 is not supported.

```bash
cd src/Web/WebUI
pnpm install --frozen-lockfile --strict-peer-dependencies
pnpm start
```

The HTTPS frontend runs at `https://localhost:4200`. Its prestart script exports the ASP.NET development certificate. On Linux, if the certificate export directory is missing, create it with restricted permissions first:

```bash
mkdir -p "$HOME/.aspnet/https"
chmod 700 "$HOME/.aspnet/https"
```

When `APPDATA` is set, the script uses `$APPDATA/ASP.NET/https` instead. Do not commit certificates or private keys. The browser may require trusting the local development certificate.

The API also starts the frontend through its SPA proxy:

```bash
dotnet run --project src/Web/WebAPI/WebAPI.csproj --launch-profile Project
```

Run that command from the repository root. Development API startup automatically migrates and seeds the configured database; do not point it at production data.

## Build

```bash
pnpm build
```

Production output is written to `dist/webui/browser`. The Angular build enforces the budgets in `angular.json`.

## Unit tests

Vitest and jsdom are development dependencies. Angular's native `@angular/build:unit-test` builder supplies Angular compilation and TestBed setup; no custom Vitest config or Jasmine/Karma/Zone setup is needed. Existing-project Vitest migration is marked experimental in the [Angular migration guide](https://angular.dev/guide/testing/migrating-to-vitest).

```bash
pnpm test                           # Watch mode
pnpm test:run                       # One run, suitable for automation
pnpm exec ng test --watch=false --include='**/app.component.spec.ts'
```

Specs live alongside code as `*.spec.ts`. Import `describe`, `it`, `expect`, and `vi` from `vitest` and use Angular TestBed for component/service tests. The app-shell smoke test renders the real router outlet in jsdom. Passing it establishes runner integration, not full application coverage.

## Dependency updates

Keep Angular framework packages aligned; CLI/build versions may have a different patch number. Upgrade `@ng-bootstrap/ng-bootstrap`, `@ng-select/ng-select`, and its Angular CDK peer together with Angular. Validate with strict peer checks, unit tests, and a production build. Preserve `pnpm-lock.yaml` and run the official Angular update migrations for major upgrades.

`pnpm-workspace.yaml` pins only Oxc 0.156.0's WASM runtime to 1.2.5: its binding requires emnapi alpha.6, while runtime 1.2.6/1.2.7 accepts different prereleases. This is a temporary upstream compatibility pin, not a disabled peer check. Remove it once the upstream binding/runtime versions agree. The same file preserves the project's existing build-script approvals and its explicit `core-js` denial. Review lifecycle-script permissions when updating dependencies.
