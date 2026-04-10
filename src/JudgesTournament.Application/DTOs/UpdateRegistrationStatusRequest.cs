using JudgesTournament.Domain.Enums;

namespace JudgesTournament.Application.DTOs;

public class UpdateRegistrationStatusRequest
{
    public int RegistrationId { get; set; }
    public RegistrationStatus NewStatus { get; set; }
    public string? AdminNotes { get; set; }
    public string AdminUserId { get; set; } = string.Empty;
    public byte[] RowVersion { get; set; } = [];
}
