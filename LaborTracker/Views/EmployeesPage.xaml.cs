using LaborTracker.Models;
using LaborTracker.ViewModels;

namespace LaborTracker.Views
{
    public partial class EmployeesPage : ContentPage
    {
        public EmployeesPage(EmployeesViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Загружаем данные при отображении страницы
            if (BindingContext is EmployeesViewModel vm)
            {
                await vm.LoadEmployees();
            }
        }
        private async void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is Employee employee)
            {
                if (BindingContext is EmployeesViewModel vm)
                {
                    await vm.DeleteEmployeeCommand.ExecuteAsync(employee);
                }
            }
        }
    }
}