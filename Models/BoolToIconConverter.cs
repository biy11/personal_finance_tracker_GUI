using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace PersonalFinanceTracker.Converters
{
    public class BoolToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isIncome)
            {
                return isIncome ? "\uf155" : "\uf153"; // FontAwesome icons for money in and out
            }
            return "\uf128"; // Default icon
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}