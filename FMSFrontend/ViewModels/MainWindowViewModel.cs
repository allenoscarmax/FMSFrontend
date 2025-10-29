using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
//using OSCARMAXFMS_V3.DBmodels;
using CommunityToolkit.Mvvm.Messaging; // ← 新增
using CommunityToolkit.Mvvm.Messaging.Messages; // ← 新增：Message 型別
using ControlzEx.Standard;
using FMSFrontend.Controls;
using FMSFrontend.Extensions;
using FMSFrontend.Helpers;
using FMSFrontend.Models;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views;
using IniFile;
using OSCARMAXFMS_V3.DBmodels;
using OSCARMAXFMS_V3.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json; // ← 新增：JsonElement
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls; // 放在你的 ViewModel 上方
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace FMSFrontend.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        // 開啟頁面視窗（改為延遲建立：避免啟動時一次建立大量 UI）
        private ProductionLines? productionLines;
        private FactoryOverviewPage? factoryOverviewPage;
        private MachineOverviewPage? machineOverviewPage;
        private WorkOrder? workOrder;
        private RFIDBind? rFIDBind;
        private OperationHistory? operationHistory;
        private InventoryInformationPage? inventoryInformationPage;
        private SettingsView? settingsView;
        private readonly IHttpService _httpService;
        public AlarmPageViewModel AlarmVM { get; }
        [ObservableProperty] private bool _isMenuVisible;
        [ObservableProperty] private string currentDateTime = "";  //存現在的時間
        [ObservableProperty] private string _loggedInUser = string.Empty; //登入的名稱
        [ObservableProperty] private UserControl? _currentPageView;
        [ObservableProperty] private string currentPageKey = "";  // 存目前的頁面
        [ObservableProperty] private UserControl storageControlPage;
        

        [ObservableProperty] private bool _isIdle;
        [ObservableProperty] private bool _isHint = true;
        [ObservableProperty] private bool _isAlarm = false;
        [ObservableProperty] private string _summaryMessage = "系統正常運作";
        public bool IsLoggedIn => !string.IsNullOrEmpty(LoggedInUser);
        [ObservableProperty] private bool isDispatch;

        //控制區按鈕
        private bool MuRobot = false;
        private List<string> RobotNames = new List<string>() ;
        [ObservableProperty] private Robot _robot = new Robot();
        //開始
        [ObservableProperty] private bool startStatus;
        [ObservableProperty] private Brush startBackground = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
        [ObservableProperty] private Brush startForeground = new SolidColorBrush(Color.FromRgb(0x00, 0x4E, 0x79));
        //暫停
        [ObservableProperty] private bool pauseStatus;
        [ObservableProperty]private Brush pauseBackground = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
        [ObservableProperty]private Brush pauseForeground = new SolidColorBrush(Color.FromRgb(0x00, 0x4E, 0x79));
        //停止
        [ObservableProperty] private bool stopStatus;
        [ObservableProperty] private Brush stopBackground = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
        [ObservableProperty] private Brush stopForeground = new SolidColorBrush(Color.FromRgb(0x00, 0x4E, 0x79));
        //派工
        [ObservableProperty] private bool dispatchStatus;
        [ObservableProperty] private string dispatchText = "派工啟動";
        [ObservableProperty] private Brush dispatchBackground = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
        [ObservableProperty] private Brush dispatchForeground = new SolidColorBrush(Color.FromRgb(0x00, 0x4E, 0x79));

        //電極門 與 工件門
        [ObservableProperty] private Brush _leftTitleBrush = new SolidColorBrush(Color.FromRgb(0x27, 0x79, 0xA7));
        [ObservableProperty] private string _leftTitle = "電極";
        [ObservableProperty] private Brush _rightTitleBrush = new SolidColorBrush(Color.FromRgb(0xE0, 0x8E, 0x45));
        [ObservableProperty] private string _rightTitle = "工件";
        [ObservableProperty] public ObservableCollection<Brush> _upperDoorLights1 = new ObservableCollection<Brush>(Enumerable.Repeat(Brushes.Gray, 1));
        [ObservableProperty] public ObservableCollection<Brush> _upperDoorLights2 = new ObservableCollection<Brush>(Enumerable.Repeat(Brushes.Gray, 1));
        [ObservableProperty] public ObservableCollection<Brush> _lowerDoorLights1 = new ObservableCollection<Brush>(Enumerable.Repeat(Brushes.Gray, 1));
        [ObservableProperty] public ObservableCollection<Brush> _lowerDoorLights2 = new ObservableCollection<Brush>(Enumerable.Repeat(Brushes.Gray, 1));
        // ASRS 參數輪詢計時器
        DispatcherTimer asrsTimer;
        // ✅ 新增：關機儲存UI設定
        public void SaveCurrentStoragePageType()
        {
            INIFile ini = new INIFile(AppDomain.CurrentDomain.BaseDirectory + "\\Basesitting.ini");
            string pageType = (StorageControlPage is FMSFrontend.Views.StorageUnitMiniControlPage).ToString();
            ini.Write("Prarm", "IsStorageUnitControlMini", pageType);

            if (productionLines?.DataContext is FMSFrontend.ViewModels.ProductionLinesViewModel vm)
                pageType = (vm.CurrentStorageView is FMSFrontend.Controls.StorageOverviewControl).ToString();
            else
                pageType = false.ToString();
            ini.Write("Prarm", "IsStorageOverviewControl", pageType);
        }

      
        public MainWindowViewModel(IHttpService httpService, AlarmPageViewModel alarmVM)
        {
            _httpService = httpService;
            
            // 使用 DispatcherTimer 在 UI Thread 週期性更新時間（比起背景執行緒直接更新屬性更安全且不會產生跨執行緒問題）
            var timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, (s, e) =>
            {
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
            
            //✅ 新增：讀取初始參數,然後開啟輪詢
            _ = MainWindowViewModelAsync_Init();
        }

        //讀取初始參數
        private async Task MainWindowViewModelAsync_Init()
        {
            try  //取得Robot資料
            {
                JsonElement? json = await _httpService.GetJsonAsync<JsonElement>("Robot/DB_GetAllRobots", default);
                List<Robots> list = (json.HasValue && json.Value.ValueKind != JsonValueKind.Undefined) ?
                     JsonSerializer.Deserialize<List<Robots>>(json.Value.GetRawText()) ?? new List<Robots>() :
                     new List<Robots>();
                RobotNames = new List<string>();
                RobotNames.Clear();
                foreach (var r in list)
                {
                    RobotNames.Add(r.robotName);

                }
            }
            catch { }
            _ = FetchASRSParameterAsync();
            
            // Start ASRS parameter polling timer
            asrsTimer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, async (s, e) =>
            {
                await FetchASRSParameterAsync();
                await FetchDoorAsync();
            }, Application.Current.Dispatcher);
            asrsTimer.Start();
        }
        //輪尋讀取ASRS參數
        private async Task FetchASRSParameterAsync()
        {
            try
            {
                JsonElement? json = await _httpService.GetJsonAsync<JsonElement>("ASRS/GetASRSParameter", default);
                ASRSParameter a = (json.HasValue && json.Value.ValueKind != JsonValueKind.Undefined) ?
                      JsonSerializer.Deserialize<ASRSParameter>(json.Value.GetRawText()) ?? new ASRSParameter() :
                      new ASRSParameter();
                if (a != null)
                {
                    //更新機器人狀態
                    if (Robot == null ) Robot = new Robot();
                    Robot.Name = RobotNames[0];
                    Robot.IsRobotConnected = a.isRobotConnected;
                    Robot.CurrentLocation = a.robotPosition ?? "未知";
                    Robot.CurrentAction = a.robotDoingNow ?? "未知";
                    Robot.NextAction = a.robotDoingNext ?? "未知";
                    if(RobotNames.Count >1)
                        Robot.SelectedRobotIndexDisplay = a.robotNumber.ToString() + " / " + RobotNames.Count.ToString();
                    else
                        Robot.SelectedRobotIndexDisplay = "1 / 1";

                    //更新按鈕狀態
                    var dark = new SolidColorBrush(Color.FromRgb(0x00, 0x4E, 0x79));
                    var light = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
                    // Use generated properties so PropertyChanged is raised
                    StartStatus = a.asrsControlStart;
                    PauseBackground = a.asrsControlPause ? dark : light;
                    PauseForeground = a.asrsControlPause ? light : dark;

                    PauseStatus = a.asrsControlStart;
                    StartBackground = a.asrsControlStart ? dark : light;
                    StartForeground = a.asrsControlStart ? light : dark;

                    StopStatus = a.asrsControlStop;
                    StopBackground = a.asrsControlStop ? dark : light;
                    StopForeground = a.asrsControlStop ? light : dark;

                    DispatchStatus = a.dispatchSwitch;
                    DispatchText = DispatchStatus ? "派工中" : "派工啟動";
                    DispatchBackground = a.dispatchSwitch ? dark : light;
                    DispatchForeground = a.dispatchSwitch ? light : dark;
                }
            }
            catch
            {
                // ignore transient errors
            }
        }
        private async Task FetchDoorAsync()
        {
            try
            {
                JsonElement? json = await _httpService.GetJsonAsync<JsonElement>("PLC/GetALLMagazinePara", default);
                MagazinePara p = (json.HasValue && json.Value.ValueKind != JsonValueKind.Undefined) ?
                      JsonSerializer.Deserialize<MagazinePara>(json.Value.GetRawText()) ?? new MagazinePara() :
                      new MagazinePara();
                if (p != null)
                {
                    UpperDoorLights1[0] = p.ShouldScanEle ? Brushes.Lime : Brushes.Gray;
                    UpperDoorLights2[0] = p.EleMagzineDoorOpen[0] ? Brushes.Lime : Brushes.Gray;
                    LowerDoorLights1[0] = p.ShouldScanPart ? Brushes.Lime : Brushes.Gray;
                    LowerDoorLights2[0] = p.PartMagzineDoorOpen[0] ? Brushes.Lime : Brushes.Gray;
                }
            }
            catch
            {
                // ignore transient errors
            }
        }
        #region PageChange

        [RelayCommand]
        private void GoToProductionLines()
        {
            if (productionLines == null) productionLines = new ProductionLines();
            CurrentPageView = productionLines;
            CurrentPageKey = "ProductionLines";
            WeakReferenceMessenger.Default.Send(new ValueChangedMessage<string>(CurrentPageKey));
        }

        [RelayCommand]
        private void GoToFactoryOverview()
        {
            if (factoryOverviewPage == null) factoryOverviewPage = new FactoryOverviewPage();
            CurrentPageView = factoryOverviewPage;
            CurrentPageKey = "FactoryOverview";
        }
        [RelayCommand]
        private void GoToMachineOverview()
        {
            if (machineOverviewPage == null) machineOverviewPage = new MachineOverviewPage();
            CurrentPageView = machineOverviewPage;
            CurrentPageKey = "MachineOverview";
        }

        [RelayCommand]
        private void GoToWorkOrder()
        {
            if (workOrder == null) workOrder = new WorkOrder();
            CurrentPageView = workOrder;
            CurrentPageKey = "WorkOrder";
            WeakReferenceMessenger.Default.Send(new ValueChangedMessage<string>(CurrentPageKey));
        }
        [RelayCommand]
        private void GoToRFIDBind()
        {
            if (rFIDBind == null) rFIDBind = new RFIDBind();
            CurrentPageView = rFIDBind;
            CurrentPageKey = "RFIDBind";
            WeakReferenceMessenger.Default.Send(new ValueChangedMessage<string>(CurrentPageKey));
        }
        [RelayCommand]
        private void GoToOperationHistory()
        {
            if (operationHistory == null) operationHistory = new OperationHistory();
            CurrentPageView = operationHistory;
            CurrentPageKey = "OperationHistory";
        }
        [RelayCommand]
        private void GoToMaterial()
        {
            if (inventoryInformationPage == null) inventoryInformationPage = new InventoryInformationPage();
            CurrentPageView = inventoryInformationPage;
            CurrentPageKey = "Material";
        }
        [RelayCommand]
        private void GoToSettingsView()
        {
            if (settingsView == null) settingsView = new SettingsView();
            CurrentPageView = settingsView;
            CurrentPageKey = "SettingsView";
        }
        /// <summary>
        /// 由狀態列「提示訊息」進入 Alarm 頁，並讓 PageMenu 看起來沒有選中
        /// </summary>
        [RelayCommand]
        private void OpenAlarm()
        {
            // 延遲建立 AlarmPage（避免一次建立過多 UI）
            var alarm = new AlarmPage();
            CurrentPageView = alarm; // 你的 Alarm UserControl / Page
                                               // 讓下方 PageMenu 不顯示選中狀態
            CurrentPageKey = "";             // 或 string.Empty 都可
                                               // 若你的 PageMenu 是用 SelectedIndex 套樣式，這行也一起用：
                                               // SelectedPageIndex = -1;
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
            if (!int.TryParse(storageId, out int n)) return;
            int idx = n - 1;
            if (idx is < 0 or > 7) return;
            isUpperDoorOpen[idx] = !isUpperDoorOpen[idx];
            try 
            {
                var route = $"PLC/ELEMagzineDoorSwitch/0/0/{isUpperDoorOpen[idx].ToString().ToLower()}";
                await _httpService.SendPutAsync(route, new { });
            }
            catch { }
        }

        // 下門 -> 使用 LowerDoorLights1 對應索引
        [RelayCommand]
        private async Task LowerDoor(string storageId)
        {
            if (!int.TryParse(storageId, out int n)) return;
            int idx = n - 1;
            if (idx is < 0 or > 7) return;
            isLowerDoorOpen[idx] = !isLowerDoorOpen[idx];

            try
            {
                var route = $"PLC/ELEMagzineDoorSwitch/0/1/{isUpperDoorOpen[idx].ToString().ToLower()}";
                await _httpService.SendPutAsync(route, new { });
            }
            catch { }
        }
       
        [ObservableProperty]
        private bool _isDoorLightOn;

        // 新增命令：ToggleButton 切換時呼叫
        [RelayCommand]
        private async Task DoorLightSwitchChanged(bool isChecked)
        {
            // 0: 關閉, 1: 開啟
            int lightSwitch = isChecked ? 1 : 0;
            try
            {
                var route = $"PLC/EleMagazineDoorLightSwitch/0/{lightSwitch}";
                await _httpService.SendPutAsync(route, new { });
            }
            catch { }
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
        [RelayCommand]
        private void Login()
        {
            LoggedInUser = "王小明";
            var dialog = new DialogMessageWindow($"歡迎登入，{LoggedInUser}！");
            dialog.ShowDialog();
        }
        #endregion

        #region ControlUnit Start Pause Stop  Reset Dispatch
        [RelayCommand]
        private async Task RobotStartButton()
        {
            if (!StartStatus)
            {
                try
                {
                    const string route = "ASRS/SetASRSRobotStart";
                    await _httpService.SendPutAsync(route, new { });
                    await Task.Delay(300);
                    var op = Application.Current.Dispatcher.InvokeAsync(async () => await FetchASRSParameterAsync());
                    await op.Task;
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
                // 非同步等待 1 秒，避免阻塞 UI 執行緒，然後在 UI 執行緒上更新按鈕顏色
                try
                {
                    const string route = "ASRS/SetASRSRobotPause";
                    await _httpService.SendPutAsync(route, new { });
                    await Task.Delay(100);
                    var op = Application.Current.Dispatcher.InvokeAsync(async () => await FetchASRSParameterAsync());
                    await op.Task;
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
                    string route = "ASRS/SetASRSRobotStop";
                    await _httpService.SendPutAsync(route, new { });
                    await Task.Delay(300);
                    var op = Application.Current.Dispatcher.InvokeAsync(async () => await FetchASRSParameterAsync());
                    await op.Task;
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
                var route = "Robot/ASRSRobotResetStatus/0";
                await _httpService.SendPutAsync(route, new { });
            }
            catch 
            {
                new DialogMessageWindow("Reset Fail").ShowDialog();
            }
        }
        [RelayCommand]
        private async Task RobotDispatchButtonClick()
        {
            DispatchStatus = !DispatchStatus;
            DispatchText = DispatchStatus ? "派工中" : "派工啟動";
            try
            {
                var route = $"ASRS/SetASRSDispatchSwitch/{DispatchStatus.ToString().ToLower()}";
                await _httpService.SendPutAsync(route, new { });
                await Task.Delay(300);
                var op = Application.Current.Dispatcher.InvokeAsync(async () => await FetchASRSParameterAsync());
                await op.Task;
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
