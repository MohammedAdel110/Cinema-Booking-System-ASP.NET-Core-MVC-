namespace CinemaBooking.Application;

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<CinemaBooking.Application.Mappings.MappingProfile>());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(CinemaBooking.Application.Behaviors.ValidationBehavior<,>));
        });

        return services;
    }
}
