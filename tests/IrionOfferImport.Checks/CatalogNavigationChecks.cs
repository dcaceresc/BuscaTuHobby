using System.Reflection;
using System.Text.Json;
using Application.Common.Interfaces;
using Application.Dashboard.Queries.GetCategoryProducts;
using Application.Dashboard.Queries.GetStoreDetail;
using Application.Maintainer.Products.Commands.CreateProduct;
using Application.Maintainer.Products.Commands.UpdateProduct;
using Application.Maintainer.Products.Queries.GetProductById;
using Application.Maintainer.Products.Queries.GetProducts;
using Domain.Common;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

// Catálogo Gunpla: contrato del grado, campos nuevos de la entidad, modelo y migración, lado de escritura del
// mantenedor y los dos listados públicos (categoría y tienda). Todo contra un contexto en memoria y generación de
// SQL sin conexión: aquí no se aplica ninguna migración ni se ejecuta SQL Server.
internal static class CatalogNavigationChecks
{
    public static async Task Run(Action<bool, string> check)
    {
        EnumContract(check);
        EntityContract(check);
        ContractShapes(check);
        ModelShape(check);
        MigrationShape(check);
        await MaintainerWriteSide(check);
        await CategoryListing(check);
        await PriceHonesty(check);
        await StoreDetail(check);
    }

    private static void EnumContract(Action<bool, string> check)
    {
        var values = Enum.GetValues<GunplaGrade>();
        check(values.SequenceEqual(new[]
            {
                GunplaGrade.None, GunplaGrade.HighGrade, GunplaGrade.MasterGrade, GunplaGrade.RealGrade,
                GunplaGrade.PerfectGrade, GunplaGrade.SuperDeformed, GunplaGrade.Other
            }) && values.Length == 7 && Enum.GetUnderlyingType(typeof(GunplaGrade)) == typeof(int),
            "The grade contract is exactly seven integer values in the documented order");
        check((int)GunplaGrade.None == 0 && (int)GunplaGrade.HighGrade == 1 && (int)GunplaGrade.MasterGrade == 2 &&
            (int)GunplaGrade.RealGrade == 3 && (int)GunplaGrade.PerfectGrade == 4 &&
            (int)GunplaGrade.SuperDeformed == 5 && (int)GunplaGrade.Other == 6,
            "Every grade number matches the HTTP contract (0 none, 1 HG, 2 MG, 3 RG, 4 PG, 5 SD, 6 other)");
        check(GunplaGradeRules.LabelOf(GunplaGrade.None) == "Sin grado" &&
            GunplaGradeRules.LabelOf(GunplaGrade.HighGrade) == "High Grade (HG)" &&
            GunplaGradeRules.LabelOf(GunplaGrade.PerfectGrade) == "Perfect Grade (PG)" &&
            GunplaGradeRules.LabelOf(GunplaGrade.Other) == "Otro",
            "The facet label of each grade is the Spanish label the frontend renders");
    }

    private static void EntityContract(Action<bool, string> check)
    {
        var manufacturerId = Guid.NewGuid();
        var franchiseId = Guid.NewGuid();
        var bare = Product.Create("Kit", manufacturerId, franchiseId, null, false, "15+", "1/144", "desc", default);
        check(bare.Grade == GunplaGrade.None && bare.Scale is null && bare.PartNumber is null,
            "A product created without Gunpla data defaults to no grade, no scale and no part number");

        var providedId = Guid.NewGuid();
        var provided = Product.CreateWithProvidedId(providedId, "Kit", manufacturerId, franchiseId, null, false,
            "15+", "1/144", "desc", default);
        check(provided.ProductId == providedId && provided.Grade == GunplaGrade.None &&
            provided.Scale is null && provided.PartNumber is null,
            "The provided-id factory keeps the identity and the same Gunpla defaults");

        var full = Product.Create("Kit MG", manufacturerId, franchiseId, null, true, "15+", "1/100", "desc", default,
            GunplaGrade.MasterGrade, "1/100", "MG-001");
        check(full.Grade == GunplaGrade.MasterGrade && full.Scale == "1/100" && full.PartNumber == "MG-001",
            "The factory stores the grade, the scale and the part number it receives");
        check(Product.Create("Kit", manufacturerId, franchiseId, null, false, "15+", "1/144", "desc", default,
                GunplaGrade.None, new string('a', 16), new string('b', 64)).Scale == new string('a', 16) &&
            Product.Create("Kit", manufacturerId, franchiseId, null, false, "15+", "1/144", "desc", default,
                GunplaGrade.None, null, new string('b', 64)).PartNumber == new string('b', 64),
            "A scale of exactly 16 and a part number of exactly 64 characters are accepted");

        var reached = 0;
        foreach (var input in new (GunplaGrade Grade, string? Scale, string? PartNumber, string Message)[]
                 {
                     ((GunplaGrade)7, null, null, GunplaGradeRules.InvalidGradeMessage),
                     ((GunplaGrade)(-1), null, null, GunplaGradeRules.InvalidGradeMessage),
                     (GunplaGrade.None, new string('a', 17), null, GunplaGradeRules.InvalidScaleMessage),
                     (GunplaGrade.None, "", null, GunplaGradeRules.InvalidScaleMessage),
                     (GunplaGrade.None, "   ", null, GunplaGradeRules.InvalidScaleMessage),
                     (GunplaGrade.None, " 1/144", null, GunplaGradeRules.InvalidScaleMessage),
                     (GunplaGrade.None, null, new string('b', 65), GunplaGradeRules.InvalidPartNumberMessage),
                     (GunplaGrade.None, null, "", GunplaGradeRules.InvalidPartNumberMessage),
                     (GunplaGrade.None, null, "HGUC-191 ", GunplaGradeRules.InvalidPartNumberMessage)
                 })
        {
            try
            {
                Product.Create("Kit", manufacturerId, franchiseId, null, false, "15+", "1/144", "desc", default,
                    input.Grade, input.Scale, input.PartNumber);
                check(false, "The entity must reject " + input.Message);
            }
            catch (ArgumentException e)
            {
                reached++;
                check(e.Message.StartsWith(input.Message, StringComparison.Ordinal),
                    "The entity rejects with the exact Spanish message: " + input.Message);
            }
        }
        check(reached == 9, "Every invalid grade, scale and part number reaches the entity validation");

        // El mutador exige los tres datos y no muta nada cuando rechaza.
        var target = Product.Create("Kit", manufacturerId, franchiseId, null, false, "15+", "1/144", "desc", default,
            GunplaGrade.RealGrade, "1/144", "RG-001");
        target.Update("Kit", manufacturerId, franchiseId, null, false, "15+", "1/144", "desc", default,
            GunplaGrade.PerfectGrade, "1/60", "PG-001");
        check(target.Grade == GunplaGrade.PerfectGrade && target.Scale == "1/60" && target.PartNumber == "PG-001",
            "Update replaces the grade, the scale and the part number");
        var afterUpdate = Describe(target);
        try
        {
            target.Update("Kit", manufacturerId, franchiseId, null, false, "15+", "1/144", "desc", default,
                (GunplaGrade)9, "1/144", "PG-001");
            check(false, "Update must reject an undefined grade");
        }
        catch (ArgumentException)
        {
            check(Describe(target) == afterUpdate,
                "A rejected update leaves the stored Gunpla details untouched");
        }

        var updateParameters = typeof(Product).GetMethod(nameof(Product.Update))!.GetParameters();
        check(updateParameters.Count(p => p.Name is "grade" or "scale" or "partNumber") == 3 &&
            updateParameters.Where(p => p.Name is "grade" or "scale" or "partNumber").All(p => !p.HasDefaultValue),
            "Update requires grade, scale and part number with no default so a caller cannot erase them silently");
        var createParameters = typeof(Product).GetMethod(nameof(Product.Create))!.GetParameters();
        check(createParameters.Where(p => p.Name is "grade" or "scale" or "partNumber").All(p => p.HasDefaultValue) &&
            createParameters.Single(p => p.Name == "grade").DefaultValue is GunplaGrade.None &&
            createParameters.Single(p => p.Name == "scale").DefaultValue is null &&
            createParameters.Single(p => p.Name == "partNumber").DefaultValue is null,
            "Create keeps the Gunpla parameters optional and defaulting to no grade, no scale and no part number");
    }

