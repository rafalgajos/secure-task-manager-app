using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace secure_task_manager_app.Converters
{
    public class DefaultDateVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                return dateTime == DateTime.MinValue ? string.Empty : dateTime.ToString("yyyy-MM-dd");
            }
            return string.Empty; // Zwraca pusty tekst, jeśli wartość nie jest datą
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
