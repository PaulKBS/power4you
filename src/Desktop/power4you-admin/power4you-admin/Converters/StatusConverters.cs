using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace power4you_admin.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double powerOutput)
            return new SolidColorBrush(Colors.Gray);

        // This is a simplified conversion - in reality you'd need the total capacity
        // For now, we'll use basic thresholds
        return powerOutput switch
        {
            >= 1000 => new SolidColorBrush(Colors.Green),
            >= 500 => new SolidColorBrush(Colors.Orange),
            >= 100 => new SolidColorBrush(Colors.Red),
            _ => new SolidColorBrush(Colors.Gray)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StatusToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double powerOutput)
            return "No Data";

        return powerOutput switch
        {
            >= 1000 => "Good",
            >= 500 => "Medium",
            >= 100 => "Low",
            _ => "Very Low"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 