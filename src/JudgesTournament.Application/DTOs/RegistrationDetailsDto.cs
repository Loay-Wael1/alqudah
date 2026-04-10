using JudgesTournament.Domain.Enums;

namespace JudgesTournament.Application.DTOs;

public class RegistrationDetailsDto
{
    public int Id { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string Governorate { get; set; } = string.Empty;
    public int PlayersCount { get; set; }
    public string UniformColor { get; set; } = string.Empty;
    public string ContactPersonName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string WhatsAppNumber { get; set; } = string.Empty;
    public string TransferFromNumber { get; set; } = string.Empty;
    public string TransferName { get; set; } = string.Empty;
    public decimal TransferAmount { get; set; }
    public DateTime TransferDate { get; set; }
    public string ReceiptImagePath { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public RegistrationStatus Status { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public List<StatusHistoryItemDto> StatusHistory { get; set; } = [];
}

public class StatusHistoryItemDto
{
    public RegistrationStatus OldStatus { get; set; }
    public RegistrationStatus NewStatus { get; set; }
    public string ChangedByAdminId { get; set; } = string.Empty;
    public DateTime ChangedAtUtc { get; set; }
    public string? Notes { get; set; }
}
