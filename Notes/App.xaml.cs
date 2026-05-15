using Microsoft.Maui.Storage;

namespace Notes;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Загружаем сохранённую тему при старте
        var savedTheme = Preferences.Get("app_theme", "system");
        Application.Current!.UserAppTheme = savedTheme switch
        {
            "light" => AppTheme.Light,
            "dark" => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };

        MainPage = new AppShell();
    }
}