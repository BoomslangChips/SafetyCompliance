using SafetyCompliance.Application;
using SafetyCompliance.Infrastructure;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace SafetyCompliance.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        var config = new ConfigurationBuilder()
            .AddJsonStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("SafetyCompliance.Maui.appsettings.json")!)
            .Build();

        builder.Configuration.AddConfiguration(config);

        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddApplication();

        return builder.Build();
    }
}