    // Forma exacta de los DTO públicos, para conciliar el contrato con el frontend sin adivinar nombres.
    private static void ContractShapes(Action<bool, string> check)
    {
        static string Shape(Type type) => string.Join(",", type.GetProperties().OrderBy(p => p.Name)
            .Select(p => p.Name + ":" + Friendly(p.PropertyType)));

        static string Friendly(Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)
                ? "List<" + Friendly(type.GetGenericArguments()[0]) + ">"
                : Nullable.GetUnderlyingType(type) is { } underlying ? Friendly(underlying) + "?"
                : type.Name;

        check(Shape(typeof(CategoryProductsDto)) ==
            "CategoryName:String,CategorySlug:String,Franchises:List<FilterOptionDto>,Grades:List<FilterOptionDto>," +
            "Items:List<CatalogProductDto>,Manufacturers:List<FilterOptionDto>,Page:Int32,PageSize:Int32,Total:Int32",
            "CategoryProductsDto publishes exactly the agreed category listing contract");
        // Los dos precios son opcionales: null significa que el producto no tiene ninguna oferta elegible y por
        // eso no se publica un cero inventado; un cero real sigue siendo un valor publicado.
        check(Shape(typeof(CatalogProductDto)) ==
            "FranchiseName:String,Grade:Int32,ImageUrls:List<String>,ManufacturerName:String,MaxPrice:Int32?," +
            "MinPrice:Int32?,PartNumber:String,ProductId:Guid,ProductName:String,Scale:String,StoreCount:Int32",
            "CatalogProductDto publishes the two prices as optional so no price and a zero price differ");
        check(Shape(typeof(FilterOptionDto)) == "Count:Int32,Label:String,Value:String",
            "FilterOptionDto publishes exactly value, label and count");
        check(Shape(typeof(StoreDetailDto)) ==
            "OfferCount:Int32,Offers:List<StoreOfferDto>,ProductCount:Int32,StoreIcon:String,StoreId:Guid," +
            "StoreName:String,StoreSlug:String,StoreWebSite:String",
            "StoreDetailDto publishes exactly the agreed store detail contract");
        check(Shape(typeof(StoreOfferDto)) ==
            "Condition:Int32,DiscountPercentage:Int32,InventoryId:Guid,ObservedAtUtc:DateTime?," +
            "OriginalPrice:Int32,PaymentMethodLabel:String,Price:Int32,ProductId:Guid,ProductName:String",
            "StoreOfferDto publishes exactly the agreed store offer contract");
        check(typeof(ProductDto).GetProperty(nameof(ProductDto.Grade))?.PropertyType == typeof(int) &&
            typeof(ProductDto).GetProperty(nameof(ProductDto.Scale))?.PropertyType == typeof(string) &&
            typeof(ProductDto).GetProperty(nameof(ProductDto.PartNumber))?.PropertyType == typeof(string) &&
            typeof(ProductVM).GetProperty(nameof(ProductVM.Grade))?.PropertyType == typeof(int) &&
            typeof(ProductVM).GetProperty(nameof(ProductVM.Scale))?.PropertyType == typeof(string) &&
            typeof(ProductVM).GetProperty(nameof(ProductVM.PartNumber))?.PropertyType == typeof(string),
            "Both maintainer read sides expose the integer grade, the scale and the part number");
    }

    private static void ModelShape(Action<bool, string> check)
    {
        using var context = StructuralContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        var entity = model.FindEntityType(typeof(Product));
        var grade = entity?.FindProperty(nameof(Product.Grade));
        var scale = entity?.FindProperty(nameof(Product.Scale));
        var partNumber = entity?.FindProperty(nameof(Product.PartNumber));
        check(grade is not null && grade.ClrType == typeof(GunplaGrade) && !grade.IsNullable &&
            grade.GetColumnType() == "int" && Convert.ToInt32(grade.GetDefaultValue()) == 0 &&
            grade.GetDefaultValue() is GunplaGrade,
            "The model maps the grade as a required int column with a database default of 0 (no grade)");
        check(scale is not null && scale.IsNullable && scale.GetMaxLength() == 16 &&
            scale.GetColumnType() == "nvarchar(16)",
            "The model maps the scale as a nullable nvarchar(16) column");
        check(partNumber is not null && partNumber.IsNullable && partNumber.GetMaxLength() == 64 &&
            partNumber.GetColumnType() == "nvarchar(64)",
            "The model maps the part number as a nullable nvarchar(64) column");
    }

