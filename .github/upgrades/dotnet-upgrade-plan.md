# .NET 10.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that an .NET 10.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 10.0 upgrade.
3. Upgrade src/Domain/Domain.csproj
4. Upgrade src/Application/Application.csproj
5. Upgrade src/Infrastructure/Infrastructure.csproj
6. Upgrade src/Web/WebUI/WebUI.esproj
7. Upgrade src/Web/WebAPI/WebAPI.csproj

## Settings

This section contains settings and data used by execution steps.

### Excluded projects

Table below contains projects that do belong to the dependency graph for selected projects and should not be included in the upgrade.

| Project name                                   | Description                 |
|:-----------------------------------------------|:---------------------------:|

### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name                                   | Current Version              | New Version | Description                                        |
|:-----------------------------------------------|:----------------------------:|:-----------:|:---------------------------------------------------|
| Microsoft.AspNetCore.Authentication.JwtBearer  | 8.0.18                       | 10.0.0      | Recommended for .NET 10.0                          |
| Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore | 8.0.18                  | 10.0.0      | Recommended for .NET 10.0                          |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 8.0.18                    | 10.0.0      | Recommended for .NET 10.0                          |
| Microsoft.AspNetCore.OpenApi                   | 8.0.18                       | 10.0.0      | Recommended for .NET 10.0                          |
| Microsoft.AspNetCore.SpaProxy                  | 8.*-*                        | 10.0.0      | Recommended for .NET 10.0                          |
| Microsoft.EntityFrameworkCore                  | 8.0.15                       | 10.0.0      | Recommended for .NET 10.0                          |
| Microsoft.EntityFrameworkCore.Design           | 8.0.18                       | 10.0.0      | Recommended for .NET 10.0                          |
| Microsoft.EntityFrameworkCore.SqlServer        | 8.0.18                       | 10.0.0      | Recommended for .NET 10.0                          |
| Microsoft.EntityFrameworkCore.Tools            | 8.0.18                       | 10.0.0      | Recommended for .NET 10.0                          |

### Project upgrade details
This section contains details about each project upgrade and modifications that need to be done in the project.

#### src/Domain/Domain.csproj modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

NuGet packages changes:
  - (none)

Feature upgrades:
  - (none)

Other changes:
  - (none)

#### src/Application/Application.csproj modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

NuGet packages changes:
  - Microsoft.EntityFrameworkCore should be updated from `8.0.15` to `10.0.0` (*recommended for .NET 10.0*)

Feature upgrades:
  - (none)

Other changes:
  - (none)

#### src/Infrastructure/Infrastructure.csproj modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

NuGet packages changes:
  - Microsoft.AspNetCore.Authentication.JwtBearer should be updated from `8.0.18` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore should be updated from `8.0.18` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.AspNetCore.Identity.EntityFrameworkCore should be updated from `8.0.18` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.EntityFrameworkCore.SqlServer should be updated from `8.0.18` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.EntityFrameworkCore.Tools should be updated from `8.0.18` to `10.0.0` (*recommended for .NET 10.0*)

Feature upgrades:
  - (none)

Other changes:
  - (none)

#### src/Web/WebUI/WebUI.esproj modifications

Project properties changes:
  - Target framework should be changed from `.NETFramework,Version=v4.7.2` to `net10.0`

NuGet packages changes:
  - (none)

Feature upgrades:
  - (none)

Other changes:
  - (none)

#### src/Web/WebAPI/WebAPI.csproj modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

NuGet packages changes:
  - Microsoft.AspNetCore.OpenApi should be updated from `8.0.18` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.AspNetCore.SpaProxy should be updated from `8.*-*` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.EntityFrameworkCore.Design should be updated from `8.0.18` to `10.0.0` (*recommended for .NET 10.0*)

Feature upgrades:
  - (none)

Other changes:
  - (none)
