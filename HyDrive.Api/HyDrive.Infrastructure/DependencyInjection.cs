using HyDrive.Application.Interfaces.Repositories;
using HyDrive.Application.Interfaces.Services;
using HyDrive.Application.Services;
using HyDrive.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HyDrive.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IStorageService, StorageService>();
        services.AddScoped<IBucketRepository, BucketRepository>();
        services.AddScoped<IBucketObjectRepository, BucketObjectRepository>();
        
        return services;
    }
}