using System.Windows.Controls;
using PPS.ViewModels;

namespace PPS.Services;

/// <summary>Simple ViewModel-based navigation service for the shell content area.</summary>
public class NavigationService
{
    private static NavigationService? _instance;
    public static NavigationService Instance => _instance ??= new NavigationService();

    public event Action<BaseViewModel>? Navigated;

    public void NavigateTo(BaseViewModel viewModel) => Navigated?.Invoke(viewModel);
}
