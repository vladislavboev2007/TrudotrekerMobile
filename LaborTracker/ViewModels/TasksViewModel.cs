using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaborTracker.Models;
using LaborTracker.Views;
using LaborTracker.Services;
using System.Collections.ObjectModel;

namespace LaborTracker.ViewModels
{
    public partial class TasksViewModel : BaseViewModel
    {
        private readonly DatabaseService _dbService;

        [ObservableProperty]
        private ObservableCollection<TaskItem> _tasks = new();

        [ObservableProperty]
        private ObservableCollection<TaskItem> _filteredTasks = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedFilter = "Все";

        public TasksViewModel(DatabaseService dbService)
        {
            _dbService = dbService;
            Title = "Задачи";

            // Исправляем: запускаем загрузку асинхронно
            Task.Run(async () => await LoadTasks());
        }

        [RelayCommand]
        public void SearchTasks()
        {
            ApplyFilter();
        }

        [RelayCommand]
        public async Task LoadTasks()
        {
            try
            {
                IsBusy = true;
                Console.WriteLine("🔄 Загрузка задач из БД...");

                var tasks = await _dbService.GetTasksAsync();
                Console.WriteLine($"✅ Загружено {tasks?.Count ?? 0} задач");

                // Выводим в лог для отладки
                if (tasks != null)
                {
                    foreach (var task in tasks)
                    {
                        Console.WriteLine($"   - {task.Name} ({task.Status})");
                    }
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Tasks.Clear();
                    if (tasks != null)
                    {
                        foreach (var task in tasks)
                        {
                            Tasks.Add(task);
                        }
                    }
                    ApplyFilter();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка загрузки задач: {ex.Message}");
                await ShowAlertAsync("Ошибка", ex.Message);
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public void FilterTasks(string filterType)
        {
            SelectedFilter = filterType;
            ApplyFilter();
        }

        public void ApplyFilter()
        {
            if (Tasks == null) return;

            IEnumerable<TaskItem> filtered = Tasks;

            if (SelectedFilter == "В работе")
                filtered = filtered.Where(t => t.Status == "Выполняется");
            else if (SelectedFilter == "Завершены")
                filtered = filtered.Where(t => t.Status == "Выполнена");

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(t =>
                    (t.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (t.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (t.EmployeeName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)
                );
            }

            FilteredTasks = new ObservableCollection<TaskItem>(filtered);
        }

        [RelayCommand]
        private async Task AddTask()
        {
            try
            {
                Console.WriteLine("🔄 Переход к странице добавления задачи...");
                await Shell.Current.GoToAsync(nameof(AddTaskPage));
                Console.WriteLine("✅ Успешный переход");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка перехода: {ex.Message}");
                await ShowAlertAsync("Ошибка",
                    $"Не удалось перейти к добавлению задачи: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task StartTask(TaskItem task)
        {
            // ДОБАВЬТЕ эту строку для отладки
            Console.WriteLine($"🔘 StartTaskCommand ВЫЗВАНА для задачи: {task?.Name}");

            if (task == null) return;

            try
            {
                // Добавьте вызов в главный поток
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    // Используем методы TaskItem
                    task.StartTask();
                });

                // Обновляем в БД
                await _dbService.UpdateTaskAsync(task);

                // Перезагружаем список
                await LoadTasks();

                await ShowAlertAsync("Успех", $"Задача '{task.Name}' начата");
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Ошибка", $"Не удалось запустить задачу: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task StopTask(TaskItem task)
        {
            if (task == null) return;

            try
            {
                // Используем методы TaskItem для обновления
                task.StopTask();

                // Обновляем в базе данных
                await _dbService.UpdateTaskAsync(task);

                // Перезагружаем список задач
                await LoadTasks();

                await ShowAlertAsync("Успех", $"Задача '{task.Name}' завершена");
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Ошибка", $"Не удалось завершить задачу: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task InfoTask(TaskItem task)
        {
            if (task == null) return;

            string message = $"Название: {task.Name}\n" +
                             $"Описание: {task.Description}\n" +
                             $"Исполнитель: {task.EmployeeName}\n" +
                             $"Дата: {task.Date:dd.MM.yyyy}\n" +
                             $"Статус: {task.Status}\n" +
                             $"Начало: {task.Start?.ToString(@"hh\:mm") ?? "не начато"}\n" +
                             $"Завершение: {task.Final?.ToString(@"hh\:mm") ?? "не завершено"}\n" +
                             $"Длительность: {task.DurationMinutes}";

            await ShowAlertAsync("Подробности задачи", message);
        }

        [RelayCommand]
        private async Task DeleteTask(TaskItem task)
        {
            if (task == null) return;

            bool confirm = await ShowConfirmationAsync(
                "Подтверждение удаления",
                $"Вы уверены, что хотите удалить задачу \"{task.Name}\"?",
                "Удалить",
                "Отмена");

            if (confirm)
            {
                try
                {
                    // Удаляем задачу из базы данных
                    bool success = await _dbService.DeleteTaskAsync(task.TaskId);

                    if (success)
                    {
                        // Удаляем из коллекций
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            Tasks.Remove(task);
                            FilteredTasks.Remove(task);
                        });

                        await ShowAlertAsync("Успех", $"Задача \"{task.Name}\" удалена");
                    }
                    else
                    {
                        await ShowAlertAsync("Ошибка", "Не удалось удалить задачу из базы данных");
                    }
                }
                catch (Exception ex)
                {
                    await ShowAlertAsync("Ошибка", $"Не удалось удалить задачу: {ex.Message}");
                }
            }
        }

        // Вспомогательные методы для безопасного показа диалогов
        private async Task ShowAlertAsync(string title, string message)
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

        private async Task<bool> ShowConfirmationAsync(string title, string message, string accept, string cancel)
        {
            bool result = false;
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
            return result;
        }
    }
}