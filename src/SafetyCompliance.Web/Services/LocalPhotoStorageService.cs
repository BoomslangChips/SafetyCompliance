using Microsoft.AspNetCore.Hosting;
using SafetyCompliance.Application.Interfaces;

namespace SafetyCompliance.Web.Services;

/// <summary>
/// Stores inspection photos in wwwroot/uploads/inspections/{id}/ so they are
/// served automatically by app.UseStaticFiles().
/// </summary>
public class LocalPhotoStorageService(IWebHostEnvironment env) : IPhotoStorageService
{
    public async Task<string> SavePhotoAsync(Stream stream, string originalFileName, int equipmentInspectionId)
    {
        var ext      = Path.GetExtension(originalFileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var folder   = Path.Combine(env.WebRootPath, "uploads", "inspections", equipmentInspectionId.ToString());
        Directory.CreateDirectory(folder);

        var physicalPath = Path.Combine(folder, fileName);
        await using var fs = new FileStream(physicalPath, FileMode.Create);
        await stream.CopyToAsync(fs);

        return $"/uploads/inspections/{equipmentInspectionId}/{fileName}";
    }

    public Task DeletePhotoAsync(string webPath)
    {
        var physical = Path.Combine(
            env.WebRootPath,
            webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(physical))
            File.Delete(physical);

        return Task.CompletedTask;
    }
}
