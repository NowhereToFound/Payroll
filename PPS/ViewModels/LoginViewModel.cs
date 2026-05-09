using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPS.Models;
using PPS.Services;

namespace PPS.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService       _auth;
    private readonly CurrentUserService _session;
    private readonly ISystemLogService  _logger;

    public override string Title => "Login";

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private bool   _isLoginFailed;

    /// <summary>Raised when login succeeds; Shell subscribes to open main window.</summary>
    public event Action<User>? LoginSucceeded;

    public LoginViewModel(IAuthService auth, CurrentUserService session, ISystemLogService logger)
    {
        _auth    = auth;
        _session = session;
        _logger  = logger;
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        IsBusy        = true;
        IsLoginFailed = false;
        ClearMessages();

        try
        {
            var user = await _auth.LoginAsync(Username.Trim(), Password);
            if (user is null)
            {
                IsLoginFailed = true;
                SetError("Invalid username or password.");
                await _logger.LogAsync("Login", $"Failed login attempt for username: '{Username.Trim()}'",
                    severity: LogSeverity.Warning);
                return;
            }

            _session.SetUser(user);
            await _logger.LogAsync("Login", $"User '{user.Username}' logged in successfully",
                details: $"Role: {user.Role}");
            LoginSucceeded?.Invoke(user);
        }
        finally
        {
            IsBusy   = false;
            Password = string.Empty;
        }
    }

    private bool CanLogin()
        => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password) && !IsBusy;

    partial void OnUsernameChanged(string value)  => LoginCommand.NotifyCanExecuteChanged();
    partial void OnPasswordChanged(string value)  => LoginCommand.NotifyCanExecuteChanged();
}