    private static void MigrationShape(Action<bool, string> check)
    {
        using var context = StructuralContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        var migrations = context.GetService<IMigrationsAssembly>();
        var newest = migrations.Migrations.OrderBy(m => m.Key, StringComparer.Ordinal).Last();
        check(newest.Key == "20260723000000_ProductGradeScaleAndPartNumber" &&
            string.CompareOrdinal(newest.Key, "20260722000000_OfferConditionAndPaymentMethod") > 0,
            "The new migration is the newest discoverable one and sorts after the offer condition migration");
        var migration = migrations.CreateMigration(newest.Value!, context.Database.ProviderName!);
        var up = migration.UpOperations;
        var grade = up.OfType<AddColumnOperation>().SingleOrDefault(c => c.Name == "Grade");
        var scale = up.OfType<AddColumnOperation>().SingleOrDefault(c => c.Name == "Scale");
        var partNumber = up.OfType<AddColumnOperation>().SingleOrDefault(c => c.Name == "PartNumber");
        check(up.Count == 3 && grade is not null && grade.Table == "Products" && grade.ColumnType == "int" &&
            !grade.IsNullable && grade.DefaultValue is 0,
            "Up adds the grade as int NOT NULL DEFAULT 0 so existing products stay without grade");
        check(scale is not null && scale.Table == "Products" && scale.ColumnType == "nvarchar(16)" &&
            scale.IsNullable && scale.MaxLength == 16,
            "Up adds the scale as nullable nvarchar(16)");
        check(partNumber is not null && partNumber.Table == "Products" && partNumber.ColumnType == "nvarchar(64)" &&
            partNumber.IsNullable && partNumber.MaxLength == 64,
            "Up adds the part number as nullable nvarchar(64)");
        var down = migration.DownOperations;
        check(down.Count == 3 && down.OfType<DropColumnOperation>()
                .Select(c => c.Table + "." + c.Name).OrderBy(n => n, StringComparer.Ordinal)
                .SequenceEqual(new[] { "Products.Grade", "Products.PartNumber", "Products.Scale" }),
            "Down drops exactly the three new columns and nothing else");

        var generator = context.GetService<IMigrationsSqlGenerator>();
        var sql = string.Join("\n", generator.Generate(up, model).Select(c => c.CommandText));
        check(sql.Contains("ALTER TABLE [Products] ADD [Grade] int NOT NULL DEFAULT 0") &&
            sql.Contains("ALTER TABLE [Products] ADD [Scale] nvarchar(16) NULL") &&
            sql.Contains("ALTER TABLE [Products] ADD [PartNumber] nvarchar(64) NULL"),
            "SQL Server DDL adds the three columns with the default and the nullability");
        var downSql = string.Join("\n", generator.Generate(down, model).Select(c => c.CommandText));
        check(downSql.Contains("DROP COLUMN [Grade]") && downSql.Contains("DROP COLUMN [Scale]") &&
            downSql.Contains("DROP COLUMN [PartNumber]"),
            "SQL Server rollback drops the three columns");

        var file = Directory.EnumerateFiles("src/Infrastructure/Data/Migrations", "*_ProductGradeScaleAndPartNumber.cs")
            .SingleOrDefault();
        var text = file is null ? "" : File.ReadAllText(file);
        check(text.Contains("AddColumn<int>") && text.Contains("defaultValue: 0") &&
            text.Contains("maxLength: 16") && text.Contains("maxLength: 64") && text.Contains("DropColumn"),
            "The hand-written migration file itself declares the default, both lengths and the rollback");
        var designer = Directory.EnumerateFiles("src/Infrastructure/Data/Migrations",
            "*_ProductGradeScaleAndPartNumber.Designer.cs").SingleOrDefault();
        check(designer is not null &&
            File.ReadAllText(designer).Contains("[Migration(\"20260723000000_ProductGradeScaleAndPartNumber\")]"),
            "The frozen designer metadata of the new migration exists");
    }

