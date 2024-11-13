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
            return string.Empty; // Returns empty text if the value is not a date
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
