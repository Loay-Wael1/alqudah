using JudgesTournament.Application.Options;
using JudgesTournament.Infrastructure.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace JudgesTournament.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind Options
        services.Configure<RegistrationOptions>(configuration.GetSection(RegistrationOptions.SectionName));
        services.Configure<UploadOptions>(configuration.GetSection(UploadOptions.SectionName));
        services.Configure<RateLimitingOptions>(configuration.GetSection(RateLimitingOptions.SectionName));

        // WebHostEnvironment accessor for FileStorageService
        services.AddSingleton<IWebHostEnvironmentAccessor>(sp =>
        {
            var env = sp.GetRequiredService<IWebHostEnvironment>();
            return new WebHostEnvironmentAccessor(env.ContentRootPath);
        });

        // Rate limiting
        var rateLimitOptions = configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>() ?? new();
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("registration", limiterOptions =>
            {
                limiterOptions.PermitLimit = rateLimitOptions.PermitLimit;
                limiterOptions.Window = TimeSpan.FromMinutes(rateLimitOptions.WindowMinutes);
                limiterOptions.QueueLimit = 0;
            });

            options.OnRejected = async (context, _) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsync("تم تجاوز عدد المحاولات المسموحة. يرجى المحاولة لاحقًا.");
            };
        });

        // Cookie authentication paths
        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Admin/Account/Login";
            options.AccessDeniedPath = "/Admin/Account/Login";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
        });

        // Antiforgery
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
        });

        return services;
    }
}

internal class WebHostEnvironmentAccessor : IWebHostEnvironmentAccessor
{
    public string ContentRootPath { get; }
    public WebHostEnvironmentAccessor(string contentRootPath) => ContentRootPath = contentRootPath;
}
