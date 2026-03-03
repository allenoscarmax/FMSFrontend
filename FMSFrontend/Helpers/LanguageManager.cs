using System;
using System.Linq;
using IniFile;
using System.Windows;

namespace FMSFrontend.Helpers
{
    public static class LanguageManager
    {
        public static string GetString(string key, string fallback)
        {
            if (Application.Current != null && Application.Current.TryFindResource(key) is string value)
            {
                return value;
            }

            return fallback;
        }

        public static string ApplySavedLanguage()
        {
            INIFile ini = new INIFile(AppDomain.CurrentDomain.BaseDirectory + "\\Basesitting.ini");
            var languageValue = ini.Read("Param", "Language");
            var selectedLanguage = languageValue == "1"
                ? "English"
                : languageValue == "2"
                    ? "日本語"
                    : "繁體中文";
            ApplyLanguageResource(selectedLanguage);
            return selectedLanguage;
        }

        public static void ApplyLanguageResource(string language)
        {
            // Map language to the corresponding resource dictionary
            var resourcePath = language == "English"
                ? "pack://application:,,,/FMSFrontend;component/Resources/Strings.en-US.xaml"
                : language == "日本語"
                    ? "pack://application:,,,/FMSFrontend;component/Resources/Strings.ja-JP.xaml"
                    : "pack://application:,,,/FMSFrontend;component/Resources/Strings.zh-TW.xaml";

            var appResources = Application.Current?.Resources;
            if (appResources?.MergedDictionaries == null)
                return;

            var existing = appResources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null
                    && d.Source.OriginalString.Contains("Resources/Strings.", StringComparison.OrdinalIgnoreCase));

            var languageDictionary = new ResourceDictionary
            {
                Source = new Uri(resourcePath, UriKind.Absolute)
            };

            if (existing != null)
            {
                var index = appResources.MergedDictionaries.IndexOf(existing);
                appResources.MergedDictionaries[index] = languageDictionary;
            }
            else
            {
                appResources.MergedDictionaries.Add(languageDictionary);
            }
        }
    }
}