    private static async Task MaintainerWriteSide(Action<bool, string> check)
    {
        var context = DispatchProxy.Create<IApplicationDbContext, CatalogContext>();
        // DispatchProxy no copia estado: las listas se leen de la instancia real del proxy.
        var proxy = (CatalogContext)(object)context;
        var services = new ApiResponseService();
        var manufacturer = Manufacturer.Create("Bandai");
        var franchise = Franchise.Create("Gundam");
        var category = Category.Create("Maquetas", "bi bi-tools", 1, "maquetas");
        var other = Category.Create("Figuras", "bi bi-person", 2, "figuras");

        CreateProduct Request() => new()
        {
            ProductName = "Kit nuevo", ManufacturerId = manufacturer.ManufacturerId,
            FranchiseId = franchise.FranchiseId, SerieId = null, ProductHasBase = true,
            ProductTargetAge = "15+", ProductSize = "1/144", ProductDescription = "desc",
            ProductReleaseDate = new DateOnly(2026, 1, 1), CategoryIds = [category.CategoryId],
            Grade = (int)GunplaGrade.HighGrade, Scale = "1/144", PartNumber = "HGUC-191"
        };

        var created = await new CreateProductHandler(context, services).Handle(Request(), default);
        check(created.Success && created.Data != Guid.Empty && proxy.Products.Count == 1 && proxy.Saves == 2,
            "Creating a product keeps the success envelope and the two-save sequence");
        var stored = proxy.Products.Single();
        check(created.Data == stored.ProductId && stored.Grade == GunplaGrade.HighGrade &&
            stored.Scale == "1/144" && stored.PartNumber == "HGUC-191" && proxy.Links.Count == 1,
            "Creating a product stores the grade, the scale, the part number and its categories");

        foreach (var invalid in new[]
                 {
                     (1, new string('a', 17), "HGUC-191", GunplaGradeRules.InvalidScaleMessage),
                     (1, "1/144", new string('b', 65), GunplaGradeRules.InvalidPartNumberMessage),
                     (7, "1/144", "HGUC-191", GunplaGradeRules.InvalidGradeMessage),
                     (-1, "1/144", "HGUC-191", GunplaGradeRules.InvalidGradeMessage),
                     (1, " ", "HGUC-191", GunplaGradeRules.InvalidScaleMessage)
                 })
        {
            var rejectedContext = (IApplicationDbContext)DispatchProxy.Create<IApplicationDbContext, CatalogContext>();
            var rejected = (CatalogContext)(object)rejectedContext;
            var response = await new CreateProductHandler(rejectedContext, services).Handle(
                Request() with { Grade = invalid.Item1, Scale = invalid.Item2, PartNumber = invalid.Item3 }, default);
            check(!response.Success && response.Message == invalid.Item4 && response.Data == Guid.Empty &&
                rejected.Products.Count == 0 && rejected.Saves == 0,
                "An invalid create is rejected with the exact Spanish message and persists nothing: " + invalid.Item4);
        }

        var updated = await new UpdateProductHandler(context, services).Handle(new()
        {
            ProductId = stored.ProductId, ProductName = "Kit nuevo", ManufacturerId = manufacturer.ManufacturerId,
            FranchiseId = franchise.FranchiseId, SerieId = Guid.Empty, ProductHasBase = true,
            ProductTargetAge = "15+", ProductSize = "1/144", ProductDescription = "desc",
            ProductReleaseDate = new DateOnly(2026, 1, 1), CategoryIds = [category.CategoryId, other.CategoryId],
            Grade = (int)GunplaGrade.MasterGrade, Scale = "1/100", PartNumber = "MG-001"
        }, default);
        check(updated.Success && updated.Message == "Producto actualizado correctamente" &&
            stored.Grade == GunplaGrade.MasterGrade && stored.Scale == "1/100" &&
            stored.PartNumber == "MG-001" && proxy.Links.Count == 2,
            "Updating a product replaces its Gunpla data and keeps the existing success message");

        // La petición declara los tres datos como opcionales: null significa "conserva lo que ya está guardado".
        check(typeof(UpdateProduct).GetProperty(nameof(UpdateProduct.Grade))?.PropertyType == typeof(int?) &&
            typeof(UpdateProduct).GetProperty(nameof(UpdateProduct.Scale))?.PropertyType == typeof(string) &&
            typeof(UpdateProduct).GetProperty(nameof(UpdateProduct.PartNumber))?.PropertyType == typeof(string),
            "The update request declares the Gunpla fields as optional so omitting one keeps the stored value");

        // Misma petición válida dejando en null los datos que no se quieren cambiar.
        UpdateProduct NominalRequest(int? grade = null, string? scale = null, string? partNumber = null) => new()
        {
            ProductId = stored.ProductId, ProductName = "Kit nuevo", ManufacturerId = manufacturer.ManufacturerId,
            FranchiseId = franchise.FranchiseId, SerieId = Guid.Empty, ProductHasBase = true,
            ProductTargetAge = "15+", ProductSize = "1/144", ProductDescription = "desc",
            ProductReleaseDate = new DateOnly(2026, 1, 1), CategoryIds = [category.CategoryId, other.CategoryId],
            Grade = grade, Scale = scale, PartNumber = partNumber
        };

        // Omitir los tres datos conserva lo guardado en lugar de borrarlo.
        var omitted = await new UpdateProductHandler(context, services).Handle(NominalRequest(), default);
        check(omitted.Success && omitted.Message == "Producto actualizado correctamente" &&
            stored.Grade == GunplaGrade.MasterGrade && stored.Scale == "1/100" && stored.PartNumber == "MG-001",
            "An update that omits the grade, the scale and the part number keeps the three stored values");

        // Un solo dato informado cambia solo ese dato y deja los otros dos como estaban.
        var scaleOnly = await new UpdateProductHandler(context, services)
            .Handle(NominalRequest(scale: "1/60"), default);
        check(scaleOnly.Success && stored.Grade == GunplaGrade.MasterGrade && stored.Scale == "1/60" &&
            stored.PartNumber == "MG-001",
            "Supplying only the scale changes the scale and keeps the stored grade and part number");
        var partNumberOnly = await new UpdateProductHandler(context, services)
            .Handle(NominalRequest(partNumber: "MG-002"), default);
        check(partNumberOnly.Success && stored.Grade == GunplaGrade.MasterGrade && stored.Scale == "1/60" &&
            stored.PartNumber == "MG-002",
            "Supplying only the part number changes it and keeps the stored grade and scale");
        var gradeOnly = await new UpdateProductHandler(context, services)
            .Handle(NominalRequest(grade: (int)GunplaGrade.PerfectGrade), default);
        check(gradeOnly.Success && stored.Grade == GunplaGrade.PerfectGrade && stored.Scale == "1/60" &&
            stored.PartNumber == "MG-002",
            "Supplying only the grade changes it and keeps the stored scale and part number");

        var afterUpdate = Describe(stored);
        var updateSaves = proxy.Saves;

        // Una cadena vacía no es una omisión: se valida como un valor informado y se rechaza con su mensaje.
        foreach (var invalid in new[]
                 {
                     (7, "1/100", "MG-002", GunplaGradeRules.InvalidGradeMessage),
                     (-1, "1/100", "MG-002", GunplaGradeRules.InvalidGradeMessage),
                     ((int)GunplaGrade.MasterGrade, new string('a', 17), "MG-002", GunplaGradeRules.InvalidScaleMessage),
                     ((int)GunplaGrade.MasterGrade, "", "MG-002", GunplaGradeRules.InvalidScaleMessage),
                     ((int)GunplaGrade.MasterGrade, "1/60", new string('b', 65), GunplaGradeRules.InvalidPartNumberMessage),
                     ((int)GunplaGrade.MasterGrade, "1/60", "", GunplaGradeRules.InvalidPartNumberMessage)
                 })
        {
            var response = await new UpdateProductHandler(context, services)
                .Handle(NominalRequest(invalid.Item1, invalid.Item2, invalid.Item3), default);
            check(!response.Success && response.Message == invalid.Item4 && Describe(stored) == afterUpdate &&
                proxy.Saves == updateSaves,
                "An invalid update is rejected with the exact Spanish message and changes nothing: " + invalid.Item4);
        }
        // El listado del mantenedor proyecta los tres campos nuevos en ambos queries.
        foreach (var file in new[] { "GetProducts/GetProducts.cs", "GetProductById/GetProductById.cs" })
        {
            var source = File.ReadAllText(Path.Combine("src", "Application", "Maintainer", "Products", "Queries", file));
            check(source.Contains("Grade = (int)") && source.Contains("Scale =") && source.Contains("PartNumber ="),
                "The maintainer query projects the grade, the scale and the part number: " + file);
        }
    }

