using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Intelectia.Application.Common.Behaviors;
using Intelectia.Application.Mappings;

namespace Intelectia.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // AutoMapper 16.x: usar AddAutoMapper con acción de configuración
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

        // FluentValidation 12.x: AddValidatorsFromAssembly está en FluentValidation namespace
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        return services;
    }
}
