using JudgesTournament.Application.Interfaces;
using JudgesTournament.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JudgesTournament.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<TeamRegistration> TeamRegistrations => Set<TeamRegistration>();
    public DbSet<RegistrationStatusHistory> RegistrationStatusHistories => Set<RegistrationStatusHistory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
