using HyDrive.Application.Interfaces.Services;
using HyDrive.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HyDrive.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Link interfaces to concrete classes
        services.AddScoped<IStorageService, StorageService>();

        // Add other services here
        return services;
    }
}