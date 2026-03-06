using SafetyCompliance.Application;
using SafetyCompliance.Application.Interfaces;
using SafetyCompliance.Infrastructure;
using SafetyCompliance.Maui.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace SafetyCompliance.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("SafetyCompliance.Maui.appsettings.json");

        if (stream is not null)
        {
            var config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();
            builder.Configuration.AddConfiguration(config);
        }

        // Data protection is required by ASP.NET Identity (token generation)
        // but is not auto-registered in MAUI like it is in ASP.NET Core web apps
        builder.Services.AddDataProtection()
            .SetApplicationName("SafeCheck")
            .PersistKeysToFileSystem(
                new DirectoryInfo(Path.Combine(FileSystem.AppDataDirectory, "dp-keys")));

        builder.Services.AddLogging();

        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddApplication();
        builder.Services.AddScoped<IPhotoStorageService, MauiPhotoStorageService>();

        return builder.Build();
    }
}
