using Android.App;
using Android.Runtime;

namespace SafetyCompliance.Maui;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(nint handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
        AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
        {
            var ex = args.Exception;
            System.Diagnostics.Debug.WriteLine($"=== UNHANDLED EXCEPTION ===");
            System.Diagnostics.Debug.WriteLine($"Type: {ex.GetType().FullName}");
            System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
            if (ex.InnerException is not null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner: {ex.InnerException.GetType().FullName}");
                System.Diagnostics.Debug.WriteLine($"Inner Message: {ex.InnerException.Message}");
                System.Diagnostics.Debug.WriteLine($"Inner Stack: {ex.InnerException.StackTrace}");
            }
            Android.Util.Log.Error("SafeCheck", $"CRASH: {ex}");
        };
    }

    protected override MauiApp CreateMauiApp()
    {
        try
        {
            return MauiProgram.CreateMauiApp();
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("SafeCheck", $"CreateMauiApp FAILED: {ex}");
            System.Diagnostics.Debug.WriteLine($"=== CreateMauiApp FAILED ===\n{ex}");
            throw;
        }
    }
}
