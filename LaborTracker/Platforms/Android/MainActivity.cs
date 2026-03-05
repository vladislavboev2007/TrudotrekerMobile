using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Runtime;

namespace LaborTracker
{
    [Activity(Theme = "@style/Maui.SplashTheme",
              MainLauncher = true,
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
              WindowSoftInputMode = SoftInput.AdjustResize)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Отключаем все защищенные флаги
            if (Window != null)
            {
                // Снимаем FLAG_SECURE если он есть
                Window.ClearFlags(WindowManagerFlags.Secure);

                // Настраиваем окно для разрешения скриншотов
                var attributes = new WindowManagerLayoutParams();
                attributes.Flags &= ~WindowManagerFlags.Secure;
                Window.Attributes = attributes;

                // Показываем системные панели
                Window.DecorView.SystemUiFlags = SystemUiFlags.LayoutStable;

                // Для Android 12+ дополнительные настройки
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
                {
                    // Разрешаем скриншоты явно
                    Android.Util.Log.Debug("LaborTracker", "Android 12+ обнаружен, применяем доп. настройки");
                }
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            // Каждый раз при возобновлении снимаем Secure флаг
            if (Window != null)
            {
                Window.ClearFlags(WindowManagerFlags.Secure);

                // Проверяем, что флаг действительно снят
                var flags = Window.Attributes.Flags;
                if ((flags & WindowManagerFlags.Secure) != 0)
                {
                    Android.Util.Log.Warn("LaborTracker", "⚠️ Secure флаг всё ещё активен! Принудительное снятие...");

                    // Альтернативный метод снятия
                    RunOnUiThread(() => {
                        Window.SetFlags(0, WindowManagerFlags.Secure);
                    });
                }
                else
                {
                    Android.Util.Log.Info("LaborTracker", "✅ Secure флаг успешно снят, скриншоты разрешены");
                }
            }
        }
    }
}