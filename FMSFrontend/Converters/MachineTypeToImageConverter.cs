using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace FMSFrontend.Converters
{
    public class MachineTypeToImageConverter : IValueConverter
    {
        private const string BasePath = "pack://application:,,,/FMSFrontend;component/Image/MachineIcons/";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string machineType || string.IsNullOrWhiteSpace(machineType))
                return null;

            string imageFile = $"{machineType}.png";
            string uri = $"{BasePath}{imageFile}";

            try
            {
                return new BitmapImage(new Uri(uri, UriKind.Absolute));
            }
            catch
            {
                // fallback 圖示（可選）
                return null; // 或 new BitmapImage(new Uri(...其他安全圖片))
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}