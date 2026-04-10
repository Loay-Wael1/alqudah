namespace JudgesTournament.Application.DTOs;

public class DashboardStatsDto
{
    public int TotalRegistrations { get; set; }
    public int PendingCount { get; set; }
    public int PaymentVerifiedCount { get; set; }
    public int AcceptedCount { get; set; }
    public int RejectedCount { get; set; }
    public decimal TotalTransferAmount { get; set; }
}