    private static async Task CategoryListing(Action<bool, string> check)
    {
        var fixture = BuildFixture();
        var services = new ApiResponseService();
        var handler = new GetCategoryProductsHandler(fixture.Context, services);

        async Task<ApiResponse<CategoryProductsDto>> List(int? page = null, int? pageSize = null,
            string? sort = null, int? grade = null, Guid? manufacturerId = null, Guid? franchiseId = null,
            int? minPrice = null, int? maxPrice = null, string slug = "maquetas") =>
            await handler.Handle(new(slug, page, pageSize, sort, grade, manufacturerId, franchiseId, minPrice, maxPrice), default);

        var all = await List();
        check(all.Success && all.Message is null && all.Data is not null,
            "The category listing answers with the usual success envelope");
        var data = all.Data!;
        check(data.CategoryName == "Maquetas" && data.CategorySlug == "maquetas" && data.Total == 4 &&
            data.Page == 1 && data.PageSize == 20 && data.Items.Count == 4,
            "The listing resolves the category by slug, defaults to page 1 / 20 and counts 4 active products");
        check(data.Items.Select(i => i.ProductName).SequenceEqual(new[] { "Alpha Gundam", "Beta Gundam", "Delta Gundam", "Gamma Figure" }),
            "The default order is by product name and excludes the inactive product");
        check(data.Items.All(i => i.ProductId != fixture.Inactive.ProductId && i.ProductId != fixture.OtherCategoryProduct.ProductId),
            "An inactive product and a product of another category never reach the listing");

        var alpha = data.Items.Single(i => i.ProductId == fixture.Alpha.ProductId);
        check(alpha.ManufacturerName == "Bandai" && alpha.FranchiseName == "Gundam" &&
            alpha.Grade == (int)GunplaGrade.HighGrade && alpha.Scale == "1/144" && alpha.PartNumber == "HGUC-191",
            "Each row carries the product identity and its Gunpla details as an integer grade");
        check(alpha.MinPrice == 20000 && alpha.MaxPrice == 24000 && alpha.StoreCount == 2,
            "Prices and store count describe only the eligible offers");
        check(alpha.ImageUrls.Count == 2 && alpha.ImageUrls.All(u => u.StartsWith("/images/") && u.EndsWith(".jpg")) &&
            alpha.ImageUrls[0] != alpha.ImageUrls[1],
            "Image URLs follow the published /images/{id}.jpg path in the stored order");
        var gamma = data.Items.Single(i => i.ProductId == fixture.Gamma.ProductId);
        check(gamma.MinPrice is null && gamma.MaxPrice is null && gamma.StoreCount == 0,
            "A product whose only offer is ineligible publishes no price and zero stores");

        var manufacturers = data.Manufacturers;
        check(manufacturers.Count == 2 &&
            manufacturers[0].Label == "Bandai" && manufacturers[0].Count == 3 &&
            manufacturers[1].Label == "Kotobukiya" && manufacturers[1].Count == 1,
            "The manufacturer facet counts the category products that use each value, most used first");
        check(manufacturers[0].Value == fixture.Bandai.ManufacturerId.ToString(),
            "The manufacturer facet publishes the identifier the filter accepts");
        check(data.Franchises.Count == 2 && data.Franchises[0].Label == "Gundam" && data.Franchises[0].Count == 3 &&
            data.Franchises[1].Label == "Evangelion" && data.Franchises[1].Count == 1,
            "The franchise facet counts the category products that use each value");
        check(data.Grades.Count == 3 &&
            data.Grades[0].Value == "1" && data.Grades[0].Label == "High Grade (HG)" && data.Grades[0].Count == 2 &&
            data.Grades[1].Value == "2" && data.Grades[1].Count == 1 &&
            data.Grades[2].Value == "6" && data.Grades[2].Label == "Otro" && data.Grades[2].Count == 1,
            "The grade facet publishes the integer value, the Spanish label and the product count");

        foreach (var slug in new[] { "no-existe", "categoria-inactiva", "" })
        {
            var unknown = await List(slug: slug);
            check(!unknown.Success && unknown.Message == "La categoría no existe o no está disponible." &&
                unknown.Data is null,
                "An unknown, inactive or blank slug fails with the same typed Spanish envelope: " + slug);
        }

        var requestedSize = await List(pageSize: 2);
        check(requestedSize.Data!.Items.Count == 2 && requestedSize.Data.PageSize == 2 &&
            requestedSize.Data.Total == 4 && requestedSize.Data.Items[0].ProductName == "Alpha Gundam",
            "A smaller page size pages the listing and still reports the count before paging");
        var secondPage = await List(page: 2, pageSize: 2);
        check(secondPage.Data!.Items.Select(i => i.ProductName).SequenceEqual(new[] { "Delta Gundam", "Gamma Figure" }) &&
            secondPage.Data.Page == 2,
            "The second page continues the same order");
        var clampedPage = await List(page: 0, pageSize: 2);
        check(clampedPage.Data!.Page == 1 && clampedPage.Data.Items.Count == 2,
            "A page below 1 clamps to the first page");
        var negativePage = await List(page: -5);
        check(negativePage.Data!.Page == 1 && negativePage.Data.Items.Count == 4,
            "A negative page clamps to the first page");
        var zeroSize = await List(pageSize: 0);
        check(zeroSize.Data!.PageSize == 1 && zeroSize.Data.Items.Count == 1 && zeroSize.Data.Total == 4,
            "A page size below 1 clamps to one product");
        var hugeSize = await List(pageSize: 500);
        check(hugeSize.Data!.PageSize == 48 && hugeSize.Data.Items.Count == 4,
            "A page size above the maximum clamps to 48");
        var hugePage = await List(page: int.MaxValue, pageSize: 48);
        check(hugePage.Success && hugePage.Data!.Page == int.MaxValue && hugePage.Data.Items.Count == 0 &&
            hugePage.Data.Total == 4,
            "A page beyond the last one returns no rows with the real total instead of overflowing");

        foreach (var (sort, expected) in new[]
                 {
                     ("name", new[] { "Alpha Gundam", "Beta Gundam", "Delta Gundam", "Gamma Figure" }),
                     ("price-asc", new[] { "Beta Gundam", "Alpha Gundam", "Delta Gundam", "Gamma Figure" }),
                     ("price-desc", new[] { "Delta Gundam", "Alpha Gundam", "Beta Gundam", "Gamma Figure" }),
                     ("views", new[] { "Delta Gundam", "Beta Gundam", "Gamma Figure", "Alpha Gundam" }),
                     ("desconocido", new[] { "Alpha Gundam", "Beta Gundam", "Delta Gundam", "Gamma Figure" })
                 })
        {
            var sorted = await List(sort: sort);
            check(sorted.Success && sorted.Data!.Items.Select(i => i.ProductName).SequenceEqual(expected),
                "The " + sort + " order lists the products in the expected sequence");
        }

        // Solo los dos órdenes de precio llevan al final el producto sin ofertas elegibles; "views" ordena por su
        // propia clave y no promete nada sobre él. La comprobación anterior no puede afirmar lo contrario.
        foreach (var (sort, offerlessLast) in new[] { ("price-asc", true), ("price-desc", true), ("views", false) })
        {
            var sorted = await List(sort: sort);
            check((sorted.Data!.Items[^1].ProductId == fixture.Gamma.ProductId) == offerlessLast,
                "The " + sort + " order " + (offerlessLast ? "places" : "does not place") +
                " the product without eligible offers last");
        }

        foreach (var (filter, expected) in new (Func<Task<ApiResponse<CategoryProductsDto>>>, string[])[]
                 {
                     (() => List(grade: (int)GunplaGrade.HighGrade), new[] { "Alpha Gundam", "Delta Gundam" }),
                     (() => List(manufacturerId: fixture.Bandai.ManufacturerId),
                         new[] { "Alpha Gundam", "Delta Gundam", "Gamma Figure" }),
                     (() => List(franchiseId: fixture.Gundam.FranchiseId),
                         new[] { "Alpha Gundam", "Beta Gundam", "Delta Gundam" }),
                     (() => List(minPrice: 20000), new[] { "Alpha Gundam", "Delta Gundam" }),
                     (() => List(maxPrice: 20000), new[] { "Alpha Gundam", "Beta Gundam" }),
                     (() => List(minPrice: 14000, maxPrice: 25000), new[] { "Alpha Gundam", "Beta Gundam" }),
                     (() => List(grade: (int)GunplaGrade.HighGrade, franchiseId: fixture.Gundam.FranchiseId,
                         minPrice: 25000), new[] { "Delta Gundam" }),
                     (() => List(manufacturerId: fixture.Kotobukiya.ManufacturerId, maxPrice: 15000),
                         new[] { "Beta Gundam" })
                 })
        {
            var filtered = await filter();
            check(filtered.Success && filtered.Data!.Total == expected.Length &&
                filtered.Data.Items.Select(i => i.ProductName).SequenceEqual(expected),
                "The filter set combines to the expected products: " + string.Join(",", expected));
        }

        var excludedByPrice = await List(minPrice: 1);
        check(excludedByPrice.Data!.Total == 3 &&
            excludedByPrice.Data.Items.All(i => i.ProductId != fixture.Gamma.ProductId),
            "A price filter excludes a product without eligible offers instead of treating it as zero");
        var filteredFacets = await List(grade: (int)GunplaGrade.MasterGrade);
        check(filteredFacets.Data!.Total == 1 && filteredFacets.Data.Items.Single().ProductName == "Beta Gundam" &&
            filteredFacets.Data.Manufacturers.Count == 2 && filteredFacets.Data.Grades.Count == 3,
            "The facets keep describing the whole category while the items apply the filter");

        var caseInsensitive = await List(slug: "  MAQUETAS  ");
        check(caseInsensitive.Success && caseInsensitive.Data!.Total == 4,
            "The slug matches regardless of case and surrounding spaces");
    }

