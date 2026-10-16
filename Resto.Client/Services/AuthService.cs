using Resto.Client.Models;

namespace Resto.Client.Services;

public class AuthService
{
    public bool IsAuthenticated { get; private set; }

    public Manager? CurrentManager { get; private set; }

    public event Action? OnAuthStateChanged;


    // ============================================================
    // SET AUTHENTICATED MANAGER
    // ============================================================

    public void SetAuthenticatedManager(
        LoginResponse response)
    {
        CurrentManager = new Manager
        {
            Id = response.Id,
            FullName = response.FullName,
            Email = response.Email,
            Role = response.Role,

            // PasswordHash is NOT returned by the API.
            PasswordHash = string.Empty,

            // Phone is not returned by LoginResponse.
            Phone = string.Empty
        };

        IsAuthenticated = true;

        OnAuthStateChanged?.Invoke();
    }


    // ============================================================
    // LOGOUT
    // ============================================================

    public void Logout()
    {
        CurrentManager = null;

        IsAuthenticated = false;

        OnAuthStateChanged?.Invoke();
    }


    // ============================================================
    // UPDATE PROFILE
    // ============================================================

    public void UpdateProfile(
        string fullName,
        string phone)
    {
        if (CurrentManager == null)
            return;

        if (string.IsNullOrWhiteSpace(fullName))
            return;

        CurrentManager.FullName = fullName;

        CurrentManager.Phone = phone;

        OnAuthStateChanged?.Invoke();
    }
}