using JudgesTournament.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JudgesTournament.Infrastructure.Data.Configurations;

public class RegistrationStatusHistoryConfiguration : IEntityTypeConfiguration<RegistrationStatusHistory>
{
    public void Configure(EntityTypeBuilder<RegistrationStatusHistory> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.ChangedByAdminId).IsRequired().HasMaxLength(450);
        builder.Property(h => h.Notes).HasMaxLength(1000);
        builder.Property(h => h.ChangedAtUtc).IsRequired();

        builder.HasOne(h => h.TeamRegistration)
            .WithMany(r => r.StatusHistory)
            .HasForeignKey(h => h.TeamRegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(h => h.TeamRegistrationId)
            .HasDatabaseName("IX_StatusHistory_TeamRegistrationId");
    }
}
