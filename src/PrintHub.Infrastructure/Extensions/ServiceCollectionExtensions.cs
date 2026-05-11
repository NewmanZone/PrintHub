using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrintHub.Core.Interfaces.Repositories;
using PrintHub.Infrastructure.Data;
using PrintHub.Infrastructure.Repositories;

namespace PrintHub.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPrintHubInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PrintHubDbContext>(options =>
        {
            options.UseCosmos(
                configuration["Azure:CosmosDb:ConnectionString"] ?? "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                configuration["Azure:CosmosDb:DatabaseName"] ?? "PrintHub");
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IShopRepository, ShopRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IPartRepository, PartRepository>();
        services.AddScoped<IPrintJobRepository, PrintJobRepository>();

        return services;
    }
}
