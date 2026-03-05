using CommunityToolkit.Mvvm.ComponentModel;

namespace LaborTracker.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private bool _isRefreshing;
    }
}