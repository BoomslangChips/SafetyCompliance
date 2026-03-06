using SafetyCompliance.Application;
using SafetyCompliance.Application.Interfaces;
using SafetyCompliance.Infrastructure;
using SafetyCompliance.Maui.Services;
using Microsoft.AspNetCore.Components.Authorization;
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

        // Use mobile-safe registration (DbContext + repos, no Identity)
        builder.Services.AddInfrastructureMobile(builder.Configuration);
        builder.Services.AddApplication();
        builder.Services.AddScoped<IPhotoStorageService, MauiPhotoStorageService>();

        // Auth state for Blazor AuthorizeView (no ASP.NET Identity needed)
        builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<AuthenticationStateProvider, MobileAuthStateProvider>();
        builder.Services.AddCascadingAuthenticationState();

        return builder.Build();
    }
}
