using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace FMSFrontend.Converters
{
    public class KeyDownEnterOnlyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 判斷是否是 Enter
            if (value is KeyEventArgs e && e.Key == Key.Enter)
                return null; // 讓 Command 執行
            return Binding.DoNothing; // 不執行 Command
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
