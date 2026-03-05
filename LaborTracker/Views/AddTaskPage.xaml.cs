using LaborTracker.Models;
using LaborTracker.Services;
using System;

namespace LaborTracker.Views
{
    public partial class AddTaskPage : ContentPage
    {
        private readonly DatabaseService _dbService;

        public AddTaskPage(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;



            // ��������� ����������� �� ��
            LoadEmployees();
        }

        private async void LoadEmployees()
        {
            try
            {
                var employees = await _dbService.GetEmployeesAsync();
                EmployeePicker.Items.Clear();

                if (employees != null && employees.Count > 0)
                {
                    foreach (var employee in employees)
                    {
                        EmployeePicker.Items.Add(employee.FIO);
                    }
                    EmployeePicker.SelectedIndex = 0;
                }
                else
                {
                    await DisplayAlert("��������", "��� ��������� �����������. �������� ����������� �������.", "OK");
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("������", $"�� ������� ��������� �����������: {ex.Message}", "OK");
            }
        }

        // В файле Views/AddTaskPage.xaml.cs измените метод OnAddTaskClicked:

        private async void OnAddTaskClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskNameEntry.Text))
            {
                await DisplayAlert("Ошибка", "Введите название задачи", "OK");
                return;
            }

            if (EmployeePicker.SelectedIndex == -1)
            {
                await DisplayAlert("Ошибка", "Выберите исполнителя", "OK");
                return;
            }

            try
            {
                // Получаем выбранного сотрудника
                var employees = await _dbService.GetEmployeesAsync();
                var selectedEmployeeName = EmployeePicker.SelectedItem?.ToString();
                var selectedEmployee = employees?.FirstOrDefault(emp =>
                    emp.FIO == selectedEmployeeName);

                if (selectedEmployee == null)
                {
                    await DisplayAlert("Ошибка", "Не удалось найти выбранного сотрудника", "OK");
                    return;
                }

                var task = new TaskItem
                {
                    Name = TaskNameEntry.Text.Trim(),
                    Description = DescriptionEditor.Text?.Trim() ?? string.Empty,
                    Date = DateTime.Today,
                    EmpId = selectedEmployee.EmpId,
                    Status = "Не начата"
                };

                bool success = await _dbService.AddTaskAsync(task);

                if (success)
                {
                    // Исправлено: явно указываем текст сообщения
                    await DisplayAlert("✅ Успех", "Задача успешно добавлена", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await DisplayAlert("❌ Ошибка", "Не удалось добавить задачу", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("❌ Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}