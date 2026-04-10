using Microsoft.AspNetCore.Mvc;

namespace JudgesTournament.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Handles HTTP status code error pages (404, 403, 500).
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [Route("Home/StatusCode")]
    public new IActionResult StatusCode(int code = StatusCodes.Status500InternalServerError)
    {
        Response.StatusCode = code;

        return code switch
        {
            404 => View("NotFound"),
            403 => View("AccessDenied"),
            _ => View("Error")
        };
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
