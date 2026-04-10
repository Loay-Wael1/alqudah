using JudgesTournament.Application.DTOs;
using JudgesTournament.Application.Interfaces;
using JudgesTournament.Application.Options;
using JudgesTournament.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace JudgesTournament.Web.Controllers;

public class RegistrationController : Controller
{
    private readonly IRegistrationService _registrationService;
    private readonly IFileStorageService _fileStorageService;
    private readonly RegistrationOptions _registrationOptions;
    private readonly UploadOptions _uploadOptions;
    private readonly ILogger<RegistrationController> _logger;

    public RegistrationController(
        IRegistrationService registrationService,
        IFileStorageService fileStorageService,
        IOptions<RegistrationOptions> registrationOptions,
        IOptions<UploadOptions> uploadOptions,
        ILogger<RegistrationController> logger)
    {
        _registrationService = registrationService;
        _fileStorageService = fileStorageService;
        _registrationOptions = registrationOptions.Value;
        _uploadOptions = uploadOptions.Value;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Register()
    {
        var model = new RegisterTeamViewModel
        {
            IsRegistrationOpen = _registrationOptions.IsOpen
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("registration")]
    public async Task<IActionResult> Register(RegisterTeamViewModel model, CancellationToken cancellationToken)
    {
        model.IsRegistrationOpen = _registrationOptions.IsOpen;

        if (!_registrationOptions.IsOpen)
        {
            _logger.LogWarning("Registration attempt while registration is closed from IP {IP}",
                HttpContext.Connection.RemoteIpAddress);
            ModelState.AddModelError("", "التسجيل مغلق حاليًا.");
            return View(model);
        }

        // Level 1 file validation
        if (model.ReceiptImage is not null)
        {
            var extension = Path.GetExtension(model.ReceiptImage.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !_uploadOptions.AllowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(model.ReceiptImage),
                    "نوع الملف غير مسموح. الأنواع المسموحة: " + string.Join(", ", _uploadOptions.AllowedExtensions));
            }

            if (model.ReceiptImage.Length > _uploadOptions.MaxReceiptSizeBytes)
            {
                ModelState.AddModelError(nameof(model.ReceiptImage),
                    $"حجم الملف يتجاوز الحد المسموح ({_uploadOptions.MaxReceiptSizeMb} ميجابايت).");
            }

            if (model.ReceiptImage.Length == 0)
            {
                ModelState.AddModelError(nameof(model.ReceiptImage), "الملف فارغ.");
            }
        }

        if (!ModelState.IsValid)
            return View(model);

        string? receiptPath = null;
        try
        {
            // Save file (with Level 2 validation inside service)
            await using var stream = model.ReceiptImage!.OpenReadStream();
            receiptPath = await _fileStorageService.SaveReceiptAsync(stream, model.ReceiptImage.FileName, cancellationToken);

            var request = new CreateRegistrationRequest
            {
                TeamName = model.TeamName,
                Governorate = model.Governorate,
                PlayersCount = model.PlayersCount,
                UniformColor = model.UniformColor,
                ContactPersonName = model.ContactPersonName,
                PhoneNumber = model.PhoneNumber,
                WhatsAppNumber = model.WhatsAppNumber,
                TransferFromNumber = model.TransferFromNumber,
                TransferName = model.TransferName,
                TransferAmount = model.TransferAmount,
                TransferDate = model.TransferDate!.Value,
                ReceiptImagePath = receiptPath,
                AgreedToTerms = model.AgreedToTerms,
                ConfirmedPreliminary = model.ConfirmedPreliminary
            };

            var response = await _registrationService.CreateAsync(request, cancellationToken);

            if (!response.Success)
            {
                // Clean up uploaded file on failure
                _fileStorageService.DeleteReceipt(receiptPath);

                _logger.LogInformation("Registration rejected for team {Team}: {Reason}",
                    model.TeamName, response.ErrorMessage);
                ModelState.AddModelError("", response.ErrorMessage!);
                return View(model);
            }

            return RedirectToAction(nameof(Success), new { referenceNumber = response.ReferenceNumber, teamName = model.TeamName });
        }
        catch (InvalidOperationException ex)
        {
            // File validation errors from FileStorageService
            _logger.LogWarning("File validation failed during registration: {Message}", ex.Message);
            ModelState.AddModelError(nameof(model.ReceiptImage), ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            // Clean up uploaded file on unexpected error
            if (receiptPath is not null)
            {
                try { _fileStorageService.DeleteReceipt(receiptPath); }
                catch (Exception cleanupEx)
                {
                    _logger.LogWarning(cleanupEx, "Failed to cleanup receipt file after error");
                }
            }

            _logger.LogError(ex, "Unexpected error during registration for team {Team}", model.TeamName);
            ModelState.AddModelError("", "حدث خطأ أثناء التسجيل. يرجى المحاولة مرة أخرى.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Success(string referenceNumber, string teamName)
    {
        if (string.IsNullOrEmpty(referenceNumber))
            return RedirectToAction(nameof(Register));

        var model = new RegistrationSuccessViewModel
        {
            ReferenceNumber = referenceNumber,
            TeamName = teamName
        };
        return View(model);
    }
}
