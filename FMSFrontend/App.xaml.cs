using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
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

            // === 共用服務層 ===
            services.AddSingleton<IHttpService, HttpService>();
            services.AddSingleton<IWindowService, WindowService>();

            #region RestoreSingleton
            // === Robot ===
            services.AddSingleton<RobotStore>();
            services.AddSingleton<IRobotService, RobotService>();
            services.AddSingleton<RobotLiveUpdater>();

            //==PLC ===
            services.AddSingleton<PlcStore>();
            services.AddSingleton<IPlcService, PlcService>();
            services.AddSingleton<PlcLiveUpdater>();

            //RFIDBindData 
            services.AddSingleton<RFIDBindStore>();
            services.AddSingleton<IRFIDMgmtModuleService, RFIDMgmtModuleService>();
            services.AddSingleton<RFIDBindLiveUpdater>();

            #endregion


            // === ViewModel 註冊 ===
            RegisterViewModels(services);

            // === View 註冊 ===
            RegisterViews(services);


            // === MainWindow ===
            services.AddSingleton<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();

            // 啟動手臂資訊輪詢（只要啟一次）
            ServiceProvider.GetRequiredService<RobotLiveUpdater>().Start();
            ServiceProvider.GetRequiredService<PlcLiveUpdater>().Start();
            ServiceProvider.GetRequiredService<RFIDBindLiveUpdater>().Stop();
            // 啟動主視窗
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void RegisterViewModels(IServiceCollection services)
        {
            // 單例 ViewModel（跨頁共用資料）
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<AlarmPageViewModel>();

            // 一般頁面 (Transient，每次開啟新頁面會重新建立)
            services.AddTransient<FactoryOverviewPageViewModel>();
            services.AddTransient<InventoryInformationViewModel>();
            services.AddTransient<MachineOverviewMainViewModel>();
            services.AddTransient<MachineMainDetailViewModel>();
            services.AddTransient<OperationHistoryViewModel>();
            services.AddTransient<ProductionLinesViewModel>();
            services.AddTransient<RFIDBindPageViewModel>();
            services.AddTransient<SettingsPageViewModel>();
            services.AddTransient<WorkOrderPageViewModel>();

            // 細節頁或子頁面
            services.AddTransient<ElectrodeDetailViewModel>();
            services.AddTransient<WorkpieceDetailViewModel>();
            services.AddTransient<EmptyMaterialDetailViewModel>();
            services.AddTransient<TimelineItemViewModel>();
            services.AddTransient<StorageUnitControlPageViewModel>();
        }
        private void RegisterViews(IServiceCollection services)
        {
            services.AddTransient<AlarmPage>();
            services.AddTransient<FactoryOverviewPage>();
            services.AddTransient<InventoryInformationPage>();
            services.AddTransient<MachineOverviewPage>();
            services.AddTransient<Manual>();
            services.AddTransient<OperationHistory>();
            services.AddTransient<ProductionLines>();
            services.AddTransient<RFIDBind>();
            services.AddTransient<SettingsView>();
            services.AddTransient<WorkOrder>();
        }

    }
}
