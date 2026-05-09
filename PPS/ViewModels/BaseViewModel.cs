using CommunityToolkit.Mvvm.ComponentModel;

namespace PPS.ViewModels;

/// <summary>Base ViewModel — provides ObservableObject + common busy/error state.</summary>
public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _successMessage = string.Empty;

    public bool IsNotBusy => !IsBusy;

    public virtual string Title { get; } = string.Empty;

    protected void SetError(string message)
    {
        ErrorMessage   = message;
        SuccessMessage = string.Empty;
    }

    protected void SetSuccess(string message)
    {
        SuccessMessage = message;
        ErrorMessage   = string.Empty;
    }

    protected void ClearMessages()
    {
        ErrorMessage   = string.Empty;
        SuccessMessage = string.Empty;
    }
}
