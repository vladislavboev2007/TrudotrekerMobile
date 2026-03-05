using LaborTracker.ViewModels;

namespace LaborTracker.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainViewModel viewModel)
        {
            try
            {
                Console.WriteLine("🔄 Инициализация MainPage...");
                InitializeComponent();
                BindingContext = viewModel;
                Console.WriteLine("✅ MainPage успешно инициализирована");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка инициализации MainPage: {ex.Message}");
                throw;
            }
        }
    }
}