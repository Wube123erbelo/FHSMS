using System.Reflection;
using FHSMS.Application.Common.Behaviours;
using FHSMS.Domain.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FHSMS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));

        // Domain services are registered here (rather than in Infrastructure) because
        // TaxEngine has zero infrastructure dependencies - it is pure business logic.
        services.AddScoped<ITaxEngine, TaxEngine>();

        return services;
    }
}
