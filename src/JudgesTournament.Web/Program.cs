using JudgesTournament.Application;
using JudgesTournament.Infrastructure;
using JudgesTournament.Infrastructure.Data;
using JudgesTournament.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Service registrations (Clean - delegated to extension methods)
builder.Services.AddControllersWithViews();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddWebServices(builder.Configuration);

// Limit request body size for file uploads
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB overall limit
});

var app = builder.Build();

// Database initialization (admin seeding)
await ApplicationDbInitializer.InitializeAsync(app.Services);

// Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseGlobalExceptionHandler();
    app.UseHsts();
}

// Status code pages (404, 403, etc.) — must be before routing
app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

// Security headers
app.UseSecurityHeaders();

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Route configuration
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
