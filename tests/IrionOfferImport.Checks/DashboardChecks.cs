using System.Reflection;
using System.Text.Json;
using Application.Common.Interfaces;
using Application.Dashboard.Queries.GetBestDeals;
using Application.Dashboard.Queries.GetFeaturedStores;
using Application.Dashboard.Queries.GetMostSearchedProducts;
using Application.Dashboard.Queries.SearchProducts;
using Domain.Entities;
using Infrastructure.Services;

internal static class DashboardChecks
{
    public static async Task Run(Action<bool, string> check)
    {
        foreach (var scenario in new[] { "legacy", "available", "sold-out", "disabled-inventory", "disabled-store" })
        {
            var context = DispatchProxy.Create<IApplicationDbContext, InspectionContext>();
            var fake = (InspectionContext)(object)context;
            var manufacturer = Manufacturer.Create("Maker");
            var franchise = Franchise.Create("Series");
            Product AddProduct(string name, int views)
            {
                var product = Product.Create(name, manufacturer.ManufacturerId, franchise.FranchiseId,
                    null, false, "", "", "Description", default);
                product.SetViewCount(views);
                Set(product, nameof(Product.Manufacturer), manufacturer);
                Set(product, nameof(Product.Franchise), franchise);
                Set(product, nameof(Product.ProductCategories), new List<ProductCategory>());
                Set(product, nameof(Product.Inventories), new List<Inventory>());
                fake.Products.Add(product);
                return product;
            }
            Store AddStore(string name, int order)
            {
                var store = Store.Create(name, "https://example.test/", "icon", order, name);
                Set(store, nameof(Store.Inventories), new List<Inventory>());
                fake.Stores.Add(store);
                return store;
            }
            Inventory AddOffer(Product product, Store store, int price, int discount, bool? available)
            {
                var row = available is null
                    ? Inventory.Create(product.ProductId, store.StoreId, price, 0, discount)
                    : Inventory.CreateImportedOffer(product.ProductId, store.StoreId, price,
                        discount == 0 ? 0 : price * 10, discount,
                        "https://irion.cl/products/model", "123", available.Value, DateTime.UtcNow);
                Set(row, nameof(Inventory.Product), product);
                Set(row, nameof(Inventory.Store), store);
                product.Inventories.Add(row);
                store.Inventories.Add(row);
                fake.Rows.Add(row);
                return row;
            }

            var main = AddProduct("Model", 100);
            var empty = AddProduct("Model empty", 90);
            var inactive = AddProduct("Model inactive", 1000);
            inactive.ToggleActive();
            for (var index = 0; index < 10; index++) AddProduct("Model " + index, 80 - index);
            var legacyStore = AddStore("Legacy", 2);
            var availableStore = AddStore("Available", 1);
            var candidateStore = AddStore("Candidate", 3);
            var maximumStore = AddStore("Maximum", 4);
            var emptyStore = AddStore("Empty", 5);
            var legacy = AddOffer(main, legacyStore, 100, 20, null);
            AddOffer(main, availableStore, 200, 0, true);
            var eligible = scenario is "legacy" or "available";
            bool? availability = scenario == "legacy" ? null : scenario != "sold-out";
            var candidate = AddOffer(main, candidateStore, 1, 90, availability);
            var maximum = AddOffer(main, maximumStore, 999, 0, availability);
            if (scenario == "disabled-inventory")
            {
                candidate.ToggleActive();
                maximum.ToggleActive();
            }
            if (scenario == "disabled-store")
            {
                candidateStore.ToggleActive();
                maximumStore.ToggleActive();
            }
            AddOffer(inactive, legacyStore, 1, 90, true);
            AddOffer(empty, emptyStore, 1, 90, false);
            var services = new ApiResponseService();
            // Serialize scalar state rather than the deliberately bidirectional navigation graph.
            string State() => JsonSerializer.Serialize(new
            {
                Products = fake.Products.Select(p => new { p.ProductId, p.IsActive, p.ProductViewCount }),
                Stores = fake.Stores.Select(s => new { s.StoreId, s.IsActive }),
                Rows = fake.Rows.Select(i => new { i.InventoryId, i.Price, i.DiscountPercentage, i.Availability, i.IsActive }),
                fake.Saves
            });
            var before = State();
            void Assert(bool condition, string message) => check(condition, scenario + ": " + message);

            var search = new SearchProductsHandler(context, services);
            var result = await search.Handle(new SearchProducts("  MODEL  "), default);
            Assert(result.Success && result.Message is null && result.Data?.Count == 10, "search exact success envelope and limit");
            if (result.Data is { Count: > 0 } data)
            {
                Assert(data.Select(p => p.ProductId).SequenceEqual(fake.Products.Where(p => p.IsActive)
                    .OrderByDescending(p => p.ProductViewCount).Take(10).Select(p => p.ProductId)), "search ordering and active catalog");
                var item = data.Single(p => p.ProductId == main.ProductId);
                Assert(item.StoreCount == (eligible ? 4 : 2) && item.MinPrice == (eligible ? 1 : 100) &&
                    item.MaxPrice == (eligible ? 999 : 200) && item.BestDiscount == (eligible ? 90 : 20),
                    "search exact eligible counts, minimum, maximum and discount");
                Assert(item.ManufacturerName == "Maker" && item.FranchiseName == "Series" &&
                    item.ProductDescription == "Description" && item.Categories.Count == 0, "search descriptive fields preserved");
                var fallback = data.Single(p => p.ProductId == empty.ProductId);
                Assert(fallback.StoreCount == 0 && fallback.MinPrice == 0 && fallback.MaxPrice == 0 && fallback.BestDiscount == 0,
                    "sold-out-only product remains visible with zero fallback");
            }
            var blank = await search.Handle(new SearchProducts("  "), default);
            Assert(blank.Success && blank.Message is null && blank.Data is { Count: 0 }, "blank search unchanged");
            var missing = await search.Handle(new SearchProducts("not-matching"), default);
            Assert(missing.Success && missing.Message is null && missing.Data is { Count: 0 }, "search filtering unchanged");

            var deals = await new GetBestDealsHandler(context, services).Handle(new GetBestDeals(), default);
            Assert(deals.Success && deals.Message is null && deals.Data is not null, "deals exact success envelope");
            Assert(deals.Data?.Select(d => d.InventoryId).SequenceEqual(eligible
                ? new[] { candidate.InventoryId, legacy.InventoryId } : new[] { legacy.InventoryId }) == true,
                "deals exact eligibility and descending discount order, including inactive-product exclusion");
            Assert(deals.Data?.Last() is { StoreName: "Legacy", ProductName: "Model", DiscountPercentage: 20, ProductViewCount: 100 },
                "legacy deal fields unchanged");

            var popular = await new GetMostSearchedProductsHandler(context, services).Handle(new GetMostSearchedProducts(), default);
            Assert(popular.Success && popular.Message is null && popular.Data?.Count == 4, "popular exact success envelope and limit");
            if (popular.Data is { Count: > 0 } top)
            {
                Assert(top.Select(p => p.ProductId).SequenceEqual(fake.Products.Where(p => p.IsActive)
                    .OrderByDescending(p => p.ProductViewCount).Take(4).Select(p => p.ProductId)), "popular ordering preserved");
                Assert(top[0].StoreCount == (eligible ? 4 : 2) && top[0].StoreNames.SequenceEqual(eligible
                    ? new[] { "Legacy", "Available", "Candidate", "Maximum" } : new[] { "Legacy", "Available" }),
                    "popular exact eligible store count and names");
                Assert(top[1].ProductId == empty.ProductId && top[1].StoreCount == 0 && top[1].StoreNames.Count == 0,
                    "popular preserves empty-offer catalog fallback");
            }

            var featured = await new GetFeaturedStoresHandler(context, services).Handle(new GetFeaturedStores(), default);
            Assert(featured.Success && featured.Message is null && featured.Data is not null, "featured exact success envelope");
            if (featured.Data is { } stores)
            {
                Assert(stores.Select(s => s.StoreId).SequenceEqual(fake.Stores.Where(s => s.IsActive)
                    .OrderBy(s => s.StoreOrder).Select(s => s.StoreId)), "featured store activity and ordering preserved");
                foreach (var item in stores)
                {
                    var expectedCount = item.StoreId == emptyStore.StoreId ? 0 :
                        item.StoreId == candidateStore.StoreId || item.StoreId == maximumStore.StoreId ? (eligible ? 1 : 0) : 1;
                    var expectedOffers = item.StoreId == legacyStore.StoreId ? 1 :
                        item.StoreId == candidateStore.StoreId && eligible ? 1 : 0;
                    Assert(item.ProductCount == expectedCount && item.OfferCount == expectedOffers,
                        "featured exact product/deal counts: " + item.StoreName);
                    Assert(item.StoreIcon == "icon" && item.StoreWebSite == "https://example.test/", "featured metadata preserved");
                }
            }
            Assert(State() == before && fake.Saves == 0, "all dashboard handlers leave state and save counters unchanged");

            // Pin the existing top-five deal cap using only ordinary active legacy offers.
            var ranked = new List<Inventory>();
            for (var index = 0; index < 6; index++)
                ranked.Add(AddOffer(AddProduct("Ranked " + index, 0), legacyStore, 100, 30 + index, null));
            var capped = await new GetBestDealsHandler(context, services).Handle(new GetBestDeals(), default);
            Assert(capped.Success && capped.Message is null && capped.Data?.Count == 5 &&
                capped.Data.Select(d => d.InventoryId).SequenceEqual(
                    (eligible ? new[] { candidate }.Concat(ranked.OrderByDescending(i => i.DiscountPercentage)).Take(5)
                        : ranked.OrderByDescending(i => i.DiscountPercentage).Take(5)).Select(i => i.InventoryId)),
                "previous top-five deal limit and discount ordering preserved");

            foreach (var row in fake.Rows.Where(i => i.IsActive)) row.ToggleActive();
            var noDeals = await new GetBestDealsHandler(context, services).Handle(new GetBestDeals(), default);
            Assert(noDeals.Success && noDeals.Message is null && noDeals.Data is { Count: 0 }, "no eligible deal fallback");
            var noOffers = await search.Handle(new SearchProducts("Model"), default);
            Assert(noOffers.Success && noOffers.Message is null && noOffers.Data is { Count: 10 } &&
                noOffers.Data.All(p => p.StoreCount == 0 && p.MinPrice == 0 && p.MaxPrice == 0 && p.BestDiscount == 0),
                "catalog without any eligible offers retains ordering/limit and zero aggregates");
        }
    }

    private static void Set<T>(T entity, string property, object value) =>
        typeof(T).GetProperty(property)!.SetValue(entity, value);
}
