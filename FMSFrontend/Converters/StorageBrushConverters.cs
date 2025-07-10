using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using FMSFrontend.ViewModels.Production; // 請根據你自己的專案調整命名空間

namespace FMSFrontend.Converters
{
    public class StorageTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value?.ToString() == "Workpiece")
                return (SolidColorBrush)(new BrushConverter().ConvertFrom("#E08E45"));
            return (SolidColorBrush)(new BrushConverter().ConvertFrom("#2779A7")); // Default to Electrode
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BoolToVisibilityConverter : IValueConverter
    {
        public bool Inverse { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)(value ?? false);
            if (Inverse)
                boolValue = !boolValue;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Visibility)value) == Visibility.Visible;
        }
    }
    public class ResultStatusToGeometryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value?.ToString())
            {
                case "CheckSuccess":
                    return Application.Current.TryFindResource("CheckIconGeometry");
                case "Checking":
                    return Application.Current.TryFindResource("CheckingIconGeometry");
                case "CheckFail":
                    return Application.Current.TryFindResource("CheckFailIconGeometry");
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
    public class CheckStatusToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value?.ToString())
            {
                case "Checked":
                    return Application.Current.TryFindResource("CheckedFileIcon");
                case "Unchecked":
                    return Application.Current.TryFindResource("UncheckfileIcon");
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }




    //public class SlotStatusToBrushConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value is not StorageSlotViewModel slot) return Brushes.Gray;

    //        if (slot.IsDisabled)
    //            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#555555"));
    //        if (slot.IsReserved)
    //            return new SolidColorBrush(Colors.White);
    //        if (slot.IsEmpty)
    //            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCCCCC"));

    //        return slot.WorkStatus switch
    //        {
    //            "Waiting" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3B407")),
    //            "Working" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1DB945")),
    //            "Abnormal" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CF1916")),
    //            "Done" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0877E3")),
    //            _ => Brushes.Transparent
    //        };
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    //}
}
