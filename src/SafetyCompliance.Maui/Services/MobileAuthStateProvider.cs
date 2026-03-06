using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace SafetyCompliance.Maui.Services;

/// <summary>
/// Provides a default authenticated identity for the MAUI mobile app
/// (no ASP.NET Identity stack required).
/// </summary>
public class MobileAuthStateProvider : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.Name, "Mobile User"),
                new Claim(ClaimTypes.Role, "Admin")
            },
            "MobileAuth");

        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }
}
