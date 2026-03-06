using SafetyCompliance.Domain.Common;

namespace SafetyCompliance.Domain.Entities;

/// <summary>
/// A named contact associated with a plant, grouped by functional category
/// (e.g. Firefighters, Management, Medical, Maintenance, Security, Contractors).
/// </summary>
public class PlantContact : AuditableEntity
{
    public int    PlantId   { get; set; }
    public Plant  Plant     { get; set; } = null!;

    /// <summary>Free-text category label, e.g. "Firefighters", "Plant Management".</summary>
    public string  Category  { get; set; } = string.Empty;

    public string  Name      { get; set; } = string.Empty;

    /// <summary>Role / title within the category, e.g. "Fire Captain", "Safety Officer".</summary>
    public string? Role      { get; set; }

    public string? Phone     { get; set; }
    public string? Email     { get; set; }
    public string? Notes     { get; set; }

    /// <summary>Marks the primary/on-call contact within a category.</summary>
    public bool IsPrimary    { get; set; }

    public int  SortOrder    { get; set; }

    public ICollection<ContactDocument> Documents { get; set; } = [];
}
