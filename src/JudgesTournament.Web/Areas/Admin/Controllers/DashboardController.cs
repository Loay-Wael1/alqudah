using JudgesTournament.Application.Interfaces;
using JudgesTournament.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JudgesTournament.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly IRegistrationService _registrationService;

    public DashboardController(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var stats = await _registrationService.GetDashboardStatsAsync(cancellationToken);
        var model = new DashboardViewModel { Stats = stats };
        return View(model);
    }
}
