using Microsoft.AspNetCore.Hosting;
using SafetyCompliance.Application.Interfaces;

namespace SafetyCompliance.Web.Services;

/// <summary>
/// Stores inspection photos in wwwroot/uploads/inspections/{id}/ so they are
/// served automatically by app.UseStaticFiles().
/// Falls back to ContentRootPath/wwwroot if WebRootPath is null (no wwwroot folder at startup).
/// </summary>
public class LocalPhotoStorageService(IWebHostEnvironment env) : IPhotoStorageService
{
    // WebRootPath is null when there is no wwwroot folder present at startup.
    // We resolve it ourselves so the first upload also creates the folder.
    private string WebRoot => env.WebRootPath
        ?? Path.Combine(env.ContentRootPath, "wwwroot");

    public async Task<string> SavePhotoAsync(Stream stream, string originalFileName, int equipmentInspectionId)
    {
        var ext      = Path.GetExtension(originalFileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var folder   = Path.Combine(WebRoot, "uploads", "inspections", equipmentInspectionId.ToString());
        Directory.CreateDirectory(folder);

        var physicalPath = Path.Combine(folder, fileName);
        await using var fs = new FileStream(physicalPath, FileMode.Create);
        await stream.CopyToAsync(fs);

        return $"/uploads/inspections/{equipmentInspectionId}/{fileName}";
    }

    public Task DeletePhotoAsync(string webPath)
    {
        var physical = Path.Combine(
            WebRoot,
            webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(physical))
            File.Delete(physical);

        return Task.CompletedTask;
    }

    public async Task<byte[]?> ReadPhotoAsync(string webPath)
    {
        var physical = Path.Combine(
            WebRoot,
            webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        if (!File.Exists(physical))
            return null;

        return await File.ReadAllBytesAsync(physical);
    }
}
