using Bogus;
using IntelligentAgents.DataLibrary;
using IntelligentAgents.DataLibrary.Models;
using System.Net.Http.Json;
using System.Text.Json;

public static class DataSeeder
{
    public static async Task Seed(AppDataDbContext context, HttpClient embeddingMicroserviceHttpClient)
    {
        if (context.Categories.Any() || context.Discounts.Any() || context.Products.Any() || context.ShippingOptions.Any()
            || context.PaymentOptions.Any() || context.Variants.Any())
        {
            return;
        }

        var faker = new Faker();

        // 1. Categories - 8 fixed categories with random names
        var categories = new List<Category>();
        for (int i = 1; i <= 8; i++)
        {
            categories.Add(new Category
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"{faker.Commerce.Categories(1)[0]} {i}",
                CreatedAt = DateTime.Now,
            });
        }
        context.Categories.AddRange(categories);

        // 2. Discounts - 8 discounts
        var discounts = new List<Discount>();
        for (int i = 1; i <= 8; i++)
        {
            discounts.Add(new Discount
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"DISC-{i}",
                Percentage = faker.Random.Int(5, 30),
                CreatedAt = DateTime.Now,
            });
        }
        context.Discounts.AddRange(discounts);
        context.SaveChanges();

        // 3. Products - 20 products
        var products = new List<Product>();
        for (int i = 1; i <= 20; i++)
        {
            var product = new Product
            {
                Id = Guid.NewGuid().ToString(),
                Code = $"PROD{i:0000}",
                Name = faker.Commerce.ProductName(),
                CreatedAt = DateTime.Now,
            };

            //each product can have up to 2 categories
            var categoriesCount = faker.Random.Int(0, 2);
            if (categoriesCount > 0)
                product.Categories = faker.PickRandom(categories, categoriesCount).ToList();

            //each product must have at least one variant and less than 4 variants
            var variantsCount = faker.Random.Int(1, 3);
            product.Variants = new List<Variant>();
            for (int v = 1; v <= variantsCount; v++)
            {
                var variant = new Variant
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductId = product.Id, //not needed, but left here for clarity
                    SKU = $"SKU-{product.Code}-{v}",
                    Price = faker.Random.Decimal(10, 100),
                    UnitsInStock = faker.Random.Int(0, 100),
                    CreatedAt = DateTime.Now,
                };

                if (faker.Random.Bool(0.3f)) //30 percent chance for a variant to have a discount
                    variant.DiscountId = faker.PickRandom(discounts).Id;

                product.Variants.Add(variant);
            }
            products.Add(product);
        }

        foreach (var product in products)
        {
            var categoriesText = product.Categories != null && product.Categories.Any()
                ? string.Join(", ", product.Categories.Select(c => c.Name))
                : "no categories";

            var variantsText = string.Join(" ; ", product.Variants.Select(v =>
            {
                var discountText = v.DiscountId != null ? ", discounted by " : "";
                return $"SKU {v.SKU} priced at {v.Price:C} with {v.UnitsInStock} units in stock{discountText}%";
            }));

            product.Description = $"{product.Name} (Code: {product.Code}) product in categories: {categoriesText}. Variants: {variantsText}.";
        }
        context.Products.AddRange(products);
        context.SaveChanges();

        // 4. Shipping Options - custom options
        var shippingOptions = new List<ShippingOption>
        {
            new ShippingOption { Id = Guid.NewGuid().ToString(), Name = "Standard Shipping", ExtraCost = 5m, ContainsDelivery = true,
                Description = "Standard Shipping, moderately priced shipping option with extra cost of 5. Includes delivery.", CreatedAt = DateTime.Now},
            new ShippingOption { Id = Guid.NewGuid().ToString(), Name = "Express Shipping", ExtraCost = 15m, ContainsDelivery = true,
                Description = "Express Shipping, expensive shipping option with extra cost 15. Includes delivery.", CreatedAt = DateTime.Now },
            new ShippingOption { Id = Guid.NewGuid().ToString(), Name = "In-store Pickup", ExtraCost = 0m, ContainsDelivery = false,
                Description = "In-store Pickup, cheapest shipping option with no extra cost. Does not include delivery.", CreatedAt = DateTime.Now},
            new ShippingOption { Id = Guid.NewGuid().ToString(), Name = "Drone Delivery", ExtraCost = 25m, ContainsDelivery = true,
                Description = "Drone Delivery, most expensive shipping option with extra cost 25. Includes delivery.", CreatedAt = DateTime.Now },
        };
        context.ShippingOptions.AddRange(shippingOptions);

        // 5. Payment Options - custom options
        var paymentOptions = new List<PaymentOption>
        {
            new PaymentOption { Id = Guid.NewGuid().ToString(), Name = "Credit Card", NameAlias = "card", Description="Credit Card payment option (alias: card) with no extra cost.", ExtraCost = 0m, CreatedAt = DateTime.Now},
            new PaymentOption { Id = Guid.NewGuid().ToString(), Name = "PayPal", NameAlias = "paypal", Description="PayPal payment option (alias: paypal) with no extra cost.", ExtraCost = 0m, CreatedAt = DateTime.Now},
            new PaymentOption { Id = Guid.NewGuid().ToString(), Name = "Cash on Delivery", NameAlias = "cod", Description="Cash on Delivery most expensive payment option (alias: cod) with extra cost 2.", ExtraCost = 2m, CreatedAt = DateTime.Now },
        };
        context.PaymentOptions.AddRange(paymentOptions);
        context.SaveChanges();

        //Here try to call the microservice
        Dictionary<string, string> idDescriptionPairs = new Dictionary<string, string>();
        foreach (Product product in products)
            idDescriptionPairs.Add(product.Id!, product.Description!);

        foreach (ShippingOption shippingOption in shippingOptions)
            idDescriptionPairs.Add(shippingOption.Id!, shippingOption.Description!);

        foreach (PaymentOption paymentOption in paymentOptions)
            idDescriptionPairs.Add(paymentOption.Id!, paymentOption.Description!);

        var response = await embeddingMicroserviceHttpClient.PostAsJsonAsync("createEmbeddings", new { idDescriptionPairs = idDescriptionPairs });
        if (!response.IsSuccessStatusCode) //potentionally add some logging there
            return;

        string responseBody = await response.Content.ReadAsStringAsync();
        Dictionary<string, float[]> IdEmbeddingPairs = JsonSerializer.Deserialize<Dictionary<string, float[]>>(responseBody)!;

        foreach (KeyValuePair<string, float[]> keyValuePair in IdEmbeddingPairs)
        {
            ShippingOption? foundShippingOption = shippingOptions.FirstOrDefault(shippingOption => shippingOption.Id == keyValuePair.Key);
            if (foundShippingOption is not null)
            {
                foundShippingOption.Embedding = string.Join(',', keyValuePair.Value);
                continue;
            }

            PaymentOption? foundPaymentOption = paymentOptions.FirstOrDefault(paymentOption => paymentOption.Id == keyValuePair.Key);
            if (foundPaymentOption is not null)
            {
                foundPaymentOption.Embedding = string.Join(',', keyValuePair.Value);
                continue;
            }

            Product? foundProduct = products.FirstOrDefault(product => product.Id == keyValuePair.Key);
            if (foundProduct is not null)
            {
                foundProduct.Embedding = string.Join(',', keyValuePair.Value);
                continue;
            }
        }

        context.SaveChanges();
    }
}
