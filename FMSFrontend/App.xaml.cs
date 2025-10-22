using FMSFrontend.Helpers;
using FMSFrontend.Interfaces;
using FMSFrontend.Services;
using FMSFrontend.ViewModels;
using FMSFrontend.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;

namespace FMSFrontend
{
    public partial class App : Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

#if DEBUG
            string lastTheme = ThemeManager.LoadLastTheme();

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                // 設計時用 Light，避免錯誤
                ThemeManager.ApplyTheme("Light");
            }
            else
            {
                ThemeManager.ApplyTheme(lastTheme);
            }
#else
    ThemeManager.ApplyTheme(ThemeManager.LoadLastTheme());
#endif

            var services = new ServiceCollection();

            // 原本的 typed client 會要求建構子有 HttpClient → 改用一般 Singleton
            // services.AddHttpClient<IHttpService, HttpService>();
            services.AddSingleton<IHttpService, HttpService>();

            // Services
            services.AddSingleton<IWindowService, WindowService>();

            // 註冊 ViewModel
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<AlarmPageViewModel>();          // 需要共用狀態 → Singleton

            // 註冊 MainWindow，讓 DI 可以注入 ViewModel
            services.AddSingleton<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
