using JudgesTournament.Application.Interfaces;
using JudgesTournament.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JudgesTournament.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IRegistrationService, RegistrationService>();
        return services;
    }
}
