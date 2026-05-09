using System.Globalization;
using System.Windows;
using System.Windows.Data;
using PPS.Models;

namespace PPS.Converters;

[ValueConversion(typeof(decimal), typeof(string))]
public class DecimalToCurrencyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is decimal d ? $"₱{d:N2}" : "₱0.00";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => decimal.TryParse(value?.ToString()?.Replace("₱", "").Replace(",", ""),
               out var d) ? d : 0m;
}

[ValueConversion(typeof(object), typeof(Visibility))]
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is null ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

[ValueConversion(typeof(PayPeriod), typeof(string))]
public class PayPeriodToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is PayPeriod p
            ? p == PayPeriod.FirstHalf ? "1st Half (1–15)" : "2nd Half (16–End)"
            : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

[ValueConversion(typeof(Enum), typeof(string))]
public class EnumToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value?.ToString() ?? string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

[ValueConversion(typeof(bool), typeof(string))]
public class BoolToStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? "Active" : "Inactive";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

[ValueConversion(typeof(LoanStatus), typeof(string))]
public class LoanStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value switch
        {
            LoanStatus.Active    => "#4CAF50",
            LoanStatus.FullyPaid => "#2196F3",
            LoanStatus.Defaulted => "#F44336",
            _                    => "#9E9E9E"
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
