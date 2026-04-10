using JudgesTournament.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JudgesTournament.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(SignInManager<IdentityUser> signInManager, ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("Admin logged in: {Email}", model.Email);
            return RedirectToLocal(returnUrl);
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("Admin account locked out: {Email}", model.Email);
            ModelState.AddModelError("", "الحساب مقفل مؤقتًا بسبب محاولات تسجيل دخول فاشلة متعددة.");
            return View(model);
        }

        ModelState.AddModelError("", "البريد الإلكتروني أو كلمة المرور غير صحيحة.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home", new { area = "" });
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
    }
}
