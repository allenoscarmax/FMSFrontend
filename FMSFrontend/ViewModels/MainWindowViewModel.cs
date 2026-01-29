using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Extensions;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Interfaces;
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
        private readonly IWorkerService _workerService;
        private readonly IWindowService _windowService;
        private readonly IAlarmService _alarmService;
        private readonly IPlcService _PlcService;
        private readonly IAuthorizationService _auth;


        public GlobalProperties _globalProperties { get; }
        public AlarmPageViewModel AlarmVM { get; }
        
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

        // 新增：控制按鈕是否可以按 (IsEnabled)
        [ObservableProperty] private bool isStartEnabled;
        [ObservableProperty] private bool isPauseEnabled;
        [ObservableProperty] private bool isStopEnabled;

        
        public PlcStore PlcStore { get; }
        public MagazinePara MagazinePara => PlcStore.MagazinePara;
            private readonly UserSession _userSession;
        public UserSession UserSession => _userSession;


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
            IWorkerService workerService,
            IWindowService windowService,
            IAuthorizationService auth,
            AlarmPageViewModel alarmVM, 
            RobotStore store, 
            PlcStore plcStore,
            AlarmStore alarmStore,
            GlobalProperties globalProperties,
            UserSession userSession)
        {
            _httpService = httpService;
            _robotService = robotService;
            _alarmService = alarmService;
            _PlcService = plcService;
            _workerService = workerService;
            _windowService = windowService;
            _auth = auth;

            RobotStore = store;
            PlcStore = plcStore;
            AlarmStore = alarmStore;
            _globalProperties = globalProperties;
            _userSession = userSession;

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
            // Pause
            PauseStatus = pause;
            PauseBackground = pause ? Dark : Light;
            PauseForeground = pause ? Light : Dark;
            // Stop
            StopStatus = stop;
            StopBackground = stop ? Dark : Light;
            StopForeground = stop ? Light : Dark;
            // --- 2. 新增：按鈕啟用邏輯 ---
            if (start)
            {
                // 當 AsrsControlState = Started：
                IsStartEnabled = true; // 已經啟動了，不用再按
                IsPauseEnabled = true;  // Pause 按鈕 enable
                IsStopEnabled = false;  // Stop 按鈕 disabled (需求指定)
            }
            else if (pause)
            {
                // 當 AsrsControlState = Paused：
                IsStartEnabled = true;  // Start 按鈕 enable (恢復執行)
                IsPauseEnabled = true; // 已經暫停了，不用再按
                IsStopEnabled = true;   // Stop 按鈕 enable
            }
            else // 假設其餘情況視為 Stopped
            {
                // 當 AsrsControlState = Stopped：
                IsStartEnabled = true;  // Start 按鈕 enable
                IsPauseEnabled = false; // Pause 按鈕 disable
                IsStopEnabled = true;  // 已經停止了，不用再按
            }

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
                if (!_auth.RequireLoginAndWriteOperation(8, "Id = " + storageId))
                    return;
                var success = await _PlcService.EleMagzineDoorSwitchAsync(0,0,true);
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
                if (!_auth.RequireLoginAndWriteOperation(9, "Id = " + storageId))
                    return;
                var success = await _PlcService.EleMagzineDoorSwitchAsync(0, 1, true);
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
            if (!_auth.RequireLoginAndWriteOperation(10, isChecked ? "On" : "Off"))
                return;
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
            _windowService.ShowShutdownWindow(isRobotRunning: false);
        }
        #endregion

        #region Logout
        [RelayCommand]
        private void Logout()
        {
            _auth.RequireLoginAndWriteOperation(2);
            _userSession.SignOut();
            _windowService.ShowMessage("您已成功登出！");
        }
        #endregion

        #region Login
      
        [RelayCommand]
        private async Task Login()
        {
            if(!_globalProperties.IsServerAlive)
            {
                _windowService.ShowMessage("伺服器斷線，請確認");
                return;
            }
            try
            {
                var dtos = await _workerService.GetAllWorkerAsync() ?? new();

                if (dtos.Count == 0)
                {
                    _windowService.ShowMessage("目前無員工資料");
                    return;
                }

                var loginDatas = dtos.Select(dto => new LoginInfo
                {
                    Name = dto.WorkerName,
                    Password = dto.Password
                }).ToList();

                // 傳入目前登入者作為預設值
                var loginName = _windowService.ShowLoginWindow(
                    loginDatas,
                    _userSession.UserName);

                if (!string.IsNullOrWhiteSpace(loginName))
                {
                    _userSession.SignIn(loginName);
                    _windowService.ShowMessage($"歡迎登入，{loginName}！");
                }
                _auth.RequireLoginAndWriteOperation(1);
            }
            catch
            {
                _windowService.ShowMessage("登入過程發生錯誤");
            }
        }

        #endregion

        #region ControlUnit Start Pause Stop  Reset Dispatch
        [RelayCommand]
        private async Task RobotStartButtonClick()
        {
            if (!_auth.RequireLogin())
                return;

            if (!Robot.IsRobotConnected)
            {
                 _windowService.ShowMessage("機器人未連線，無法啟動");
                return;
            }
            if (!StartStatus)
            {
                if (Robot.AsrsState == AsrsControlState.Started)
                    return;

                try
                {
                    if (!_auth.RequireLoginAndWriteOperation(3))
                        return;
                    var success = await _robotService.SetRobotStartAsync();
                    if (!success)
                    {
                        new DialogMessageWindow("忙碌中").ShowDialog();
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
        }
        [RelayCommand]
        private async Task RobotPauseButtonClick()
        {
            if (!_auth.RequireLogin())
                return;
            if (!Robot.IsRobotConnected)
            {
                _windowService.ShowMessage("機器人未連線，無法執行");
                return;
            }
            if (!PauseStatus)
            {
                if (Robot.AsrsState == AsrsControlState.Paused)
                    return;
                if (!_auth.RequireLoginAndWriteOperation(4))
                    return;
                try
                {
                    var success = await _robotService.SetRobotPauseAsync();
                    if (!success)
                    {
                        new DialogMessageWindow("忙碌中").ShowDialog();
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
        }
        [RelayCommand]
        private async Task RobotStopButtonClick()
        {

            if (!Robot.IsRobotConnected)
            {
                _windowService.ShowMessage("機器人未連線，無法執行");
                return;
            }
            if (!StopStatus)
            {
                if (Robot.AsrsState == AsrsControlState.Stopped)
                    return;
                try
                {
                    if (!_auth.RequireLoginAndWriteOperation(5))
                        return;
                    var success = await _robotService.SetRobotStopAsync();
                    if (!success)
                    {
                        new DialogMessageWindow("忙碌中").ShowDialog();
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
        }
        [RelayCommand]
        private async Task RobotResetButtonClick()
        {
            if (!_auth.RequireLogin())
                return;
            if (!Robot.IsRobotConnected)
            {
                _windowService.ShowMessage("機器人未連線，無法執行");
                return;
            }
            try
            {
                if (!_auth.RequireLoginAndWriteOperation(6))
                    return;
                var success = await _robotService.ASRSRobotResetStatusAsync(0);
                if (!success)
                {
                    new DialogMessageWindow("忙碌中").ShowDialog();
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
            if (!_auth.RequireLogin())
                return;
            var target = !DispatchStatus;
            if (!_auth.RequireLoginAndWriteOperation(7, target ? " :On" : " :Off"))
                return;
            try
            {
                var ok = await _robotService.SetASRSDispatchSwitchAsync(target);
                if (!ok)
                {
                    new DialogMessageWindow("Dispatch Fail").ShowDialog();
                    return;
                }
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
