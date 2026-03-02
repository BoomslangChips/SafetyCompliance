using SafetyCompliance.Domain.Common;

namespace SafetyCompliance.Domain.Entities;

public class Note : AuditableEntity
{
    public int?   EquipmentId { get; set; }
    public int?   CompanyId   { get; set; }
    public int?   PlantId     { get; set; }

    public string Title    { get; set; } = string.Empty;
    public string Content  { get; set; } = string.Empty;

    public NoteCategory Category { get; set; } = NoteCategory.General;
    public NotePriority Priority { get; set; } = NotePriority.Normal;
    public bool         IsPinned { get; set; }

    // Navigation
    public Equipment? Equipment { get; set; }
    public Company?   Company   { get; set; }
    public Plant?     Plant     { get; set; }
}

public enum NoteCategory : byte
{
    General     = 0,
    Safety      = 1,
    Maintenance = 2,
    Observation = 3,
    Compliance  = 4
}

public enum NotePriority : byte
{
    Normal    = 0,
    Important = 1,
    Urgent    = 2
}
