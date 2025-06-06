using Bogus;
using IntelligentAgents.DataLibrary;
using IntelligentAgents.DataLibrary.Models;

public static class DataSeeder
{
    public static void Seed(AppDataDbContext context)
    {
        if (context.Categories.Any() || context.Discounts.Any() || context.Products.Any() || context.ShippingOptions.Any()
            || context.PaymentOptions.Any() || context.Coupons.Any() || context.Variants.Any() || context.UserCoupons.Any())
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
        context.Products.AddRange(products);
        context.SaveChanges();

        // 4. Shipping Options - custom options
        var shippingOptions = new List<ShippingOption>
        {
            new ShippingOption { Id = Guid.NewGuid().ToString(), Name = "Standard Shipping", ExtraCost = 5m, ContainsDelivery = true, CreatedAt = DateTime.Now },
            new ShippingOption { Id = Guid.NewGuid().ToString(), Name = "Express Shipping", ExtraCost = 15m, ContainsDelivery = true, CreatedAt = DateTime.Now },
            new ShippingOption { Id = Guid.NewGuid().ToString(), Name = "In-store Pickup", ExtraCost = 0m, ContainsDelivery = false, CreatedAt = DateTime.Now},
            new ShippingOption { Id = Guid.NewGuid().ToString(), Name = "Drone Delivery", ExtraCost = 25m, ContainsDelivery = true, CreatedAt = DateTime.Now },
        };
        context.ShippingOptions.AddRange(shippingOptions);

        // 5. Payment Options - custom options
        var paymentOptions = new List<PaymentOption>
        {
            new PaymentOption { Id = Guid.NewGuid().ToString(), Name = "Credit Card", NameAlias = "card", ExtraCost = 0m, CreatedAt = DateTime.Now},
            new PaymentOption { Id = Guid.NewGuid().ToString(), Name = "PayPal", NameAlias = "paypal", ExtraCost = 0m, CreatedAt = DateTime.Now},
            new PaymentOption { Id = Guid.NewGuid().ToString(), Name = "Cash on Delivery", NameAlias = "cod", ExtraCost = 2m, CreatedAt = DateTime.Now },
        };
        context.PaymentOptions.AddRange(paymentOptions);

        // 6. Coupons
        var coupons = new List<Coupon>();
        float couponChanceToBeAddedToFirstUser = faker.Random.Float(0.1f, 0.5f);
        float couponChanceToBeAddedToSecondUser = faker.Random.Float(0.1f, 0.5f);
        for (int i = 1; i <= 3; i++)
        {
            var coupon = new Coupon
            {
                Id = Guid.NewGuid().ToString(),
                Code = $"Coupon{i:0000}",
                Description = faker.Lorem.Paragraph(),
                DiscountPercentage = faker.Random.Int(5, 30),
                UsageLimit = faker.Random.Int(1, 4),

                StartDate = DateTime.Now,
                ExpirationDate = DateTime.Now.AddDays(faker.Random.Int(5, 30)),
                IsUserSpecific = false,
                TriggerEvent = "NoTrigger",
                IsDeactivated = false,
                CreatedAt = DateTime.Now,
            };

            if (faker.Random.Bool(couponChanceToBeAddedToFirstUser)) //30 percent chance for the coupon to be added to the first user
            {
                var userCoupon = new UserCoupon
                {
                    Id = Guid.NewGuid().ToString(),
                    Code = $"UserCoupon{i:0000}-A",
                    TimesUsed = faker.Random.Int(0, coupon.UsageLimit ?? 0),
                    StartDate = coupon.StartDate,
                    ExpirationDate = coupon.ExpirationDate,
                    CouponId = coupon.Id, //is not needed, but I leave it here for clarity
                    UserId = "7c9e6679-7425-40de-944b-e07fc1f90ae7", //hardcoded first user id
                    CreatedAt = DateTime.Now,
                };

                coupon.UserCoupons.Add(userCoupon);
            }

            if (faker.Random.Bool(couponChanceToBeAddedToSecondUser)) //30 percent chance for the coupon to be added to the first user
            {
                var userCoupon = new UserCoupon
                {
                    Id = Guid.NewGuid().ToString(),
                    Code = $"UserCoupon{i:0000}-B",
                    TimesUsed = faker.Random.Int(0, coupon.UsageLimit ?? 0),
                    StartDate = coupon.StartDate,
                    ExpirationDate = coupon.ExpirationDate,
                    CouponId = coupon.Id, //is not needed, but I leave it here for clarity
                    UserId = "b58b4f4e-3c0d-4b8a-9217-0c0a3c1e79f9", //hardcoded second user id
                    CreatedAt = DateTime.Now,
                };

                coupon.UserCoupons.Add(userCoupon);
                coupons.Add(coupon);
            }
        }

        context.Coupons.AddRange(coupons);
        context.SaveChanges();
    }
}
