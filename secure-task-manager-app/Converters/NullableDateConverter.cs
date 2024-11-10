using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace secure_task_manager_app.Converters
{
    public class NullableDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ?? DateTime.Now; // Or DateTime.MinValue if you'd like
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                return dateTime == DateTime.MinValue ? null : (DateTime?)dateTime;
            }
            return null;
        }
    }
}
