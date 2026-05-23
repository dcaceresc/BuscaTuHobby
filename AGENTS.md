# AGENTS.md - BuscaTuHobby

## Project Overview

Full-stack web application for searching/comparing prices of Gunpla and scale models across online stores.
- **Backend:** C# / .NET 10 / ASP.NET Core Minimal API with Carter
- **Frontend:** TypeScript / Angular 21 (standalone components, zoneless)
- **Database:** SQL Server via Entity Framework Core 10
- **Architecture:** Clean Architecture (Domain -> Application -> Infrastructure -> WebAPI)

## Build & Run Commands

### Backend (.NET)

```bash
# Restore packages
dotnet restore

# Build entire solution
dotnet build

# Build a specific project
dotnet build src/Web/WebAPI/WebAPI.csproj

# Run the API (serves both API + Angular SPA in dev)
dotnet run --project src/Web/WebAPI/WebAPI.csproj

# Apply EF Core migrations
dotnet ef database update --project src/Infrastructure --startup-project src/Web/WebAPI
```

### Frontend (Angular)

All Angular commands run from `src/Web/WebUI/`:

```bash
# Install dependencies
npm install

# Serve with dev server (proxies /api to ASP.NET on https://localhost:7091)
npx ng serve

# Production build
npx ng build

# Run unit tests (Karma + Jasmine)
npx ng test

# Run a single test file
npx ng test --include=**/products.component.spec.ts

# Generate a component (defaults: OnPush, SCSS, standalone, no tests)
npx ng generate component features/maintainer/example
```

### No CI/CD, ESLint, or Prettier configs exist. No .NET test projects exist.

## Solution Structure

```
BuscaTuHobby.sln
src/
  Domain/            # Entities, value objects, ApiResponse. Zero dependencies.
  Application/       # CQRS handlers, interfaces, custom mediator, exceptions.
  Infrastructure/    # EF Core DbContext, migrations, service implementations, JWT config.
  Web/
    WebAPI/          # Minimal API endpoints via Carter modules, auth, Swagger.
    WebUI/           # Angular 21 SPA (standalone components, zoneless change detection).
```

## Architecture & Patterns

### Clean Architecture Layers
- **Domain**: Pure C# entities with private setters, static `Create()` factories, `Update()` methods, `ToggleActive()`. Inherits `AuditableEntity`. No framework dependencies.
- **Application**: CQRS via custom mediator (`IRequest<T>` / `IRequestHandler<TReq, TRes>` / `IRequestDispatcher`). Handlers auto-registered with Scrutor. Features organized as `Feature/Commands/` and `Feature/Queries/`.
- **Infrastructure**: EF Core DbContext, `AuditableEntityInterceptor`, JWT authentication, service implementations for interfaces defined in Application.
- **WebAPI**: Carter `ICarterModule` classes grouping endpoints. One module per entity (e.g., `ProductsModule`).

### API Response Pattern
All endpoints return `ApiResponse` or `ApiResponse<T>` (defined in `Domain.Common`). Use `IApiResponseService.Success(data)` and `IApiResponseService.Fail<T>(message)`.

### Validation
Use `Ardalis.GuardClauses` with custom guard extensions in `Application.Common.Exceptions.CustomGuards` (`NotFound`, `ForbiddenAccess`, `InvalidInput`).

## C# Code Style

### Naming
- **PascalCase** for all public members, properties, methods, classes.
- Entity properties prefixed with entity name: `ProductName`, `ProductId`, `ManufacturerName`.
- Navigation properties use entity name: `Manufacturer`, `Franchise`, `ProductImages`.
- Foreign keys: `ManufacturerId`, `FranchiseId`.
- **No `I` prefix** on classes; `I` prefix on interfaces only (`IApplicationDbContext`, `IRequestHandler`).

### Types & Patterns
- File-scoped namespaces: `namespace Application.Maintainer.Products.Commands.CreateProduct;`
- `record` types for requests/commands/queries: `public record CreateProduct : IRequest<ApiResponse<Guid>>`
- `class` for DTOs and handlers.
- Primary constructors for handlers: `public class CreateProductHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<...>`
- Assign primary constructor params to `private readonly` fields: `private readonly IApplicationDbContext _context = context;`
- Use `default!` for non-nullable properties initialized later: `public string ProductName { get; set; } = default!;`
- Use `Guid` for all entity IDs.
- Use `DateOnly` for dates without time.

### Imports
- Use `GlobalUsings.cs` per project for shared imports. Avoid repeating these in individual files.
- Specific imports at top of file only when not covered by global usings.
- Namespace matches folder structure exactly.

### Error Handling
- Wrap handler logic in `try/catch (Exception)` blocks.
- Return `_responseService.Fail<T>("message")` on failure -- never throw from handlers.
- Custom exceptions: `NotFoundException`, `ForbiddenAccessException`, `ArgumentException`.
- User-facing error messages in Spanish.

### Entity Design
- Private constructors, public static `Create(...)` factory methods.
- Private setters on all properties.
- Domain logic lives on the entity (e.g., `AssignCategory`, `AssignImage`, `ToggleActive`).

### File Organization (Application Layer)
Each command/query gets its own folder:
```
Application/Maintainer/Products/
  Commands/
    CreateProduct/
      CreateProduct.cs        # Contains both the record and the handler class
    UpdateProduct/
      UpdateProduct.cs
    ToggleProduct/
      ToggleProduct.cs
  Queries/
    GetProducts/
      GetProducts.cs          # Contains record + handler
      ProductDto.cs           # DTO for this specific query
    GetProductById/
      GetProductById.cs
      ProductByIdDto.cs
```

## TypeScript / Angular Code Style

### General
- Strict mode enabled (strict templates, strict injection, strict input access modifiers).
- **OnPush** change detection strategy on all components.
- **Standalone components** only -- no NgModules.
- **Zoneless** change detection (`provideZonelessChangeDetection()`).
- Lazy loading via `loadComponent` / `loadChildren` in routes.
- SCSS for styles. Bootstrap 5.3 + Bootstrap Icons for UI.

### Naming
- `kebab-case` for file names: `products.component.ts`, `product.service.ts`.
- `PascalCase` for classes, interfaces, types: `ProductDto`, `ProductService`.
- `camelCase` for properties and methods: `productName`, `loadProducts()`.
- Suffix convention: `.component.ts`, `.service.ts`, `.guard.ts`, `.interceptor.ts`, `.model.ts`.
- Component selector prefix: `app-` (e.g., `app-products`).

### Imports & Barrel Exports
- Path aliases: `@app/*` maps to `src/app/*`, `@src/*` maps to `src/*`.
- Barrel exports via `index.ts` in `core/models/`, `core/services/`, `shared/`.
- Import from barrels: `import { ProductDto } from '@app/core/models';`

### Dependency Injection
- Use `inject()` function (not constructor injection):
  ```typescript
  private productService = inject(ProductService);
  ```
- Services decorated with `@Injectable({ providedIn: 'root' })`.

### Signals
- Use Angular `signal()` for reactive state in components: `public data = signal<ProductDto[]>([]);`

### Error Handling
- Subscribe with `next`/`error` callbacks. Check `response.success` before using data.
- Use `NotificationService` for toast notifications (SweetAlert2 under the hood).
- User-facing messages in Spanish.

### Angular Project Location
All frontend code is in `src/Web/WebUI/`. The `angular.json`, `package.json`, and `tsconfig.json` are in that directory.

### EditorConfig (Frontend)
- UTF-8, 2-space indent, single quotes in TypeScript, final newline, trim trailing whitespace.
