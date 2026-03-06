namespace SafetyCompliance.Application.Interfaces;

/// <summary>
/// Optional service for MAUI to update native status bar color when theme changes.
/// Not registered in the web project.
/// </summary>
public interface INativeThemeService
{
    void ApplyTheme(bool isDark);
}
