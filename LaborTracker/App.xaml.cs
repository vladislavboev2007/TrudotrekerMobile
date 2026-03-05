using LaborTracker.Services;
using LaborTracker.Views;
using Microsoft.Maui.Controls.Platform;

namespace LaborTracker
{
    public partial class App : Application
    {
        private readonly DatabaseService _dbService;

        public App(DatabaseService dbService)
        {
            _dbService = dbService;

            InitializeComponent();

            // НЕ устанавливаем MainPage здесь!
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException!;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException!;

#if ANDROID
            Microsoft.Maui.ApplicationModel.Platform.ActivityStateChanged += OnActivityStateChanged!;
#endif
        }

        // ТОЛЬКО ЭТОТ МЕТОД создает окно
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new SplashPage(_dbService))
            {
                Title = "Трудотрекер",
                Width = 1200,
                Height = 800
            };
        }

#if ANDROID
        private void OnActivityStateChanged(object sender, Microsoft.Maui.ApplicationModel.ActivityStateChangedEventArgs e)
        {
            if (e.State == Microsoft.Maui.ApplicationModel.ActivityState.Resumed)
            {
                var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;

                // Проверяем, что это AppCompatActivity
                if (activity is AndroidX.AppCompat.App.AppCompatActivity appCompatActivity)
                {
                    // Устанавливаем флаги безопасности
                    if (appCompatActivity.Window != null)
                    {
                        appCompatActivity.Window.SetFlags(
                            Android.Views.WindowManagerFlags.Secure,
                            Android.Views.WindowManagerFlags.Secure);
                    }

                    // Скрываем ActionBar
                    var supportActionBar = appCompatActivity.SupportActionBar;
                    if (supportActionBar != null)
                    {
                        supportActionBar.Hide();
                    }
                }
            }
        }
#endif

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            Console.WriteLine($"❌ НЕОБРАБОТАННОЕ ИСКЛЮЧЕНИЕ: {exception?.Message}");

            // Безопасный показ алерта
            MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    if (Current != null && Current.Windows != null &&
                        Current.Windows.Count > 0 && Current.Windows[0].Page != null)
                    {
                        await Current.Windows[0].Page.DisplayAlert(
                            "Критическая ошибка",
                            $"Произошла ошибка: {exception?.Message}",
                            "OK");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Ошибка в обработчике ошибок: {ex.Message}");
                }
            });
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            if (e.Exception != null)
            {
                Console.WriteLine($"❌ НЕОБРАБОТАННОЕ ИСКЛЮЧЕНИЕ ЗАДАЧИ: {e.Exception.Message}");
            }
            e.SetObserved();
        }
    }
}