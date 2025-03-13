using AuraGenie.Api.Business;
using AuraGenie.Api.Hubs;
using AuraGenie.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AuraGenie.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddData(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AuraContext>(options => { options.UseSqlite(configuration.GetConnectionString("Default")); });
    }

    public static void AddBusiness(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddHostedService<GenieResponder>();
        services.AddHttpContextAccessor();
        
        services.AddTransient<RoomService>();
        services.AddTransient<ChatService>();
        services.AddTransient<OpenAiService>();
    }
}