using System;
using System.Globalization;
using System.Windows.Data;

namespace FMSFrontend.Converters
{
    public class BoolToIconConverter : IValueConverter
    {
        // 如果 true，顯示展開向下箭頭；false 顯示向右箭頭
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isExpanded = value is bool b && b;
            return isExpanded ? "ChevronDown" : "ChevronRight"; // 或 "MenuDown"/"MenuRight"
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
