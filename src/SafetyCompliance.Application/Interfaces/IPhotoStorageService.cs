namespace SafetyCompliance.Application.Interfaces;

/// <summary>
/// Abstracts physical file save/delete for inspection photos.
/// Implemented differently per host (Web = local wwwroot, MAUI = app data dir).
/// </summary>
public interface IPhotoStorageService
{
    /// <summary>
    /// Saves a photo stream and returns the web-accessible relative URL (e.g. "/uploads/inspections/42/abc.jpg").
    /// </summary>
    Task<string> SavePhotoAsync(Stream stream, string originalFileName, int equipmentInspectionId);

    /// <summary>
    /// Deletes the physical file at the given web-relative path. No-op if the file does not exist.
    /// </summary>
    Task DeletePhotoAsync(string webPath);
}