    private static async Task StoreDetail(Action<bool, string> check)
    {
        var fixture = BuildFixture();
        var handler = new GetStoreDetailHandler(fixture.Context, new ApiResponseService());

        var detail = await handler.Handle(new("tienda-uno"), default);
        check(detail.Success && detail.Message is null && detail.Data is not null,
            "The store detail answers with the usual success envelope");
        var store = detail.Data!;
        check(store.StoreId == fixture.MainStore.StoreId && store.StoreName == "Tienda Uno" &&
            store.StoreSlug == "tienda-uno" && store.StoreIcon == "bi bi-shop" &&
            store.StoreWebSite == "https://uno.cl",
            "The store detail exposes the identity and links of the resolved store");
        check(store.OfferCount == 2 && store.ProductCount == 2,
            "Offer and product counts describe the eligible offers only, counting distinct products");
        check(store.Offers.Select(o => o.Price).SequenceEqual(new[] { 15000, 20000 }) &&
            store.Offers.Select(o => o.ProductName).SequenceEqual(new[] { "Beta Gundam", "Alpha Gundam" }),
            "The store offers are ordered from the cheapest to the most expensive with their product");
        check(store.Offers.All(o => o.InventoryId != fixture.IneligibleAvailability.InventoryId) &&
            store.Offers.All(o => o.ProductId != fixture.Inactive.ProductId) &&
            store.Offers.All(o => o.InventoryId != fixture.IneligibleByOfferFlag.InventoryId) &&
            store.Offers.All(o => o.ProductId != fixture.OtherCategoryProduct.ProductId),
            "Unavailable, disabled, inactive-product and other-store offers stay out of the store detail");
        var cheapest = store.Offers[0];
        check(cheapest.ProductId == fixture.Beta.ProductId && cheapest.OriginalPrice == 0 &&
            cheapest.DiscountPercentage == 0 && cheapest.Condition == (int)ProductCondition.New &&
            cheapest.PaymentMethodLabel is null && cheapest.ObservedAtUtc is null,
            "Each store offer carries its own price tuple, integer condition and observation date");

        foreach (var slug in new[] { "no-existe", "tienda-cerrada", "" })
        {
            var unknown = await handler.Handle(new(slug), default);
            check(!unknown.Success && unknown.Message == "La tienda no existe o no está disponible." &&
                unknown.Data is null,
                "An unknown, inactive or blank store slug fails with the same typed Spanish envelope: " + slug);
        }

        var empty = await handler.Handle(new("tienda-vacia"), default);
        check(empty.Success && empty.Data is { OfferCount: 0, ProductCount: 0, Offers.Count: 0 },
            "A store without eligible offers returns zeroed counts and an empty list");
    }

