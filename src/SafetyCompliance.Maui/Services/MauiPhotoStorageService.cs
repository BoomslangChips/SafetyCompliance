using SafetyCompliance.Application.Interfaces;

namespace SafetyCompliance.Maui.Services;

/// <summary>
/// MAUI implementation: stores photos in the app's local data directory and
/// serves them via a custom scheme or base64 data URI.
/// TODO: integrate with MAUI's file provider / MediaPicker for full camera support.
/// </summary>
public class MauiPhotoStorageService : IPhotoStorageService
{
    public async Task<string> SavePhotoAsync(Stream stream, string originalFileName, int equipmentInspectionId)
    {
        var ext      = Path.GetExtension(originalFileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var folder   = Path.Combine(FileSystem.AppDataDirectory, "uploads", "inspections", equipmentInspectionId.ToString());
        Directory.CreateDirectory(folder);

        var physicalPath = Path.Combine(folder, fileName);
        await using var fs = new FileStream(physicalPath, FileMode.Create);
        await stream.CopyToAsync(fs);

        // Return a relative path that MAUI's BlazorWebView can resolve
        return $"/uploads/inspections/{equipmentInspectionId}/{fileName}";
    }

    public Task DeletePhotoAsync(string webPath)
    {
        // webPath is relative; resolve against AppDataDirectory
        var relative = webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var physical = Path.Combine(FileSystem.AppDataDirectory, relative);
        if (File.Exists(physical)) File.Delete(physical);
        return Task.CompletedTask;
    }
}
