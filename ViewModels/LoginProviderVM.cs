using AthensWorkspace.Models;
using Utility;

namespace AthensWorkspace.ViewModels;

public class LoginProviderVm
{
    public Provider Provider { get; init; }
    public string Name => Provider.GetText();
    public string ImagePath { get; init; } = null!;
    public string BackStyle { get; init; } = null!;
    public string? RedirectUrl { get; init; }
}