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
        // Services
        services.AddScoped<IStorageService, StorageService>();
        
        // Repositories
        services.AddScoped<IBucketRepository, BucketRepository>();
        services.AddScoped<IBucketObjectRepository, BucketObjectRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        
        return services;
    }
}