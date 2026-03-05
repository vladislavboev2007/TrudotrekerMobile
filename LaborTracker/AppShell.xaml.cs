using LaborTracker.Views;
using System;
using System.Threading.Tasks;

namespace LaborTracker
{
    public partial class AppShell : Shell
    {
        private bool _isInitialized = false;

        public AppShell()
        {
            try
            {
                InitializeComponent();

                // Регистрируем маршруты
                Routing.RegisterRoute("AddTaskPage", typeof(AddTaskPage));

                Console.WriteLine("✅ AppShell успешно инициализирован");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка инициализации AppShell: {ex.Message}");
                throw;
            }
        }

        protected override async void OnNavigated(ShellNavigatedEventArgs args)
        {
            base.OnNavigated(args);

            // Загружаем данные при первом переходе
            if (!_isInitialized && args.Current?.Location?.OriginalString != null)
            {
                _isInitialized = true;
                await InitializeDataAsync();
            }
        }

        private async Task InitializeDataAsync()
        {
            try
            {
                Console.WriteLine("🔄 Начальная загрузка данных...");

                // Здесь можно инициализировать данные, если нужно
                // Например, проверить соединение с БД

                Console.WriteLine("✅ Начальная инициализация завершена");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Ошибка начальной инициализации: {ex.Message}");
            }
        }
    }
}