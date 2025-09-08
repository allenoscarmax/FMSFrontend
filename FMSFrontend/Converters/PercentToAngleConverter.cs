using System;
using System.Globalization;
using System.Windows.Data;

namespace FMSFrontend.Converters
{
    public class PercentToEndAngleConverter : IValueConverter
    {
        // 以 270 度為起點（上方 12 點鐘方向），向順時針增加
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 270d;
            double p = System.Convert.ToDouble(value);
            p = Math.Max(0, Math.Min(100, p));
            return 270d + 360d * (p / 100d);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class PercentToLargeArcConverter : IValueConverter
    {
        // >50% 就把 LargeArc 設為 true，ArcSegment 會走大弧
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;
            double p = System.Convert.ToDouble(value);
            return p > 50d;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
