namespace SafetyCompliance.Maui;

public partial class MainPage : ContentPage
{
    private static readonly Color LightBg = Color.FromArgb("#eef0f4");
    private static readonly Color DarkBg = Color.FromArgb("#0f1117");

    public static MainPage? Instance { get; private set; }

    public MainPage()
    {
        InitializeComponent();
        Instance = this;
        Padding = new Thickness(0, 48, 0, 0);
        BackgroundColor = LightBg;
    }

    /// <summary>Called from Blazor when theme changes.</summary>
    public void ApplyTheme(bool isDark)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            BackgroundColor = isDark ? DarkBg : LightBg;
#if ANDROID
            var window = Platform.CurrentActivity?.Window;
            if (window != null)
            {
                var color = isDark
                    ? Android.Graphics.Color.Rgb(15, 17, 23)
                    : Android.Graphics.Color.Rgb(238, 240, 244);
                window.SetStatusBarColor(color);

                var controller = AndroidX.Core.View.WindowCompat.GetInsetsController(window, window.DecorView);
                controller.AppearanceLightStatusBars = !isDark;
            }
#endif
        });
    }
}
