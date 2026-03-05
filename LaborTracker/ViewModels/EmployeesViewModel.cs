using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaborTracker.Models;
using LaborTracker.Services;
using System.Collections.ObjectModel;

namespace LaborTracker.ViewModels
{
    public partial class EmployeesViewModel : BaseViewModel
    {
        private readonly DatabaseService _dbService;

        [ObservableProperty]
        private ObservableCollection<Employee> _employees = new();

        [ObservableProperty]
        private string _newEmployeeName = string.Empty;

        public EmployeesViewModel(DatabaseService dbService)
        {
            _dbService = dbService;
            Title = "Сотрудники";

            // Загружаем сотрудников асинхронно
            Task.Run(async () => await LoadEmployees());
        }

        public async Task LoadEmployees()
        {
            try
            {
                IsBusy = true;
                Console.WriteLine("🔄 Загрузка сотрудников из БД...");

                var employees = await _dbService.GetEmployeesAsync();
                Console.WriteLine($"✅ Загружено {employees?.Count ?? 0} сотрудников");

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Employees.Clear();
                    if (employees != null)
                    {
                        foreach (var employee in employees)
                        {
                            Employees.Add(employee);
                            Console.WriteLine($"   - {employee.FIO}");
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка загрузки сотрудников: {ex.Message}");
                await ShowAlertAsync("Ошибка",
                    $"Не удалось загрузить сотрудников: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task AddEmployee()
        {
            bool confirmed = await ShowConfirmationAsync(
                "Добавить сотрудника",
                "Введите ФИО нового сотрудника:",
                "Добавить",
                "Отмена",
                true); // true - показывать поле для ввода

            if (confirmed && !string.IsNullOrWhiteSpace(NewEmployeeName))
            {
                await AddNewEmployee(NewEmployeeName.Trim());
                NewEmployeeName = string.Empty;
            }
        }

        private async Task AddNewEmployee(string fio)
        {
            try
            {
                IsBusy = true;

                var employee = new Employee { FIO = fio };

                bool success = await _dbService.AddEmployeeAsync(employee);

                if (success)
                {
                    // Обновляем список
                    await LoadEmployees();

                    await ShowAlertAsync("Успех",
                        $"Сотрудник '{fio}' добавлен");
                }
                else
                {
                    await ShowAlertAsync("Ошибка",
                        "Не удалось добавить сотрудника");
                }
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Ошибка",
                    $"Ошибка при добавлении сотрудника: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteEmployee(Employee employee)
        {
            if (employee == null) return;

            try
            {
                // Проверяем, есть ли у сотрудника незавершенные задачи
                bool hasUnfinished = await _dbService.HasUnfinishedTasksAsync(employee.EmpId);

                if (hasUnfinished)
                {
                    // Показываем предупреждение
                    await ShowAlertAsync("⚠️ Невозможно удалить",
                        $"У сотрудника {employee.FIO} есть незавершенные задачи.\n\n" +
                        $"Сначала завершите или удалите все задачи сотрудника.");
                    return;
                }

                // Если нет незавершенных задач, спрашиваем подтверждение
                bool confirm = await ShowConfirmationAsync(
                    "Подтверждение удаления",
                    $"Вы уверены, что хотите удалить сотрудника {employee.FIO}?",
                    "Удалить",
                    "Отмена");

                if (confirm)
                {
                    bool success = await _dbService.DeleteEmployeeAsync(employee.EmpId);

                    if (success)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            Employees.Remove(employee);
                        });

                        await ShowAlertAsync("✅ Успех",
                            $"Сотрудник {employee.FIO} удален");
                    }
                    else
                    {
                        await ShowAlertAsync("❌ Ошибка",
                            "Не удалось удалить сотрудника из базы данных");
                    }
                }
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("❌ Ошибка",
                    $"Не удалось удалить сотрудника: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task Refresh()
        {
            await LoadEmployees();
        }

        // Вспомогательные методы для безопасного показа диалогов
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
                    else
                    {
                        // Fallback для отладки
                        Console.WriteLine($"[ALERT] {title}: {message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка показа диалога: {ex.Message}");
            }
        }

        private async Task<bool> ShowConfirmationAsync(string title, string message, string accept, string cancel, bool showInput = false)
        {
            bool result = false;

            try
            {
                if (showInput)
                {
                    // Диалог с вводом текста
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        if (Application.Current != null &&
                            Application.Current.Windows != null &&
                            Application.Current.Windows.Count > 0 &&
                            Application.Current.Windows[0].Page != null)
                        {
                            string input = await Application.Current.Windows[0].Page.DisplayPromptAsync(
                                title, message, accept, cancel,
                                "Иванов Иван Иванович", -1, Keyboard.Default);

                            if (!string.IsNullOrWhiteSpace(input))
                            {
                                NewEmployeeName = input;
                                result = true;
                            }
                        }
                    });
                }
                else
                {
                    // Обычное подтверждение
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        if (Application.Current != null &&
                            Application.Current.Windows != null &&
                            Application.Current.Windows.Count > 0 &&
                            Application.Current.Windows[0].Page != null)
                        {
                            result = await Application.Current.Windows[0].Page.DisplayAlert(
                                title, message, accept, cancel);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка показа диалога подтверждения: {ex.Message}");
            }

            return result;
        }
    }
}