using SafetyCompliance.Application.Interfaces;

namespace SafetyCompliance.Maui.Services;

public class MauiNativeThemeService : INativeThemeService
{
    public void ApplyTheme(bool isDark)
    {
        MainPage.Instance?.ApplyTheme(isDark);
    }
}