    // El precio publicado no se inventa: un producto sin ofertas elegibles publica null en las dos columnas y el
    // cero solo aparece cuando una oferta elegible realmente cobra cero. Las dos filas del caso viven en la
    // categoría figuras (ver BuildFixture) para no alterar el listado de maquetas.
    private static async Task PriceHonesty(Action<bool, string> check)
    {
        var fixture = BuildFixture();
        var handler = new GetCategoryProductsHandler(fixture.Context, new ApiResponseService());

        async Task<ApiResponse<CategoryProductsDto>> List(string? sort = null, int? minPrice = null,
            int? maxPrice = null) =>
            await handler.Handle(new("figuras", null, null, sort, null, null, null, minPrice, maxPrice), default);

        var listing = await List();
        check(listing.Success && listing.Data is not null,
            "The figures listing of the price contract answers with the usual success envelope");
        var items = listing.Data!.Items;
        var offerless = items.Single(i => i.ProductId == fixture.Offerless.ProductId);
        check(offerless.MinPrice is null && offerless.MaxPrice is null && offerless.StoreCount == 0,
            "A product whose only offer is ineligible publishes no price instead of a fabricated zero");
        var zero = items.Single(i => i.ProductId == fixture.ZeroPriced.ProductId);
        check(zero.MinPrice == 0 && zero.MaxPrice == 0 && zero.StoreCount == 1,
            "A genuine zero price is published as zero and stays distinguishable from having no price");

        var cheapFirst = await List(sort: "price-asc");
        check(cheapFirst.Data!.Items.Select(i => i.ProductName)
                .SequenceEqual(new[] { "Zeta Zero", "Zeta Figure", "Alpha Figure" }),
            "The ascending price order puts the genuine zero first and the offerless product last");

        var zeroCeiling = await List(maxPrice: 0);
        check(zeroCeiling.Data!.Items.Select(i => i.ProductId).SequenceEqual(new[] { fixture.ZeroPriced.ProductId }),
            "A maximum price of zero matches the genuine zero price and excludes the product without a price");
        var aboveZero = await List(minPrice: 1);
        check(aboveZero.Data!.Items.Select(i => i.ProductId)
                .SequenceEqual(new[] { fixture.OtherCategoryProduct.ProductId }),
            "A minimum price above zero excludes both the zero price and the product without a price");
    }

    private static Fixture BuildFixture() => new();

    private sealed class Fixture
    {
        public CatalogContext Fake { get; }
        public IApplicationDbContext Context { get; }
        public Manufacturer Bandai { get; }
        public Manufacturer Kotobukiya { get; }
        public Franchise Gundam { get; }
        public Store MainStore { get; }
        public Product Alpha { get; }
        public Product Beta { get; }
        public Product Gamma { get; }
        public Product Delta { get; }
        public Product Inactive { get; }
        public Product OtherCategoryProduct { get; }
        public Product ZeroPriced { get; }
        public Product Offerless { get; }
        public Inventory IneligibleAvailability { get; }
        public Inventory IneligibleByOfferFlag { get; }

        public Fixture()
        {
            var context = DispatchProxy.Create<IApplicationDbContext, CatalogContext>();
            var fake = (CatalogContext)(object)context;
            Context = context;
            Fake = fake;

            var maquetas = Category.Create("Maquetas", "bi bi-tools", 1, "maquetas");
            var figuras = Category.Create("Figuras", "bi bi-person", 2, "figuras");
            var closedCategory = Category.Create("Cerrada", "bi bi-x", 3, "categoria-inactiva");
            closedCategory.ToggleActive();
            fake.Categories.AddRange([maquetas, figuras, closedCategory]);

            Bandai = Manufacturer.Create("Bandai");
            Kotobukiya = Manufacturer.Create("Kotobukiya");
            fake.Manufacturers.AddRange([Bandai, Kotobukiya]);
            Gundam = Franchise.Create("Gundam");
            var evangelion = Franchise.Create("Evangelion");
            fake.Franchises.AddRange([Gundam, evangelion]);

            MainStore = Store.Create("Tienda Uno", "https://uno.cl", "bi bi-shop", 1, "tienda-uno");
            var second = Store.Create("Tienda Dos", "https://dos.cl", "bi bi-shop", 2, "tienda-dos");
            var closed = Store.Create("Tienda Cerrada", "https://cerrada.cl", "bi bi-shop", 3, "tienda-cerrada");
            closed.ToggleActive();
            var empty = Store.Create("Tienda Vacía", "https://vacia.cl", "bi bi-shop", 4, "tienda-vacia");
            fake.Stores.AddRange([MainStore, second, closed, empty]);

            Alpha = Add(fake, maquetas, "Alpha Gundam", Bandai, Gundam, GunplaGrade.HighGrade, "1/144", "HGUC-191", 100);
            Beta = Add(fake, maquetas, "Beta Gundam", Kotobukiya, Gundam, GunplaGrade.MasterGrade, "1/100", "MG-001", 300);
            Gamma = Add(fake, maquetas, "Gamma Figure", Bandai, evangelion, GunplaGrade.Other, null, null, 200);
            Delta = Add(fake, maquetas, "Delta Gundam", Bandai, Gundam, GunplaGrade.HighGrade, "1/144", null, 800);
            Inactive = Add(fake, maquetas, "Echo Gundam", Bandai, Gundam, GunplaGrade.HighGrade, "1/144", "HG-9", 900);
            Inactive.ToggleActive();
            OtherCategoryProduct = Add(fake, figuras, "Zeta Figure", Bandai, Gundam, GunplaGrade.None, null, null, 50);

            // Alpha: dos ofertas elegibles y dos que no lo son por cada motivo de la regla (tienda cerrada y
            // oferta deshabilitada), más la de un producto inactivo y la de un producto de otra categoría.
            AddOffer(fake, Alpha, MainStore, 20000);
            AddOffer(fake, Alpha, second, 24000);
            AddOffer(fake, Alpha, closed, 16000);
            IneligibleByOfferFlag = AddOffer(fake, Alpha, second, 15000);
            IneligibleByOfferFlag.ToggleActive();

            // Beta: empate de precio en dos tiendas para probar el desempate estable.
            AddOffer(fake, Beta, MainStore, 15000);
            AddOffer(fake, Beta, second, 15000);
            // Gamma: su única oferta no es elegible, así que queda sin precio.
            AddOffer(fake, Gamma, second, 9000, available: false);
            // Delta: una oferta elegible y una que no lo está porque la oferta no informa disponibilidad.
            AddOffer(fake, Delta, second, 30000);
            IneligibleAvailability = AddOffer(fake, Delta, MainStore, 25000, available: false);
            // Producto inactivo: ninguna de sus ofertas debe publicarse.
            AddOffer(fake, Inactive, MainStore, 12000);
            // Otra categoría: no participa de los facets de maquetas ni del listado de la tienda principal.
            AddOffer(fake, OtherCategoryProduct, second, 1000);

            // Figuras suma dos productos para separar el precio nulo del precio cero real: uno con una oferta
            // elegible que cobra 0 y otro cuya única oferta no es elegible. Viven aquí para no mover el listado
            // de maquetas que fijan las demás comprobaciones.
            Offerless = Add(fake, figuras, "Alpha Figure", Bandai, Gundam, GunplaGrade.None, null, null, 20);
            AddOffer(fake, Offerless, second, 9000, available: false);
            ZeroPriced = Add(fake, figuras, "Zeta Zero", Bandai, Gundam, GunplaGrade.None, null, null, 10);
            AddOffer(fake, ZeroPriced, second, 0);
        }

