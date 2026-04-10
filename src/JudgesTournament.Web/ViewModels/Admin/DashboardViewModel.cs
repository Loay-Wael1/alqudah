using JudgesTournament.Application.DTOs;

namespace JudgesTournament.Web.ViewModels.Admin;

public class DashboardViewModel
{
    public DashboardStatsDto Stats { get; set; } = new();
}
