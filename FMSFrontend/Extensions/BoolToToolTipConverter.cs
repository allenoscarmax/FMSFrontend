using System;
using System.Globalization;
using System.Windows.Data;

namespace FMSFrontend.Extensions
{
    public class BoolToToolTipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool canShare = value is bool b && b;
            return canShare ? "可分享" : "僅尾碼為 02 的電極可分享";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}