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
            var languageValue = ini.Read("Prarm", "Language");
            var selectedLanguage = languageValue == "1" ? "English" : "ÁcÅé¤¤¤å";
            ApplyLanguageResource(selectedLanguage);
            return selectedLanguage;
        }

        public static void ApplyLanguageResource(string language)
        {
            var resourcePath = language == "English"
                ? "pack://application:,,,/FMSFrontend;component/Resources/Strings.en-US.xaml"
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
