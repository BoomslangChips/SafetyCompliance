namespace SafetyCompliance.Domain.Entities;

public class UserCompany
{
    public string UserId { get; set; } = string.Empty;
    public int CompanyId { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Company Company { get; set; } = null!;
}
