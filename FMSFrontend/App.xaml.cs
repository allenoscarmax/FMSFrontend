using FMSFrontend.Controls;
using FMSFrontend.Extensions;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Services.FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Helpers;
using FMSFrontend.Interfaces;
using FMSFrontend.Models;
using FMSFrontend.Services;
using FMSFrontend.ViewModels;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views;
using FMSFrontend.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Reflection;
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
            
            // === Services ===
            services.AddSingleton<IElectrodeService, ElectrodeService>();
            services.AddSingleton<IProbeService, ProbeService>();
            services.AddSingleton<IRfidService, RfidService>();
            services.AddSingleton<IRobotService, RobotService>();
            services.AddSingleton<IPlcService, PlcService>();
            services.AddSingleton<IWorkpieceService, WorkpieceService>();
            services.AddSingleton<IWorksheetsService, WorksheetsService>();
            services.AddSingleton<IMachinesService, MachinesService>();
            services.AddSingleton<IStorageService, StorageService>();
            services.AddSingleton<IDevicesService, DevicesService>();
            services.AddSingleton<IMongoDBService, MongoDBService>();
            services.AddSingleton<IServerHealthService, ServerHealthService>();
            services.AddSingleton<IWorkerService, WorkerService>();
            services.AddSingleton<IAppointmentMaintenanceService, AppointmentMaintenanceService>();
            services.AddSingleton<ICommandScheduleService, CommandScheduleService>();

            services.AddSingleton<IMachinesService, MachinesService>();

            // === Singleton ===
            services.AddSingleton<PlcStore>();
            services.AddSingleton<RFIDBindStore>();
            services.AddSingleton<RobotStore>();
            services.AddSingleton<GlobalProperties>();
            services.AddSingleton<StorageStore>();
            services.AddSingleton<MachineStore>();

            // ==LiveUpdater===
            services.AddSingleton<RobotLiveUpdater>();
            services.AddSingleton<PlcLiveUpdater>();          
            services.AddSingleton<RFIDBindLiveUpdater>();
            services.AddSingleton<ServerHealthLiveUpdater>();
            services.AddSingleton<StorageLiveUpdater>();
            services.AddSingleton<MachineLiveUpdater>();
            #endregion


            // === ViewModel 註冊 ===
            RegisterViewModels(services);

            // === View 註冊 ===
            RegisterViews(services);

            // === Window 註冊 ===
        //    RegisterWindows(services);


            // === MainWindow ===
            services.AddSingleton<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();

            // 啟動手臂資訊輪詢（只要啟一次）
            ServiceProvider.GetRequiredService<RobotLiveUpdater>().Start();
            ServiceProvider.GetRequiredService<PlcLiveUpdater>().Start();
            ServiceProvider.GetRequiredService<ServerHealthLiveUpdater>().Start();

            // 啟動主視窗
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        private static void RegisterAllSingletons(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var types = assembly.GetTypes()
                .Where(t => !t.IsInterface && !t.IsAbstract &&
                       (t.Name.EndsWith("Service") ||
                        t.Name.EndsWith("Store") ||
                        t.Name.EndsWith("LiveUpdater")));

            foreach (var implType in types)
            {
                var interfaceType = implType.GetInterfaces().FirstOrDefault();
                if (interfaceType != null)
                {
                    services.AddSingleton(interfaceType, implType);
                }
                else
                {
                    services.AddSingleton(implType);  // 沒有介面也直接註冊自己
                }
            }
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
            services.AddTransient<MaterialPairViewModel>();
            services.AddTransient<ProbePairViewModel>();
            services.AddTransient<ShowMaterialWindowViewModel>();

            //Windows
            //  services.AddTransient<SelectSharedElectrodeViewModel>();
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
            services.AddTransient<MaterialPairWindow>();
            services.AddTransient<ProbePairWindow>();
            services.AddTransient<ShowMaterialWindow>();
            services.AddTransient<MachineMainDetailControl>();

        }

        private void RegisterWindows(IServiceCollection services)
        {
            services.AddTransient<SelectSharedElectrodeWindow>();
        }

    }
}
