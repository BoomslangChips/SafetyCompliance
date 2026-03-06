namespace SafetyCompliance.Maui;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        // Push content below Android status bar
        BackgroundColor = Color.FromArgb("#4f46e5");
        Padding = new Thickness(0, 48, 0, 0);
    }
}
