using JudgesTournament.Domain.Enums;

namespace JudgesTournament.Application.DTOs;

/// <summary>
/// Lightweight DTO for list views - no receipt image path for performance.
/// </summary>
public class RegistrationListItemDto
{
    public int Id { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string Governorate { get; set; } = string.Empty;
    public int PlayersCount { get; set; }
    public string ContactPersonName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public RegistrationStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
