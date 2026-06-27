namespace CinemaBooking.Persistence;

using CinemaBooking.Domain.Interfaces;
using CinemaBooking.Persistence.Data;
using CinemaBooking.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IShowTimeRepository, ShowTimeRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
