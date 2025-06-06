using IntelligentAgents.AuthLibrary.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace IntelligentAgents.AuthLibrary;

public class AppIdentityDbContext : IdentityDbContext<AppUser>
{
    private readonly IConfiguration? _configuration;

    //used for migrations
    public AppIdentityDbContext() { }

    //used when the application is running
    public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //if the application runs use this
        if (_configuration is not null)
        {
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultAuthentication"),
                options => options.EnableRetryOnFailure());
        }
        //otherwise this is used for migrations, because the configuration can not be instantiated without the application running
        else
        {
            optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=IntelligentAgentsAuthDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
                options => options.EnableRetryOnFailure());
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>().HasData(
            new AppUser()
            {
                Id = "7c9e6679-7425-40de-944b-e07fc1f90ae7",
                UserName = "user1@gmail.com",
                Email = "user1@gmail.com",
                NormalizedUserName = "USER1@GMAIL.COM",
                NormalizedEmail = "USER1@GMAIL.COM",
                EmailConfirmed = true,
                PasswordHash = new PasswordHasher<AppUser>().HashPassword(null!, "CIiyyBRXjTGac7j!"),
                SecurityStamp = Guid.NewGuid().ToString()
            }
        );

        builder.Entity<AppUser>().HasData(
            new AppUser()
            {
                Id = "b58b4f4e-3c0d-4b8a-9217-0c0a3c1e79f9",
                UserName = "user2@gmail.com",
                Email = "user2@gmail.com",
                NormalizedUserName = "USER2@GMAIL.COM",
                NormalizedEmail = "USER2@GMAIL.COM",
                EmailConfirmed = true,
                PasswordHash = new PasswordHasher<AppUser>().HashPassword(null!, "0XfN725l5EwSTIk!"),
                SecurityStamp = Guid.NewGuid().ToString()
            }
        );
    }
}