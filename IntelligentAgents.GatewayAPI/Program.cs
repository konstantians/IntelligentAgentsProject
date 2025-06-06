using IntelligentAgents.GatewayAPI.HelperMethods;
using IntelligentAgents.GatewayAPI.IntelligentAgentService.IntelligentAgentLogic;
using IntelligentAgents.GatewayAPI.IntelligentAgentService.IntelligentAgentLogic.Connectors;


public class Program()
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        IConfiguration configuration = builder.Configuration;

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddHttpClient("AuthApiClient", client =>
        {
            client.BaseAddress = new Uri(configuration["AuthApiBaseUrl"]!);
        });
        builder.Services.AddHttpClient("DataApiClient", client =>
        {
            client.BaseAddress = new Uri(configuration["DataApiBaseUrl"]!);
        });
        builder.Services.AddHttpClient("OllamaApiClient", client =>
        {
            client.BaseAddress = new Uri("http://localhost:11434/api/");
        });

        builder.Services.AddScoped<IAiAssistantService, AiAssistantService>();
        builder.Services.AddSingleton<IUtilityMethods, UtilityMethods>();
        builder.Services.AddScoped<IOllamaConnectorService, OllamaConnectorService>();

        var app = builder.Build();

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