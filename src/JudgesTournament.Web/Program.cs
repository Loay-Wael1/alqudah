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

var app = builder.Build();

// Database initialization (admin seeding)
await ApplicationDbInitializer.InitializeAsync(app.Services);

// Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseGlobalExceptionHandler();
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
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
