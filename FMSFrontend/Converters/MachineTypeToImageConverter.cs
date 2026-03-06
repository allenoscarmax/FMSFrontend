using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace FMSFrontend.Converters
{
    public class MachineTypeToImageConverter : IValueConverter
    {
        private const string BasePath = "pack://application:,,,/FMSFrontend;component/Image/MachineIcons/";
        private const string DefaultFile = "Default.png";

        // 依你專案內實際檔名補齊
        private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
        {
            ["EDM"] = "EDM.png",
            ["CNC"] = "CNC.png",
            ["FanucCNC"] = "CNC.png",
            ["SiemensCNC"] = "UH500.png",
            ["ZNC"] = "ZNC.png",
            ["ROBOT"] = "Robot.png",
            ["ICG-2Z-NC"] = "ICG-2Z-NC.png",
            // ... 需要再加
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 1) 取得 key：支援 enum 與 string
            string? key = value switch
            {
                null => null,
                Enum e => e.ToString(),
                string s => Normalize(s),
                _ => value.ToString()
            };

            // 2) 用對應表找檔名；找不到就用 key.png；還是找不到，就 Default.png
            string file =
                (key != null && Map.TryGetValue(key, out var mapped)) ? mapped :
                (!string.IsNullOrWhiteSpace(key) ? $"{key}.png" : DefaultFile);

            // 3) 組 pack URI
            var uri = new Uri($"{BasePath}{file}", UriKind.Absolute);

            try
            {
                // 可選：先試讀資源，若不存在則 fallback
                // using var _ = Application.GetResourceStream(uri); // 若不想提前打開，可省略

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = uri;
                bmp.CacheOption = BitmapCacheOption.OnLoad;     // 立刻載入，方便 Freeze
                bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bmp.EndInit();
                if (bmp.CanFreeze) bmp.Freeze();
                return bmp;
            }
            catch
            {
                // fallback
                try
                {
                    var fallback = new BitmapImage();
                    fallback.BeginInit();
                    fallback.UriSource = new Uri($"{BasePath}{DefaultFile}", UriKind.Absolute);
                    fallback.CacheOption = BitmapCacheOption.OnLoad;
                    fallback.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    fallback.EndInit();
                    if (fallback.CanFreeze) fallback.Freeze();
                    return fallback;
                }
                catch
                {
                    return null!;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            Binding.DoNothing;

        private static string Normalize(string s)
        {
            // 取第一段，去掉常見分隔符，並去空白
            // 例如： "EDM-01" -> "EDM", "CNC 03" -> "CNC"
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            s = s.Trim();
            int cut = s.IndexOfAny(new[] { '-', ' ', '_' });
            if (cut > 0) s = s[..cut];
            return s.ToUpperInvariant();
        }
    }
}
