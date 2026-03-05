using LaborTracker.Models;
using LaborTracker.ViewModels;

namespace LaborTracker.Views
{
    public partial class TasksPage : ContentPage
    {
        public TasksPage(TasksViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // ��������� ������ ��� ����������� ��������
            if (BindingContext is TasksViewModel vm)
            {
                vm.LoadTasksCommand.ExecuteAsync(null);
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (BindingContext is TasksViewModel vm)
            {
                vm.SearchText = e.NewTextValue;
                vm.ApplyFilter();
            }
        }

        private async void OnStartButtonClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is TaskItem task)
            {
                if (BindingContext is TasksViewModel vm)
                {
                    await vm.StartTaskCommand.ExecuteAsync(task);
                }
            }
        }

        private async void OnStopButtonClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is TaskItem task)
            {
                if (BindingContext is TasksViewModel vm)
                {
                    await vm.StopTaskCommand.ExecuteAsync(task);
                }
            }
        }

        private async void OnInfoButtonClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is TaskItem task)
            {
                if (BindingContext is TasksViewModel vm)
                {
                    await vm.InfoTaskCommand.ExecuteAsync(task);
                }
            }
        }

        private async void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is TaskItem task)
            {
                if (BindingContext is TasksViewModel vm)
                {
                    await vm.DeleteTaskCommand.ExecuteAsync(task);
                }
            }
        }
    }
}