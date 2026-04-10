using JudgesTournament.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JudgesTournament.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TeamRegistration> TeamRegistrations { get; }
    DbSet<RegistrationStatusHistory> RegistrationStatusHistories { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
