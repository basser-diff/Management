using Microsoft.EntityFrameworkCore;
using Resto.API.Data;
using Resto.API.Services;

var builder = WebApplication.CreateBuilder(args);

// ======================================
// DATABASE
// ======================================

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

// ======================================
// SERVICES
// ======================================

builder.Services.AddScoped<AuthService>();

builder.Services.AddSingleton<EncryptionService>();

builder.Services.AddSingleton<CardValidationService>();

// ======================================
// CONTROLLERS
// ======================================

builder.Services.AddControllers();

// ======================================
// SWAGGER
// ======================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

// ======================================
// CORS
// ======================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// ======================================
// SWAGGER
// ======================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

// ======================================
// HTTPS
// ======================================

app.UseHttpsRedirection();

// ======================================
// CORS
// ======================================

app.UseCors("AllowClient");

// ======================================
// AUTHORIZATION
// ======================================

app.UseAuthorization();

// ======================================
// CONTROLLERS
// ======================================

app.MapControllers();

app.Run();