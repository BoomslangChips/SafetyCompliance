using SafetyCompliance.Domain.Common;

namespace SafetyCompliance.Domain.Entities;

public class Plant : AuditableEntity
{
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; } = true;
    public string? PhotoBase64 { get; set; }
    public string? PhotoFileName { get; set; }

    public Company Company { get; set; } = null!;
    public ICollection<Section> Sections { get; set; } = [];
    public ICollection<InspectionRound> InspectionRounds { get; set; } = [];
}
