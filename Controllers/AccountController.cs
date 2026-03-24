using System.Security.Claims;
using AthensWorkspace.Data;
using AthensWorkspace.Models;
using AthensWorkspace.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Utility;

namespace AthensWorkspace.Controllers;

[Authorize]
[Route("[action]")]
public class AccountController(
    MyIdentityDbContext dbContext,
    UserManager<OAuthUser> userManager,
    SignInManager<OAuthUser> signInManager
) : Controller
{
    public IActionResult Index() => RedirectToAction(nameof(Login));
    private OAuthUser? GetOAuthUser() => User.Identity?.IsAuthenticated ?? false ? userManager.GetUserAsync(User).Result : null;

    private static readonly Dictionary<Provider, string> ProviderIconPath = new()
    {
        { Provider.Google, "img/logo/google.svg" }
    };

    private static readonly Dictionary<Provider, string> ProviderBackStyle = new()
    {
        { Provider.Google, "color: black; background: white; margin: 8px 5px 5px;" }
    };

    [HttpGet]
    [AllowAnonymous]
    [Route("")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (GetOAuthUser() != null) return RedirectToAction("Index", "Home");
        return View(ExEnum.GetIter<Provider>().Select(provider => new LoginProviderVm
        {
            Provider = provider,
            ImagePath = ProviderIconPath[provider],
            BackStyle = ProviderBackStyle[provider],
            RedirectUrl = returnUrl
        }));
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("")]
    public IActionResult ExternalLogin(Provider provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action("ExternalLoginCallback", "Account",
            new { ReturnUrl = returnUrl ?? $"/{nameof(Index)}", Provider = provider });
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider.ToString(), redirectUrl);
        return new ChallengeResult(provider.ToString(), properties); // 認証ページが開く
    }

    /// <summary>認証のコールバック</summary>
    [AllowAnonymous]
    [Route("")]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, Provider? provider = null,
        string? remoteError = null)
    {
        returnUrl ??= Url.Content("~/");
        if (remoteError != null)
        {
            ModelState.AddModelError(string.Empty, $"Error from external provider:{remoteError}");
            return View(nameof(Login));
        }

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            ModelState.AddModelError(string.Empty, "Error loading external login information.");
            return View(nameof(Login));
        }

        var signInResult = await signInManager
            .ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true, true);

        var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? "";
        var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? "";
        var oAuthId = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

        // 既に登録済みのユーザー
        if (signInResult.Succeeded)
        {
            var oAuthUser = userManager.Users.First(u => u.OAuthId == oAuthId);
            var isChangeName = oAuthUser.UserName != name;
            var isUnregisteredEmail = oAuthUser.Email.IsNullOrEmpty() && !email.IsNullOrEmpty();
            if (isChangeName || isUnregisteredEmail)
            {
                if (isChangeName)
                    oAuthUser.UserName = name;
                if (isUnregisteredEmail)
                    oAuthUser.Email = email;
                dbContext.Update(oAuthUser);
                await dbContext.SaveChangesAsync();

                return LocalRedirect(returnUrl);
            }

            return LocalRedirect(returnUrl);
        }

        // 認証初めてのユーザー
        /*var user = new OAuthUser
        {
            Provider = provider ?? throw new Exception(),
            UserName = name,
            Email = email,
            OAuthId = oAuthId
        };*/

        // await userManager.CreateAsync(user);
        // await userManager.AddLoginAsync(user, info);
        // await signInManager.SignInAsync(user, true);
        return LocalRedirect(returnUrl);
    }
}