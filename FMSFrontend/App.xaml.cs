using System.Configuration;
using System.Data;
using System.Windows;
using FMSFrontend.Services;
using FMSFrontend.ViewModels;
using FMSFrontend.Views;
using Microsoft.Extensions.DependencyInjection;
using FMSFrontend.Helpers;

namespace FMSFrontend
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
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

            // 註冊 HttpClient
            services.AddHttpClient<IHttpService, HttpService>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:5032/");
            });

            // 註冊 ViewModel
            services.AddSingleton<MainWindowViewModel>();

            // 註冊 MainWindow，讓 DI 可以注入 ViewModel
            services.AddSingleton<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();

            // 透過 DI 建立 MainWindow
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }


}
