using System.Globalization;

namespace LaborTracker.Converters
{
    public class TaskStatusColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "Выполнена" => Color.FromArgb("#E8F5E9"), // Зеленый светлый
                    "Выполняется" => Color.FromArgb("#FFF3E0"), // Оранжевый светлый
                    "Не начата" => Color.FromArgb("#FFEBEE"), // Красный светлый
                    _ => Color.FromArgb("#F5F5F5")
                };
            }
            return Color.FromArgb("#F5F5F5");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TaskStatusIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "Выполнена" => "info_icon.png",
                    "Выполняется" => "start_icon.png",
                    "Не начата" => "stop_icon.png",
                    _ => "info_icon.png"
                };
            }
            return "info_icon.png";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // ОТДЕЛЬНЫЙ КЛАСС ДЛЯ КОНВЕРТЕРА ФИЛЬТРОВ
    public class SelectedFilterConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string selectedFilter && parameter is string buttonFilter)
            {
                return selectedFilter == buttonFilter ? "#2196F3" : "#E0E0E0";
            }
            return "#E0E0E0";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusIndicatorColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "Выполнена" => Color.FromArgb("#4CAF50"), // Зеленый
                    "Выполняется" => Color.FromArgb("#FFC107"), // Желтый
                    "Не начата" => Color.FromArgb("#212121"), // Черный
                    _ => Color.FromArgb("#212121")
                };
            }
            return Color.FromArgb("#212121");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TaskBackgroundColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "Выполнена" => Color.FromArgb("#E8F5E9"), // Светло-зеленый
                    "Выполняется" => Color.FromArgb("#FFF3E0"), // Светло-оранжевый
                    _ => Color.FromArgb("#FFFFFF") // Белый для "Не начата"
                };
            }
            return Color.FromArgb("#FFFFFF");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}