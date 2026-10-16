using Microsoft.EntityFrameworkCore;
using Resto.API.Data;
using Resto.API.Models;

namespace Resto.API.Services;

public class AuthService
{
    private readonly AppDbContext _db;

    public AuthService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Manager?> LoginAsync(
        string email,
        string password)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var manager = await _db.Managers
            .FirstOrDefaultAsync(x =>
                x.Email == email);

        if (manager == null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(
                manager.PasswordHash))
        {
            return null;
        }

        bool validPassword =
            BCrypt.Net.BCrypt.Verify(
                password,
                manager.PasswordHash);

        if (!validPassword)
        {
            return null;
        }

        return manager;
    }
}