using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Extensions;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Models;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views;
using FMSFrontend.Views.Windows;
using IniFile;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Security.Claims;
using System.Text.Json; // ← 新增：JsonElement
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls; // 放在你的 ViewModel 上方
using System.Windows.Media;
using System.Windows.Threading;

namespace FMSFrontend.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        // 建議做成 static readonly，避免每次輪詢都 new
      
        private static readonly SolidColorBrush Dark = new(Color.FromRgb(0x00, 0x4E, 0x79));
        private static readonly SolidColorBrush Light = new(Color.FromRgb(0xFF, 0xFF, 0xFF));
        // 開啟頁面視窗（改為延遲建立：避免啟動時一次建立大量 UI）
        //private ProductionLines? productionLines ;

        private readonly IHttpService _httpService;
        private readonly IRobotService _robotService;



        public GlobalProperties _globalProperties { get; }
        public AlarmPageViewModel AlarmVM { get; }
        //警報
        private readonly IAlarmService _alarmService;
        public AlarmStore AlarmStore { get; }
        public AlarmGroupModel AlarmGroup => AlarmStore.AlarmGroup;

        [ObservableProperty] private bool _isMenuVisible;
        [ObservableProperty] private string currentDateTime = "";  //存現在的時間
        [ObservableProperty] private string _loggedInUser = string.Empty; //登入的名稱
        [ObservableProperty] private UserControl? _currentPageView;
        [ObservableProperty] private string currentPageKey = "";  // 存目前的頁面
        [ObservableProperty] private UserControl? storageControlPage ;

        [ObservableProperty] private bool _isIdle;
        [ObservableProperty] private bool _isHint = true;
        [ObservableProperty] private bool _isAlarm = false;
        [ObservableProperty] private Brush summaryMessageBrush = new SolidColorBrush(Colors.Black);
        [ObservableProperty] private string summaryMessage = "系統正常運作";
        public bool IsLoggedIn => !string.IsNullOrEmpty(LoggedInUser);
        [ObservableProperty] private bool isDispatch;

        //控制區按鈕
        public RobotStore RobotStore { get; }

        private List<string> RobotNames = new List<string>() ;
        //[ObservableProperty] private Robot _robot = new Robot();
        public Robot Robot => RobotStore.Robot;


        //開始
        [ObservableProperty] private bool startStatus;
        [ObservableProperty] private Brush startBackground = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
        [ObservableProperty] private Brush startForeground = new SolidColorBrush(Color.FromRgb(0x00, 0x4E, 0x79));
        //暫停
        [ObservableProperty] private bool pauseStatus;
        [ObservableProperty] private Brush pauseBackground = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
        [ObservableProperty] private Brush pauseForeground = new SolidColorBrush(Color.FromRgb(0x00, 0x4E, 0x79));
        //停止
        [ObservableProperty] private bool stopStatus;
        [ObservableProperty] private Brush stopBackground = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
        [ObservableProperty] private Brush stopForeground = new SolidColorBrush(Color.FromRgb(0x00, 0x4E, 0x79));
        //派工
        [ObservableProperty] private bool dispatchStatus;
        [ObservableProperty] private string dispatchText = "派工啟動";
        [ObservableProperty] private Brush dispatchBackground = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
        [ObservableProperty] private Brush dispatchForeground = new SolidColorBrush(Color.FromRgb(0x00, 0x4E, 0x79));

        //PLC
        private readonly IPlcService _PlcService;
        public PlcStore PlcStore { get; }
        public MagazinePara MagazinePara => PlcStore.MagazinePara;

        // ASRS 參數輪詢計時器
       // DispatcherTimer? asrsTimer;
        // ✅ 新增：關機儲存UI設定
        public void SaveCurrentStoragePageType()
        {
            /*
            INIFile ini = new INIFile(AppDomain.CurrentDomain.BaseDirectory + "\\Basesitting.ini");
            string pageType = (StorageControlPage is FMSFrontend.Views.StorageUnitMiniControlPage).ToString();
            ini.Write("Prarm", "IsStorageUnitControlMini", pageType);

            if (productionLines?.DataContext is FMSFrontend.ViewModels.ProductionLinesViewModel vm)
                pageType = (vm.CurrentStorageView is FMSFrontend.Controls.StorageOverviewControl).ToString();
            else
                pageType = false.ToString();
            ini.Write("Prarm", "IsStorageOverviewControl", pageType);
            */
        }

      
        public MainWindowViewModel(IHttpService httpService, 
            IRobotService robotService,
            IPlcService plcService , 
            IAlarmService alarmService,
            AlarmPageViewModel alarmVM, 
            RobotStore store, 
            PlcStore plcStore,
            AlarmStore alarmStore,
            GlobalProperties globalProperties)
        {
            _httpService = httpService;
            _robotService = robotService;
            _alarmService = alarmService;
            _PlcService = plcService;

            RobotStore = store;
            PlcStore = plcStore;
            AlarmStore = alarmStore;
            _globalProperties = globalProperties;

            // 使用 DispatcherTimer 在 UI Thread 週期性更新時間（比起背景執行緒直接更新屬性更安全且不會產生跨執行緒問題）
            var timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, (s, e) =>
            {
                if (AlarmGroup.IsAlarm)
                {
                    IsAlarm = true;
                    IsHint = false;
                    IsIdle = false;
                    SummaryMessageBrush = new SolidColorBrush(Colors.Red);

                    SummaryMessage = "系統有警報";
                }
                else 
                {
                    IsAlarm = false;
                    IsHint = true;
                    IsIdle = true;
                    SummaryMessageBrush = new SolidColorBrush(Colors.Black);
                    SummaryMessage = "系統正常運作";
                }

                CurrentDateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            }, Application.Current.Dispatcher);
            timer.Start();
            
            AlarmVM = alarmVM;

            //✅ 新增：電極倉門初始頁面 -> 延遲建立到 UI Thread 空閒時再建立，避免啟動卡住
            INIFile ini = new INIFile(AppDomain.CurrentDomain.BaseDirectory + "\\Basesitting.ini");
            bool b = ini.Read("Prarm", "IsStorageUnitControlMini") == "True";
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                StorageControlPage = b ? new StorageUnitMiniControlPage() : new StorageUnitControlPage();
            }), DispatcherPriority.Background);


            // 初始 & 監聽 Robot 變化
            RefreshFromStore();
            RobotStore.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(RobotStore.Robot))
                    RefreshFromStore();
            };
            // 如果你會改 Robot 內部屬性，也可加：
            RobotStore.Robot.PropertyChanged += (_, __) => RefreshFromStore();
           
            //✅ 新增：讀取初始參數,然後開啟輪詢
            // _ = MainWindowViewModelAsync_Init();
        }

        // === 將 Store 狀態轉成 UI ===
        private void RefreshFromStore()
        {
            var r = Robot;
            var start = r.AsrsState == AsrsControlState.Started;
            var pause = r.AsrsState == AsrsControlState.Paused;
            var stop = r.AsrsState == AsrsControlState.Stopped;

            // Start
            StartStatus = start;
            StartBackground = start ? Dark : Light;
            StartForeground = start ? Light : Dark;

            // Pause（修正原本的誤設）
            PauseStatus = pause;
            PauseBackground = pause ? Dark : Light;
            PauseForeground = pause ? Light : Dark;

            // Stop
            StopStatus = stop;
            StopBackground = stop ? Dark : Light;
            StopForeground = stop ? Light : Dark;

            // Dispatch
            DispatchStatus = r.DispatchEnabled;
            DispatchText = DispatchStatus ? "派工中" : "派工啟動";
            DispatchBackground = DispatchStatus ? Dark : Light;
            DispatchForeground = DispatchStatus ? Light : Dark;
        }
        #region PageChange

        [RelayCommand]
        private void GoToProductionLines() => NavigateTo<ProductionLines>("ProductionLines");

        [RelayCommand]
        private void GoToFactoryOverview() => NavigateTo<FactoryOverviewPage>("FactoryOverview");

        [RelayCommand]
        private void GoToMachineOverview() => NavigateTo<MachineOverviewPage>("MachineOverview");

        [RelayCommand]
        private void GoToWorkOrder() => NavigateTo<WorkOrder>("WorkOrder");

        [RelayCommand]
        private void GoToRFIDBind() => NavigateTo<RFIDBind>("RFIDBind");

        [RelayCommand]
        private void GoToOperationHistory() => NavigateTo<OperationHistory>("OperationHistory");

        [RelayCommand]
        private void GoToMaterial() => NavigateTo<InventoryInformationPage>("Material");

        [RelayCommand]
        private void GoToSettingsView() => NavigateTo<SettingsView>("SettingsView");
        [RelayCommand]
        private void OpenAlarm() => NavigateTo<AlarmPage>("Alarm");

        private void NavigateTo<TPage>(string pageKey) where TPage : UserControl
        {
            // 檢查 ServiceProvider 是否為 null
            if (App.ServiceProvider == null)
            {
                new DialogMessageWindow("ServiceProvider 尚未初始化，無法切換頁面。").ShowDialog();
                return;
            }
            var page = App.ServiceProvider.GetRequiredService<TPage>();
            CurrentPageView = page;
            CurrentPageKey = pageKey;
            //  SelectedPageIndex = -1;
        }

        #endregion

        #region StoragePageChange
        [RelayCommand]
        private void ShowDetail(string storageId)
        {
            var detailPage = new StorageUnitControlPage
            {
                DataContext = this // 👈 傳入目前的 MainWindowViewModel
            };
            // 等畫面載入完成後再捲動
            detailPage.Loaded += (s, e) =>
            {
                detailPage.ScrollToStorageId(storageId);
            };

            StorageControlPage = detailPage;
        }

        private bool[] isUpperDoorOpen = new bool[8];
        private bool[] isLowerDoorOpen = new bool[8];


        // 上門 -> 使用第 0 顆燈
        [RelayCommand]
        private async Task UpperDoor(string storageId)
        {
            try
            {
                int id = int.Parse(storageId);
                var success = await _PlcService.EleMagzineDoorSwitchAsync(id ,0,true);
                if (!success)
                {
                    new DialogMessageWindow("API 回傳失敗").ShowDialog();
                    return;
                }
            }
            catch
            {
                new DialogMessageWindow("Reset Fail").ShowDialog();
            }
        }

        // 下門 -> 使用 LowerDoorLights1 對應索引
        [RelayCommand]
        private async Task LowerDoor(string storageId)
        {
            try
            {
                int id = int.Parse(storageId);
                var success = await _PlcService.EleMagzineDoorSwitchAsync(id, 1, true);
                if (!success)
                {
                    new DialogMessageWindow("API 回傳失敗").ShowDialog();
                    return;
                }
            }
            catch
            {
                new DialogMessageWindow("Reset Fail").ShowDialog();
            }
        }
       
        [ObservableProperty]
        //private bool _isDoorLightOn;

        private bool _isDoorLightOn;
        private int Cnt = 0;
        // 新增命令：ToggleButton 切換時呼叫
        [RelayCommand]
        private async Task DoorLightSwitchChanged(bool isChecked)
        {
            try
            {
                Cnt++;
                var success = await _PlcService.EleMagzineDoorLightSwitchAsync(0, isChecked);
                if (!success)
                {
                    new DialogMessageWindow("API 回傳失敗").ShowDialog();
                    return;
                }
            }
            catch
            {
               new DialogMessageWindow("Reset Fail").ShowDialog();
            }
        }

        [RelayCommand]
        public void Back()
        {
            StorageControlPage = new StorageUnitMiniControlPage
            {
                DataContext = this // 保持 ViewModel 綁定，否則按鈕 Command 會失效
            };
        }
        #endregion

        #region PowerButton
        [RelayCommand]
        private void PowerButtonClick()
        {
            // MessageBox.Show("關機囉！");

            // 1. 建立 ViewModel 的實例
            var viewModel = new ShutdownWindowViewModel(false);

            // 2. 建立視窗的實例
            var dialog = new ShutdownWindow();

            // 3. 將 ViewModel 設定為視窗的 DataContext
            dialog.DataContext = viewModel;

            // 4. 顯示視窗
            dialog.ShowDialog();
        }
        #endregion

        #region Logout
        [RelayCommand]
        private void Logout()
        {
            LoggedInUser = string.Empty;
            var dialog = new DialogMessageWindow("您已成功登出！");
            dialog.ShowDialog();
        }
        #endregion

        #region Login
        public static bool LoginFlag = false;
        public static string UserName = "";
        [RelayCommand]
        private async Task Login()
        {
            try
            {
                IWorkerService workerService = new WorkerService(_httpService);
                List<WorkerDto> dtos =  await workerService.GetAllWorkerAsync()?? new();
                if (dtos.Count > 0)
                {
                    var LoginDatas = new List<LoginInfo>();
                    for (int i = 0; i < dtos.Count; i++)
                    {
                        LoginDatas.Add(new LoginInfo
                        {
                            Name = dtos[i].WorkerNumber,
                            Password = dtos[i].Password
                        });
                    }
                    var win = new LoginWindow(LoginDatas, UserName);
                    var result = win.ShowDialog();
                    if (result == true && !string.IsNullOrWhiteSpace(win.ViewModel.Name))
                    {
                        UserName = win.ViewModel.Name;
                        LoggedInUser = UserName; // 使用者輸入的名稱
                        LoginFlag = true;
                        var dialog = new DialogMessageWindow($"歡迎登入，{LoggedInUser}！").ShowDialog();
                    }
                }
                else
                {
                    var dialog = new DialogMessageWindow($"目前無員工資料").ShowDialog();
                }
            }
            catch { }
        }
        #endregion

        #region ControlUnit Start Pause Stop  Reset Dispatch
        [RelayCommand]
        private async Task RobotStartButtonClick()
        {
            if (!StartStatus)
            {
                try
                {
                    if (Robot.AsrsState == AsrsControlState.Started)
                        return;

                    try
                    {
                        var success = await _robotService.SetRobotStartAsync();
                        if (!success)
                        {
                            new DialogMessageWindow("API 回傳失敗").ShowDialog();
                            return;
                        }

                        // 這裡可以選擇樂觀更新，或等 Updater 自動刷新
                        Robot.AsrsState = AsrsControlState.Started;
                    }
                    catch (Exception ex)
                    {
                        new DialogMessageWindow($"Start Fail\n{ex.Message}").ShowDialog();
                    }
                }
                catch
                {
                    new DialogMessageWindow("Start Fail").ShowDialog();
                }
            }
        }
        [RelayCommand]
        private async Task RobotPauseButtonClick()
        {
            if (!PauseStatus)
            {
                try
                {
                    if (Robot.AsrsState == AsrsControlState.Paused)
                        return;

                    try
                    {
                        var success = await _robotService.SetRobotPauseAsync();
                        if (!success)
                        {
                            new DialogMessageWindow("API 回傳失敗").ShowDialog();
                            return;
                        }

                        // 這裡可以選擇樂觀更新，或等 Updater 自動刷新
                        Robot.AsrsState = AsrsControlState.Paused;
                    }
                    catch (Exception ex)
                    {
                        new DialogMessageWindow($"Pause Fail\n{ex.Message}").ShowDialog();
                    }
                }
                catch
                {
                    new DialogMessageWindow("Pause Fail").ShowDialog();
                }

            }
        }
        [RelayCommand]
        private async Task RobotStopButtonClick()
        {
            if (!StopStatus)
            {
                try
                {
                    if (Robot.AsrsState == AsrsControlState.Stopped)
                        return;

                    try
                    {
                        var success = await _robotService.SetRobotStopAsync();
                        if (!success)
                        {
                            new DialogMessageWindow("API 回傳失敗").ShowDialog();
                            return;
                        }

                        // 這裡可以選擇樂觀更新，或等 Updater 自動刷新
                        Robot.AsrsState = AsrsControlState.Stopped;
                    }
                    catch (Exception ex)
                    {
                        new DialogMessageWindow($"Stop Fail\n{ex.Message}").ShowDialog();
                    }
                }
                catch
                {
                    new DialogMessageWindow("Stop Fail").ShowDialog();
                }
            }
        }
        [RelayCommand]
        private async Task RobotResetButtonClick()
        {
            try
            {
                var success = await _robotService.ASRSRobotResetStatusAsync(0);
                if (!success)
                {
                    new DialogMessageWindow("API 回傳失敗").ShowDialog();
                    return;
                }
            }
            catch 
            {
                new DialogMessageWindow("Reset Fail").ShowDialog();
            }
        }
        [RelayCommand]
        private async Task RobotDispatchButtonClick()
        {
            var target = !DispatchStatus;
            try
            {
                var ok = await _robotService.SetASRSDispatchSwitchAsync(target);
                if (!ok)
                {
                    new DialogMessageWindow("Dispatch Fail").ShowDialog();
                    return;
                }

                // 成功更新畫面
               // DispatchStatus = target;
               // DispatchText = DispatchStatus ? "派工中" : "派工啟動";
               // DispatchBackground = DispatchStatus ? Dark : Light;
               // DispatchForeground = DispatchStatus ? Light : Dark;
            }
            catch
            {
                new DialogMessageWindow("Dispatch Fail").ShowDialog();
            }
        }
        #endregion

        #region RobotNavigation

        [RelayCommand]
        private void NextRobot()
        {
            if (Robot == null || RobotNames.Count<2) return;
            //待增加
        }
        [RelayCommand]
        private void LastRobot()
        {
            if (Robot == null || RobotNames.Count < 2) return;
            //待增加

        }
        #endregion
    }
}
