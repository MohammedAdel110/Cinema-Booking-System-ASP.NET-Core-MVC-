namespace CinemaBooking.Infrastructure;

using CinemaBooking.Application.Interfaces;
using CinemaBooking.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IFileStorageService, FileStorageService>();

        return services;
    }
}
