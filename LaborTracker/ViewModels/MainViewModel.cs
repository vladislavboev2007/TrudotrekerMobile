using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaborTracker.Services;

namespace LaborTracker.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        private readonly DatabaseService _dbService;

        [ObservableProperty]
        private int _totalTasks;

        [ObservableProperty]
        private int _completedTasks;

        [ObservableProperty]
        private string _formattedWorkTime = "00:00";

        public MainViewModel(DatabaseService dbService)
        {
            _dbService = dbService;
            Title = "Главная";
            Console.WriteLine("✅ MainViewModel создан");

            // Загружаем данные асинхронно
            Task.Run(async () => await LoadData());
        }

        [RelayCommand]
        public async Task LoadData()
        {
            try
            {
                Console.WriteLine("🔄 Загрузка данных на главной...");
                IsBusy = true;

                var stats = await _dbService.GetStatisticsAsync();

                // Обновляем свойства в главном потоке
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    TotalTasks = stats.total;
                    CompletedTasks = stats.completed;
                    FormattedWorkTime = $"{stats.totalTime.Hours:D2}:{stats.totalTime.Minutes:D2}";
                });

                Console.WriteLine($"✅ Данные загружены: {TotalTasks} задач");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка загрузки данных: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoToTasks()
        {
            try
            {
                Console.WriteLine("🔄 Переход к задачам...");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.GoToAsync("//TasksPage");
                    Console.WriteLine("✅ Успешный переход к задачам");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка перехода к задачам: {ex.Message}");
                await ShowAlertAsync("Ошибка",
                    $"Не удалось перейти к задачам: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task GoToEmployees()
        {
            try
            {
                Console.WriteLine("🔄 Переход к сотрудникам...");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.GoToAsync("//EmployeesPage");
                    Console.WriteLine("✅ Успешный переход к сотрудникам");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка перехода к сотрудникам: {ex.Message}");
                await ShowAlertAsync("Ошибка",
                    $"Не удалось перейти к сотрудникам: {ex.Message}");
            }
        }

        // Вспомогательный метод для безопасного показа диалогов
        private async Task ShowAlertAsync(string title, string message)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (Application.Current != null &&
                        Application.Current.Windows != null &&
                        Application.Current.Windows.Count > 0 &&
                        Application.Current.Windows[0].Page != null)
                    {
                        await Application.Current.Windows[0].Page.DisplayAlert(title, message, "OK");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка показа диалога: {ex.Message}");
            }
        }
    }
}