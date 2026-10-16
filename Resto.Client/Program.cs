using Microsoft.EntityFrameworkCore;
using Resto.Client.Components;
using Resto.Client.Data;
using Resto.Client.Services;

var builder =
    WebApplication.CreateBuilder(args);

// ============================================================
// RAZOR / BLAZOR
// ============================================================

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

// ============================================================
// DATABASE
// ============================================================

builder.Services.AddDbContextFactory<AppDbContext>(
    options =>
    {
        options.UseSqlServer(
            builder.Configuration
                .GetConnectionString(
                    "DefaultConnection"),
            sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(3);
            });
    });

var apiBaseUrl =
    builder.Configuration["ApiBaseUrl"];

builder.Services.AddHttpClient(
    "RestoAPI",
    client =>
    {
        client.BaseAddress =
            new Uri(apiBaseUrl!);
    });

// ============================================================
// APPLICATION SERVICES
// ============================================================

// IMPORTANT:
// ClientEncryptionService must be registered BEFORE
// AuthService because AuthService requires it.

builder.Services.AddScoped<ClientEncryptionService>();

builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<DataService>();

builder.Services.AddScoped<ChatbotService>();

// ============================================================
// BUILD APPLICATION
// ============================================================

var app = builder.Build();

// ============================================================
// DATABASE MIGRATION
// ============================================================

using (var scope =
       app.Services.CreateScope())
{
    var factory =
        scope.ServiceProvider
            .GetRequiredService<
                IDbContextFactory<AppDbContext>>();

    var crypto =
        scope.ServiceProvider
            .GetRequiredService<
                ClientEncryptionService>();

    await using var db =
        await factory.CreateDbContextAsync();

    // Apply pending migrations.
    await db.Database.MigrateAsync();

    // If you have DbSeeder:
    // await DbSeeder.SeedAsync(db, crypto);
}

// ============================================================
// HTTP PIPELINE
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Error",
        createScopeForErrors: true);

    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();