using JudgesTournament.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace JudgesTournament.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TeamRegistration> TeamRegistrations { get; }
    DbSet<RegistrationStatusHistory> RegistrationStatusHistories { get; }
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Access to change tracking for concurrency token manipulation.
    /// </summary>
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
}
