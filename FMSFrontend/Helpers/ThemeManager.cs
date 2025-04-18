using System.Configuration;
using System.IO;
using System.Windows;

namespace FMSFrontend.Helpers
{
    public static class ThemeManager
    {
        private static ResourceDictionary? _currentTheme;

        private static string _currentThemeName = "Light";

        public static string CurrentThemeName => _currentThemeName;

        private const string LightThemePath = "Themes/Colors.Light.xaml";
        private const string DarkThemePath = "Themes/Colors.Dark.xaml";

        private const string ThemeSettingFile = "theme.config";

        public static void ApplyTheme(string theme)
        {
            var themePath = theme == "Dark" ? DarkThemePath : LightThemePath;

            var newTheme = new ResourceDictionary
            {
                Source = new Uri(themePath, UriKind.Relative)
            };

            var appResources = Application.Current.Resources.MergedDictionaries;

            // 清除所有舊的主題（不只是 _currentTheme）
            for (int i = appResources.Count - 1; i >= 0; i--)
            {
                var dict = appResources[i];
                if (dict.Source != null &&
                    (dict.Source.OriginalString.Contains("Colors.Light.xaml") ||
                     dict.Source.OriginalString.Contains("Colors.Dark.xaml")))
                {
                    appResources.RemoveAt(i);
                }
            }

            appResources.Add(newTheme);
            _currentTheme = newTheme;
            _currentThemeName = theme;

            SaveTheme(theme);
        }

        public static string LoadLastTheme()
        {
            if (File.Exists(ThemeSettingFile))
            {
                var theme = File.ReadAllText(ThemeSettingFile).Trim();
                _currentThemeName = theme == "Dark" ? "Dark" : "Light"; // 🔸加這行
                return _currentThemeName;
            }

            return "Light"; // 預設
        }

        private static void SaveTheme(string theme)
        {
            File.WriteAllText(ThemeSettingFile, theme);
        }
    }
}