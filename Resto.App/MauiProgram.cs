using Microsoft.Extensions.Logging;
using Resto.App.Data;
using Resto.App.Pages;
using Resto.App.Services;

namespace Resto.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf",  "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ── SQLite database path ──────────────────────────────────────────────
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "resto_app.db3");

        // ── Register services ─────────────────────────────────────────────────
        builder.Services.AddSingleton(_ => new AppDatabase(dbPath));
        builder.Services.AddSingleton<AppDataService>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<PaymentService>();

        // ── Register pages ────────────────────────────────────────────────────
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<MenuPage>();
        builder.Services.AddTransient<CartPage>();
        builder.Services.AddTransient<OrdersPage>();
        builder.Services.AddTransient<NotificationsPage>();
        builder.Services.AddTransient<PaymentPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
