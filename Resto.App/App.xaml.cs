using Resto.App.Services;

namespace Resto.App;

public partial class App : Application
{
    private readonly AppDataService _data;

    public App(AppDataService data)
    {
        InitializeComponent();
        _data = data;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = IPlatformApplication.Current!.Services.GetRequiredService<AppShell>();
        var window = new Window(shell);

        window.Created += async (s, e) =>
        {
            await _data.InitialiseAsync();
        };

        return window;
    }
}
