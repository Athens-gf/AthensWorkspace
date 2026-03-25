using AthensWorkspace.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AthensWorkspace.Controllers;

public abstract class AdminController(
    UserManager<OAuthUser> userManager,
    IConfiguration configuration
) : Controller
{
    private OAuthUser? GetOAuthUser() => User.Identity?.IsAuthenticated ?? false ? userManager.GetUserAsync(User).Result : null;

    protected bool IsAdmin()
    {
        var oAuthUser = GetOAuthUser();
        if (oAuthUser == null) return false;
        return oAuthUser.Email == configuration.GetSection("Admin")["Email"];
    }

    protected IActionResult CheckAdminRedirect(Func<IActionResult> makeView) => IsAdmin() ? makeView() : RedirectToAction("Index", "Home");
}