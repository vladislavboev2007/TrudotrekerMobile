using LaborTracker.Services;

namespace LaborTracker.Views
{
    public partial class SplashPage : ContentPage
    {
        private readonly DatabaseService _dbService;

        public SplashPage(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;

            // Загружаем данные при создании страницы
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                // Пауза для показа splash-экрана
                await Task.Delay(1000);

                StatusLabel.Text = "Инициализация базы данных...";

                // Проверяем соединение с базой данных
                bool isConnected = await _dbService.CheckConnectionAsync();

                if (isConnected)
                {
                    StatusLabel.Text = "✅ База данных готова";
                    StatusLabel.TextColor = Colors.Green;

                    // Пауза перед переходом
                    await Task.Delay(1000);

                    // Переходим на главный экран
                    if (Application.Current != null && Application.Current.Windows != null && Application.Current.Windows.Count > 0)
                    {
                        Application.Current.Windows[0].Page = new AppShell();
                    }
                }
                else
                {
                    StatusLabel.Text = "❌ Ошибка подключения к БД";
                    StatusLabel.TextColor = Colors.Red;

                    // Можно показать кнопку повтора
                    await ShowRetryButton();
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"❌ Ошибка: {ex.Message}";
                StatusLabel.TextColor = Colors.Red;
                await ShowRetryButton();
            }
        }

        private async Task ShowRetryButton()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var retryButton = new Button
                {
                    Text = "Повторить",
                    BackgroundColor = Color.FromArgb("#4CAF50"),
                    TextColor = Colors.White,
                    CornerRadius = 10,
                    HeightRequest = 50,
                    WidthRequest = 150
                };

                retryButton.Clicked += async (s, e) =>
                {
                    LoadData();
                };

                var stackLayout = this.Content as VerticalStackLayout;
                if (stackLayout != null)
                {
                    stackLayout.Add(retryButton);
                }
            });
        }
    }
}