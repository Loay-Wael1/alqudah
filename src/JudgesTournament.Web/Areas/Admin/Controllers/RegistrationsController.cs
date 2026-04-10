using JudgesTournament.Application.DTOs;
using JudgesTournament.Application.Interfaces;
using JudgesTournament.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JudgesTournament.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class RegistrationsController : Controller
{
    private readonly IRegistrationService _registrationService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<RegistrationsController> _logger;

    public RegistrationsController(
        IRegistrationService registrationService,
        IFileStorageService fileStorageService,
        ILogger<RegistrationsController> logger)
    {
        _registrationService = registrationService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<IActionResult> Index([FromQuery] RegistrationFilterDto filter, CancellationToken cancellationToken)
    {
        filter.PageSize = 20;
        if (filter.Page < 1)
            filter.Page = 1;

        var result = await _registrationService.GetPagedAsync(filter, cancellationToken);
        var model = new RegistrationListViewModel
        {
            PagedResult = result,
            Filter = filter
        };

        return View(model);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var registration = await _registrationService.GetByIdAsync(id, cancellationToken);
        if (registration is null)
        {
            _logger.LogWarning("Admin tried to access non-existent registration {Id}", id);
            return NotFound();
        }

        var model = new RegistrationDetailsViewModel
        {
            Registration = registration,
            NewStatus = registration.Status,
            AdminNotes = registration.AdminNotes
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(RegistrationDetailsViewModel model, CancellationToken cancellationToken)
    {
        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";

        var request = new UpdateRegistrationStatusRequest
        {
            RegistrationId = model.Registration.Id,
            NewStatus = model.NewStatus,
            AdminNotes = model.AdminNotes,
            AdminUserId = adminUserId,
            RowVersion = model.Registration.RowVersion
        };

        var (success, errorMessage) = await _registrationService.UpdateStatusAsync(request, cancellationToken);

        if (!success)
        {
            TempData["Error"] = errorMessage;
            return RedirectToAction(nameof(Details), new { id = model.Registration.Id });
        }

        TempData["Success"] = "تم تحديث حالة الطلب بنجاح.";
        return RedirectToAction(nameof(Details), new { id = model.Registration.Id });
    }

    /// <summary>
    /// Serves receipt images securely by registration ID without exposing file paths.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> ViewReceipt(int id, CancellationToken cancellationToken)
    {
        var receiptPath = await _registrationService.GetReceiptPathByIdAsync(id, cancellationToken);
        if (string.IsNullOrEmpty(receiptPath))
        {
            _logger.LogWarning(
                "Receipt view for non-existent registration {Id} by admin {Admin}",
                id,
                User.Identity?.Name);
            return NotFound();
        }

        var fullPath = _fileStorageService.GetFullPath(receiptPath);
        if (!System.IO.File.Exists(fullPath))
        {
            _logger.LogWarning(
                "Receipt file missing on disk for registration {Id}, admin {Admin}",
                id,
                User.Identity?.Name);
            return NotFound();
        }

        var extension = Path.GetExtension(fullPath).ToLowerInvariant();
        var contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        return PhysicalFile(fullPath, contentType);
    }
}
