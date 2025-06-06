﻿using IntelligentAgents.DataLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace IntelligentAgents.DataLibrary;

public class AppDataDbContext : DbContext
{
    private readonly IConfiguration? _configuration;

    //used for migrations
    public AppDataDbContext() { }

    public AppDataDbContext(DbContextOptions<AppDataDbContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //this is used for migrations, because the configuration can not be instantiated without the application running
        if (_configuration is not null)
        {

            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultData"),
                options => options.EnableRetryOnFailure());
        }
        else
        {
            optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=IntelligentAgentsDataDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
                options => options.EnableRetryOnFailure());
        }
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Variant> Variants { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<UserCoupon> UserCoupons { get; set; }
    public DbSet<PaymentOption> PaymentOptions { get; set; }
    public DbSet<ShippingOption> ShippingOptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        /******************* Category *******************/
        modelBuilder.Entity<Category>()
            .HasIndex(category => category.Name).IsUnique();

        modelBuilder.Entity<Category>()
            .Property(category => category.Name).HasMaxLength(50).IsRequired();

        /******************* Products *******************/
        //variant can have many variants(one to many)
        modelBuilder.Entity<Product>()
            .HasMany(product => product.Variants)
            .WithOne(variant => variant.Product)
            .HasForeignKey(variant => variant.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        //variant can have many categories and a category can have many products(many to many)
        modelBuilder.Entity<Product>()
            .HasMany(product => product.Categories)
            .WithMany(categories => categories.Products);

        modelBuilder.Entity<Product>()
            .HasIndex(product => product.Code).IsUnique();

        modelBuilder.Entity<Product>()
            .Property(product => product.Code).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<Product>()
            .HasIndex(product => product.Name).IsUnique();

        modelBuilder.Entity<Product>()
            .Property(product => product.Name).HasMaxLength(50).IsRequired();

        /******************* Variants *******************/
        //variant can have many attributes and an attribute can have many variants(many to many)
        modelBuilder.Entity<Variant>()
            .HasIndex(variant => variant.SKU).IsUnique();

        modelBuilder.Entity<Variant>()
            .Property(variant => variant.SKU).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<Variant>()
            .Property(variant => variant.Price).IsRequired();

        modelBuilder.Entity<Variant>()
            .Property(variant => variant.UnitsInStock).IsRequired();

        /******************* Discounts *******************/
        //discounts can have many variants(one to many)
        modelBuilder.Entity<Discount>()
            .HasMany(discount => discount.Variants)
            .WithOne(variant => variant.Discount)
            .HasForeignKey(variant => variant.DiscountId);

        modelBuilder.Entity<Discount>()
            .HasIndex(discount => discount.Name).IsUnique();

        modelBuilder.Entity<Discount>()
            .Property(discount => discount.Name).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<Discount>()
            .Property(discount => discount.Percentage).IsRequired();

        base.OnModelCreating(modelBuilder);

        /******************* Coupons *******************/
        modelBuilder.Entity<Coupon>()
            .HasIndex(coupon => coupon.Code).IsUnique();

        modelBuilder.Entity<Coupon>()
            .Property(coupon => coupon.Code).HasMaxLength(50).IsRequired(); //the code can be null since I need it in the orders even if the coupon is user specific(otherwise I could leave it null)

        modelBuilder.Entity<Coupon>()
            .Property(coupon => coupon.DiscountPercentage).IsRequired();

        modelBuilder.Entity<Coupon>()
            .Property(coupon => coupon.UsageLimit).IsRequired();

        modelBuilder.Entity<Coupon>()
            .Property(coupon => coupon.IsUserSpecific).IsRequired();

        modelBuilder.Entity<Coupon>()
            .Property(coupon => coupon.IsDeactivated).IsRequired();

        modelBuilder.Entity<Coupon>()
            .Property(coupon => coupon.TriggerEvent).HasMaxLength(50).IsRequired();

        //The start and end date can be null if the event is user specific(I could give them default values, but I do not know if that makes sense)
        //The DefaultDateIntervalInDays can be null if the event is universal

        /******************* UserCoupons *******************/
        modelBuilder.Entity<UserCoupon>()
            .HasOne(userCoupon => userCoupon.Coupon)
            .WithMany(coupon => coupon.UserCoupons)
            .HasForeignKey(userCoupon => userCoupon.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserCoupon>()
            .Property(userCoupon => userCoupon.Code).HasMaxLength(50).IsRequired(); //the code of the userCoupon can be copy pasted if the coupon that creates it is universal

        modelBuilder.Entity<UserCoupon>()
            .Property(userCoupon => userCoupon.TimesUsed).IsRequired();

        modelBuilder.Entity<UserCoupon>()
            .Property(coupon => coupon.StartDate).IsRequired();

        modelBuilder.Entity<UserCoupon>()
            .Property(coupon => coupon.ExpirationDate).IsRequired();

        modelBuilder.Entity<UserCoupon>()
            .Property(userCoupon => userCoupon.UserId).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<UserCoupon>()
            .Property(userCoupon => userCoupon.CouponId).IsRequired();

        /******************* ShippingOptions *******************/
        modelBuilder.Entity<PaymentOption>()
            .HasIndex(paymentOption => paymentOption.Name).IsUnique();

        modelBuilder.Entity<PaymentOption>()
            .Property(paymentOption => paymentOption.Name).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<PaymentOption>()
            .HasIndex(paymentOption => paymentOption.NameAlias).IsUnique();

        modelBuilder.Entity<PaymentOption>()
            .Property(paymentOption => paymentOption.NameAlias).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<PaymentOption>()
            .Property(paymentOption => paymentOption.ExtraCost).IsRequired();

        /******************* ShippingOptions *******************/
        modelBuilder.Entity<ShippingOption>()
            .HasIndex(shippingOption => shippingOption.Name).IsUnique();

        modelBuilder.Entity<ShippingOption>()
            .Property(shippingOption => shippingOption.Name).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<ShippingOption>()
            .Property(shippingOption => shippingOption.ExtraCost).IsRequired();

        modelBuilder.Entity<ShippingOption>()
            .Property(shippingOption => shippingOption.ContainsDelivery).IsRequired();
    }
}
