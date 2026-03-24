using Microsoft.AspNetCore.Identity;

// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

namespace AthensWorkspace.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public class OAuthUser : IdentityUser<int>
{
    public Provider Provider { get; set; }
    public string? OAuthId { get; set; }
}