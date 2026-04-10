using JudgesTournament.Domain.Enums;

namespace JudgesTournament.Domain.Entities;

public class TeamRegistration
{
    public int Id { get; set; }

    // Team Info
    public string TeamName { get; set; } = string.Empty;
    public string NormalizedTeamName { get; set; } = string.Empty;
    public string Governorate { get; set; } = string.Empty;
    public int PlayersCount { get; set; }
    public string UniformColor { get; set; } = string.Empty;

    // Contact Info
    public string ContactPersonName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string WhatsAppNumber { get; set; } = string.Empty;

    // Payment Info
    public string TransferFromNumber { get; set; } = string.Empty;
    public string TransferName { get; set; } = string.Empty;
    public decimal TransferAmount { get; set; }
    public DateTime TransferDate { get; set; }
    public string ReceiptImagePath { get; set; } = string.Empty;

    // Registration Info
    public string? ReferenceNumber { get; set; }
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;
    public string? AdminNotes { get; set; }
    public bool AgreedToTerms { get; set; }
    public bool ConfirmedPreliminary { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Admin review tracking
    public DateTime? ReviewedAtUtc { get; set; }
    public string? ReviewedByAdminId { get; set; }

    // Concurrency
    public byte[] RowVersion { get; set; } = [];

    // Navigation
    public ICollection<RegistrationStatusHistory> StatusHistory { get; set; } = [];
}
