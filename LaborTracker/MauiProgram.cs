using LaborTracker;
using LaborTracker.Services;
using LaborTracker.ViewModels;
using LaborTracker.Views;
using Microsoft.Extensions.Logging;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                // Основные шрифты
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

                // Montserrat шрифты для Android
                fonts.AddFont("montserrat_regular.ttf", "Montserrat");
                fonts.AddFont("montserrat_bold.ttf", "Montserrat-Bold");
                fonts.AddFont("montserrat_medium.ttf", "Montserrat-Medium");
            });

        // Регистрация сервисов
        builder.Services.AddSingleton<DatabaseService>();

        // ViewModels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<TasksViewModel>();
        builder.Services.AddTransient<EmployeesViewModel>();

        // Pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<TasksPage>();
        builder.Services.AddTransient<EmployeesPage>();
        builder.Services.AddTransient<AddTaskPage>();
        builder.Services.AddTransient<SplashPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}