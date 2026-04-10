using JudgesTournament.Domain.Enums;

namespace JudgesTournament.Domain.Entities;

public class RegistrationStatusHistory
{
    public int Id { get; set; }
    public int TeamRegistrationId { get; set; }
    public RegistrationStatus OldStatus { get; set; }
    public RegistrationStatus NewStatus { get; set; }
    public string ChangedByAdminId { get; set; } = string.Empty;
    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    // Navigation
    public TeamRegistration TeamRegistration { get; set; } = null!;
}
