using JudgesTournament.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JudgesTournament.Infrastructure.Data.Configurations;

public class TeamRegistrationConfiguration : IEntityTypeConfiguration<TeamRegistration>
{
    public void Configure(EntityTypeBuilder<TeamRegistration> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.TeamName).IsRequired().HasMaxLength(100);
        builder.Property(r => r.NormalizedTeamName).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Governorate).IsRequired().HasMaxLength(50);
        builder.Property(r => r.PlayersCount).IsRequired();
        builder.Property(r => r.UniformColor).IsRequired().HasMaxLength(50);

        builder.Property(r => r.ContactPersonName).IsRequired().HasMaxLength(100);
        builder.Property(r => r.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(r => r.WhatsAppNumber).IsRequired().HasMaxLength(20);

        builder.Property(r => r.TransferFromNumber).IsRequired().HasMaxLength(20);
        builder.Property(r => r.TransferName).IsRequired().HasMaxLength(100);
        builder.Property(r => r.TransferAmount).IsRequired().HasColumnType("decimal(10,2)");
        builder.Property(r => r.TransferDate).IsRequired();
        builder.Property(r => r.ReceiptImagePath).IsRequired().HasMaxLength(500);

        builder.Property(r => r.ReferenceNumber).HasMaxLength(20);
        builder.Property(r => r.AdminNotes).HasMaxLength(1000);

        // Concurrency token
        builder.Property(r => r.RowVersion).IsRowVersion();

        // Unique indexes for duplicate prevention at DB level
        builder.HasIndex(r => r.PhoneNumber)
            .IsUnique()
            .HasDatabaseName("IX_TeamRegistrations_PhoneNumber");

        builder.HasIndex(r => new { r.NormalizedTeamName, r.Governorate })
            .IsUnique()
            .HasDatabaseName("IX_TeamRegistrations_NormalizedTeamName_Governorate");

        builder.HasIndex(r => r.ReferenceNumber)
            .IsUnique()
            .HasFilter("[ReferenceNumber] IS NOT NULL")
            .HasDatabaseName("IX_TeamRegistrations_ReferenceNumber");

        // Performance index for list queries
        builder.HasIndex(r => r.Status)
            .HasDatabaseName("IX_TeamRegistrations_Status");

        builder.HasIndex(r => r.CreatedAtUtc)
            .HasDatabaseName("IX_TeamRegistrations_CreatedAtUtc");
    }
}
