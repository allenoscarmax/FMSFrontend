using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace FMSFrontend.Converters
{
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value?.ToString()?.ToLower() ?? "unknown";

            return status switch
            {
                "na" => new SolidColorBrush(Colors.White),
                "alarm" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CF1916")),
                "stay" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3B407")),
                "running" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1DB945")),
                "disconnection" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#555555")),
                _ => new SolidColorBrush(Colors.LightGray),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
