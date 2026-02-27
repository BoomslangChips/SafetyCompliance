using SafetyCompliance.Domain.Common;

namespace SafetyCompliance.Domain.Entities;

public class Company : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsActive { get; set; } = true;
    public string? PhotoBase64 { get; set; }
    public string? PhotoFileName { get; set; }

    public ICollection<Plant> Plants { get; set; } = [];
    public ICollection<UserCompany> UserCompanies { get; set; } = [];
}
