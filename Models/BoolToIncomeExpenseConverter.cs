using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace PersonalFinanceTracker.Converters
{
    public class BoolToIncomeExpenseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isIncome)
            {
                return isIncome ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
