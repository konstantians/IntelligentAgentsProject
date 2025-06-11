using IntelligentAgents.DataLibrary;
using IntelligentAgents.DataLibrary.DataAccess;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace IntelligentAgents.DataLibraryAPI;

public class Program()
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        IConfiguration configuration = builder.Configuration;
        // Add services to the container.

        builder.Services.AddHttpClient("EmbeddingMicroserviceApiClient", client =>
        {
            client.BaseAddress = new Uri("http://127.0.0.1:8000/");
        });

        builder.Services.AddControllers().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        });
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddDbContext<AppDataDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultData")));

        builder.Services.AddScoped<ICategoryDataAccess, CategoryDataAccess>();
        builder.Services.AddScoped<IProductDataAccess, ProductDataAccess>();
        builder.Services.AddScoped<IVariantDataAccess, VariantDataAccess>();
        builder.Services.AddScoped<IDiscountDataAccess, DiscountDataAccess>();
        builder.Services.AddScoped<IPaymentOptionDataAccess, PaymentOptionDataAccess>();
        builder.Services.AddScoped<IShippingOptionDataAccess, ShippingOptionDataAccess>();
        builder.Services.AddScoped<IQueryDataAccess, QueryDataAccess>();
        builder.Services.AddScoped<IEmbeddingsDataAccess, EmbeddingsDataAccess>();

        var app = builder.Build();
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDataDbContext>();
            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("EmbeddingMicroserviceApiClient");
            context.Database.Migrate();

            await DataSeeder.Seed(context, httpClient);
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.MapControllers();

        app.Run();
    }
}

