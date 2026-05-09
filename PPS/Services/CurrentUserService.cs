using PPS.Models;

namespace PPS.Services;

/// <summary>Holds the currently authenticated user for the session lifetime.</summary>
public class CurrentUserService
{
    public User? CurrentUser { get; private set; }

    public bool IsLoggedIn => CurrentUser is not null;

    public bool IsAdmin => CurrentUser?.Role == UserRole.Admin;

    public void SetUser(User user) => CurrentUser = user;

    public void ClearUser() => CurrentUser = null;
}
