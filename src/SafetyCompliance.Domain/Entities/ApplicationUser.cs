using Microsoft.AspNetCore.Identity;

namespace SafetyCompliance.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string FullName => $"{FirstName} {LastName}";
}
