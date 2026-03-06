using SafetyCompliance.Application;
using SafetyCompliance.Application.Interfaces;
using SafetyCompliance.Infrastructure;
using SafetyCompliance.Infrastructure.Data;
using SafetyCompliance.Maui.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
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

        builder.Services.AddLogging();

        // Local SQLite database (offline-first)
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "safecheck.db");
        builder.Services.AddInfrastructureMobile(dbPath);
        builder.Services.AddApplication();
        builder.Services.AddScoped<IPhotoStorageService, MauiPhotoStorageService>();
        builder.Services.AddSingleton<INativeThemeService, MauiNativeThemeService>();

        // Auth state for Blazor AuthorizeView (no ASP.NET Identity needed)
        builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<AuthenticationStateProvider, MobileAuthStateProvider>();
        builder.Services.AddCascadingAuthenticationState();

        // Sync service — connects to SQL Server to push/pull data
        var syncConnStr = builder.Configuration["SyncConnectionString"]
            ?? "Server=192.168.8.73\\SQLEXPRESS;Database=SafetyCompliance;User Id=sa;Password=smiles12;TrustServerCertificate=True;Connect Timeout=10";
        builder.Services.AddSingleton<ISyncService>(sp =>
            new MauiSyncService(sp.GetRequiredService<IServiceScopeFactory>(), syncConnStr));

        var app = builder.Build();

        // Ensure SQLite schema is created on first launch
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
        }

        return app;
    }
}