        private static Product Add(CatalogContext fake, Category category, string name, Manufacturer manufacturer,
            Franchise franchise, GunplaGrade grade, string? scale, string? partNumber, int views)
        {
            var product = Product.Create(name, manufacturer.ManufacturerId, franchise.FranchiseId, null, false, "15+",
                "1/144", "desc", new DateOnly(2026, 1, 1), grade, scale, partNumber);
            product.SetViewCount(views);
            Set(product, nameof(Product.Manufacturer), manufacturer);
            Set(product, nameof(Product.Franchise), franchise);
            var link = product.AssignCategory(category.CategoryId);
            Set(link, nameof(ProductCategory.Category), category);
            Set(product, nameof(Product.ProductCategories), new List<ProductCategory> { link });
            Set(product, nameof(Product.ProductImages),
                new List<ProductImage> { product.AssignImage(2), product.AssignImage(1) });
            Set(product, nameof(Product.Inventories), new List<Inventory>());
            fake.Products.Add(product);
            fake.Links.Add(link);
            return product;
        }

        private static Inventory AddOffer(CatalogContext fake, Product product, Store store, int price,
            bool? available = null)
        {
            var offer = Inventory.Create(product.ProductId, store.StoreId, price, 0, 0);
            if (available is { } value) Set(offer, nameof(Inventory.Availability), (bool?)value);
            Set(offer, nameof(Inventory.Product), product);
            Set(offer, nameof(Inventory.Store), store);
            product.Inventories.Add(offer);
            fake.Rows.Add(offer);
            return offer;
        }
    }

    private static ApplicationDbContext StructuralContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=unused;Database=StructuralOnly;Integrated Security=true;TrustServerCertificate=true")
            .Options);

    private static string Describe(Product product) => JsonSerializer.Serialize(new
    {
        product.ProductName, product.Grade, product.Scale, product.PartNumber, product.ProductSize, product.IsActive
    });

    private static void Set(object target, string property, object? value) =>
        target.GetType().GetProperty(property)!.SetValue(target, value);
}

// Contexto en memoria para el lado de escritura del mantenedor y para los dos listados públicos. No es una base de
// datos: las consultas se ejecutan con LINQ to Objects y por eso no demuestran traducción a SQL Server.
public class CatalogContext : DispatchProxy
{
    public List<Product> Products { get; } = [];
    public List<Store> Stores { get; } = [];
    public List<Category> Categories { get; } = [];
    public List<Inventory> Rows { get; } = [];
    public List<ProductCategory> Links { get; } = [];
    public List<Manufacturer> Manufacturers { get; } = [];
    public List<Franchise> Franchises { get; } = [];
    public List<ProductImage> Images { get; } = [];
    public int Saves { get; private set; }

    protected override object? Invoke(MethodInfo? method, object?[]? args) => method!.Name switch
    {
        "get_Products" => new CatalogSet<Product>(Products, p => p.ProductId),
        "get_Stores" => new CatalogSet<Store>(Stores, s => s.StoreId),
        "get_Categories" => new CatalogSet<Category>(Categories, c => c.CategoryId),
        "get_Inventories" => new CatalogSet<Inventory>(Rows, i => i.InventoryId),
        "get_ProductCategories" => new CatalogSet<ProductCategory>(Links, pc => pc.ProductId),
        "get_ProductImages" => new CatalogSet<ProductImage>(Images, pi => pi.ProductImageId),
        "get_Manufacturers" => new CatalogSet<Manufacturer>(Manufacturers, m => m.ManufacturerId),
        "get_Franchises" => new CatalogSet<Franchise>(Franchises, f => f.FranchiseId),
        "SaveChangesAsync" => Save(),
        _ => throw new NotSupportedException(method.Name)
    };

    private Task<int> Save()
    {
        Saves++;
        return Task.FromResult(1);
    }
}

internal sealed class CatalogSet<T>(List<T> rows, Func<T, Guid> key) : DbSet<T>, IQueryable<T> where T : class
{
    public override Microsoft.EntityFrameworkCore.Metadata.IEntityType EntityType => throw new NotSupportedException();
    public override Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T> Add(T entity)
    {
        rows.Add(entity);
        return null!;
    }
    public override Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T> Remove(T entity)
    {
        rows.Remove(entity);
        return null!;
    }
    public override ValueTask<T?> FindAsync(object?[]? keys, CancellationToken cancellationToken) =>
        ValueTask.FromResult(rows.SingleOrDefault(row => key(row) == (Guid)keys![0]!));
    Type IQueryable.ElementType => typeof(T);
    System.Linq.Expressions.Expression IQueryable.Expression => rows.AsQueryable().Expression;
    IQueryProvider IQueryable.Provider => new InspectionProvider(rows.AsQueryable().Provider, _ => { });
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => rows.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => rows.GetEnumerator();
}
