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
        //開啟頁面視窗
        ProductionLines productionLines = new ProductionLines();
        FactoryOverviewPage factoryOverviewPage = new FactoryOverviewPage();
        MachineOverviewPage machineOverviewPage = new MachineOverviewPage();
        WorkOrder workOrder = new WorkOrder();
        RFIDBind rFIDBind = new RFIDBind();
        OperationHistory operationHistory = new OperationHistory();
        InventoryInformationPage inventoryInformationPage = new InventoryInformationPage();
        SettingsView settingsView = new SettingsView();
        
        private readonly IHttpService _httpService;

        public AlarmPageViewModel AlarmVM { get; }

        [ObservableProperty]
        private bool _isMenuVisible;
        [ObservableProperty]
        private string currentDateTime;  //存現在的時間
        [ObservableProperty]
        private string _loggedInUser = string.Empty; //登入的名稱
        [ObservableProperty]
        private UserControl currentPageView;
        [ObservableProperty]
        private string currentPageKey;  // 存目前的頁面
        [ObservableProperty]
        private UserControl storageControlPage;

        [ObservableProperty]
        private Robot _robot;

        [ObservableProperty]
        private bool _isIdle;

        [ObservableProperty]
        private bool _isHint = true;

        [ObservableProperty]
        private bool _isAlarm = false;

        [ObservableProperty]
        private string _summaryMessage = "系統正常運作";
        public bool IsLoggedIn => !string.IsNullOrEmpty(LoggedInUser);

        //控制區按鈕
        [ObservableProperty]
        private string _sDispatchText = "派工啟動";

        [ObservableProperty]
        private bool _isDispatch;

        //電極門 與 工件門

        [ObservableProperty]
        private Brush _leftTitleBrush = new SolidColorBrush(Color.FromRgb(0x27, 0x79, 0xA7));

        [ObservableProperty]
        private string _leftTitle = "電極";

        [ObservableProperty]
        private Brush _rightTitleBrush = new SolidColorBrush(Color.FromRgb(0xE0, 0x8E, 0x45));

        [ObservableProperty]
        private string _rightTitle = "工件";
        [ObservableProperty]
        public ObservableCollection<Brush> _upperDoorLights1 = new ObservableCollection<Brush>(Enumerable.Repeat(Brushes.Gray, 1));

        [ObservableProperty]
        public ObservableCollection<Brush> _upperDoorLights2 = new ObservableCollection<Brush>(Enumerable.Repeat(Brushes.Gray, 1));

        [ObservableProperty]
        public ObservableCollection<Brush> _lowerDoorLights1 = new ObservableCollection<Brush>(Enumerable.Repeat(Brushes.Gray, 1));

        [ObservableProperty]
        public ObservableCollection<Brush> _lowerDoorLights2 = new ObservableCollection<Brush>(Enumerable.Repeat(Brushes.Gray, 1));

        // ✅ 新增：頁面刷新計時器（與 _asrsTimer 分開）
        private readonly DispatcherTimer _pageRefreshTimer = new() { Interval = TimeSpan.FromSeconds(5) };

        // ✅ 新增：關機儲存UI設定
        public void SaveCurrentStoragePageType() 
        {          
            INIFile ini = new INIFile(AppDomain.CurrentDomain.BaseDirectory + "\\Basesitting.ini");
            string pageType = (StorageControlPage is FMSFrontend.Views.StorageUnitMiniControlPage).ToString();
            ini.Write("Prarm", "IsStorageUnitControlMini", pageType);

            if (productionLines.DataContext is FMSFrontend.ViewModels.ProductionLinesViewModel vm)     
                pageType = (vm.CurrentStorageView is FMSFrontend.Controls.StorageOverviewControl).ToString();
            else  
                pageType = false.ToString();
            ini.Write("Prarm", "IsStorageOverviewControl", pageType);
        }
        public MainWindowViewModel(IHttpService httpService, AlarmPageViewModel alarmVM)
        {
            _httpService = httpService;
            // 初始化時間更新
            Task.Run(async () =>
            {
                while (true)
                {
                    CurrentDateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    await Task.Delay(1000);
                }
            });
            Robot = new Robot()
            {
                Name = "機器人",
                CurrentLocation = "EDM",
                CurrentAction = "搬運",
                NextAction = "上架",
                SelectedRobotIndexDisplay = _robotActionIndex.ToString() + " / "+ _robotActionNum.ToString(),
                IsMultipleRobotVisible = true
            };
            AlarmVM = alarmVM;

            //✅ 新增：電極倉門初始頁面
            INIFile ini = new INIFile(AppDomain.CurrentDomain.BaseDirectory + "\\Basesitting.ini");
            bool b = ini.Read("Prarm", "IsStorageUnitControlMini") == "True";
            StorageControlPage = b ? new StorageUnitMiniControlPage() : new StorageUnitControlPage();
           
            
            // ✅ 新增：啟動背景執行續，並行呼叫五個 API
            Task.Run(InitializeDataAsync);

            // ✅ 新增：頁面刷新計時器
            _pageRefreshTimer.Tick += (_, __) => RefreshActivePage();
            _pageRefreshTimer.Start();
        }
        #region PageChange
        // ✅ 新增：依目前頁面廣播刷新訊息
        private void RefreshActivePage()
        {
            if (string.IsNullOrEmpty(CurrentPageKey)) return;

            switch (CurrentPageKey)
            {
                case "FactoryOverview":
                case "ProductionLines":
                case "MachineOverview":
                case "WorkOrder":
                case "RFIDBind":
                case "OperationHistory":
                case "Material":
                case "SettingsView":
                    WeakReferenceMessenger.Default.Send(new RefreshPageMessage(CurrentPageKey));
                    break;
            }
        }

        // ✅ 依頁面調整刷新頻率（補上其他頁面）
        private void SetPageRefreshIntervalFor(string pageKey)
        {
            _pageRefreshTimer.Interval = pageKey switch
            {
                "ProductionLines" => TimeSpan.FromSeconds(3),
                "MachineOverview" => TimeSpan.FromSeconds(2),
                "FactoryOverview" => TimeSpan.FromSeconds(5),
                "WorkOrder" => TimeSpan.FromSeconds(8),
                "RFIDBind" => TimeSpan.FromSeconds(6),
                "OperationHistory" => TimeSpan.FromSeconds(7),
                "Material" => TimeSpan.FromSeconds(10),
                "SettingsView" => TimeSpan.FromSeconds(12),
                _ => TimeSpan.FromSeconds(5)
            };
        }

        // ✅ 修改：切頁時同步調整頻率，並立即刷新一次
        [RelayCommand]
        private void GoToProductionLines()
        {
            CurrentPageView = productionLines;
            CurrentPageKey = "ProductionLines";
            SetPageRefreshIntervalFor(CurrentPageKey);
            RefreshActivePage();
        }

        [RelayCommand]
        private void GoToFactoryOverview()
        {
            CurrentPageView = factoryOverviewPage;
            CurrentPageKey = "FactoryOverview";
            SetPageRefreshIntervalFor(CurrentPageKey);
            RefreshActivePage();
        }
        [RelayCommand]
        private void GoToMachineOverview()
        {
            CurrentPageView = machineOverviewPage;
            CurrentPageKey = "MachineOverview";
            SetPageRefreshIntervalFor(CurrentPageKey);
            RefreshActivePage();
        }

        [RelayCommand]
        private void GoToWorkOrder()
        {
            CurrentPageView = workOrder;
            CurrentPageKey = "WorkOrder";
            SetPageRefreshIntervalFor(CurrentPageKey);
            RefreshActivePage();
        }
        [RelayCommand]
        private void GoToRFIDBind()
        {
            CurrentPageView = rFIDBind;
            CurrentPageKey = "RFIDBind";
            SetPageRefreshIntervalFor(CurrentPageKey);
            RefreshActivePage();
        }
        [RelayCommand]
        private void GoToOperationHistory()
        {
            CurrentPageView = operationHistory;
            CurrentPageKey = "OperationHistory";
            SetPageRefreshIntervalFor(CurrentPageKey);
            RefreshActivePage();
        }
        [RelayCommand]
        private void GoToMaterial()
        {
            CurrentPageView = inventoryInformationPage;
            CurrentPageKey = "Material";
            SetPageRefreshIntervalFor(CurrentPageKey);
            RefreshActivePage();
        }
        [RelayCommand]
        private void GoToSettingsView()
        {
            CurrentPageView = settingsView;
            CurrentPageKey = "SettingsView";
            SetPageRefreshIntervalFor(CurrentPageKey);
            RefreshActivePage();
        }
        /// <summary>
        /// 由狀態列「提示訊息」進入 Alarm 頁，並讓 PageMenu 看起來沒有選中
        /// </summary>
        [RelayCommand]
        private void OpenAlarm()
        {
            CurrentPageView = new AlarmPage(); // 你的 Alarm UserControl / Page
                                               // 讓下方 PageMenu 不顯示選中狀態
            CurrentPageKey = null;             // 或 string.Empty 都可
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
        private void UpperDoor(string storageId)
        {
            if (!int.TryParse(storageId, out int n)) return;
            int idx = n - 1;
            if (idx is < 0 or > 7) return;

            isUpperDoorOpen[idx] = !isUpperDoorOpen[idx];

            UpperDoorLights1[idx] = isUpperDoorOpen[idx] ? Brushes.Lime : Brushes.Gray;

            // new DialogMessageWindow(isUpperDoorOpen[idx] ? $"{storageId} 的上門已開啟" : $"{storageId} 的上門已關閉").ShowDialog();
        }

        // 下門 -> 使用 LowerDoorLights1 對應索引
        [RelayCommand]
        private void LowerDoor(string storageId)
        {
            if (!int.TryParse(storageId, out int n)) return;
            int idx = n - 1;
            if (idx is < 0 or > 7) return;

            isLowerDoorOpen[idx] = !isLowerDoorOpen[idx];

            LowerDoorLights1[idx] = isLowerDoorOpen[idx] ? Brushes.Lime : Brushes.Gray;
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


            /*
            var dialog = new DialogYesNoWindow("是否要更換主題！");
            dialog.ShowDialog();
            if (dialog.DialogResult == true)
            {
                // 切換風格的測試邏輯
                string current = ThemeManager.CurrentThemeName;
                string nextTheme = current == "Dark" ? "Light" : "Dark";
                ThemeManager.ApplyTheme(nextTheme);
            }*/
            //System.Windows.Application.Current.Shutdown();
        }
        #endregion

        #region Logout
        [RelayCommand]
        private void Logout()
        {
            // 這裡可以補上實際的登出處理，例如呼叫 API 或清除 token
            LoggedInUser = string.Empty; // 清除登入者資訊
            var dialog = new DialogMessageWindow("您已成功登出！");
            dialog.ShowDialog();
        }
        #endregion

        #region Login
        [RelayCommand]
        private void Login()
        {
            // TODO: 改為呼叫後台 API 取得使用者資訊
            LoggedInUser = "王小明"; // 登入成功後設定使用者名稱
            var dialog = new DialogMessageWindow($"歡迎登入，{LoggedInUser}！");
            dialog.ShowDialog();
        }
        #endregion

        #region ControlUnit
        [RelayCommand]
        private async Task RobotStartButton()
        {
            const string route = "http://localhost:5032/ASRS/SetASRSRobotStart";

            var dialog = new DialogMessageWindow("Start");
            dialog.ShowDialog();
            try
            {
               // await _httpService.SendPutAsync(route, new { });
            }
            catch { }
        }
        [RelayCommand]
        private async Task RobotPauseButtonClickCommand()
        {
            const string route = "http://localhost:5032/ASRS/SetASRSRobotPause";

            var dialog = new DialogMessageWindow("Pause");
            dialog.ShowDialog();
            try
            {
              //  await _httpService.SendPutAsync(route, new { });
            }
            catch { }
        }
        [RelayCommand]
        private async Task RobotStopButtonClick()
        {
            const string route = "http://localhost:5032/ASRS/SetASRSRobotStop";
            var dialog = new DialogMessageWindow("Stop");
            dialog.ShowDialog();
            try
            {
              //  await _httpService.SendPutAsync(route, new { });
            }
            catch { }
        }
        [RelayCommand]
        private async Task RobotResetButtonClick()
        {
            const string route = "http://localhost:5032/ASRS/SetASRSRobotReset";
            var dialog = new DialogMessageWindow("Reset");
            dialog.ShowDialog();
            try
            {
              //  await _httpService.SendPutAsync(route, new { });
            }
            catch { }
        }
        [RelayCommand]
        private async Task RobotDispatchButtonClick()
        {
            IsDispatch = !IsDispatch;
            SDispatchText = IsDispatch ? "派工中" : "派工啟動";

            const string route = "http://localhost:5032/ASRS/SetASRSRobotDispatch";
            var dialog = new DialogMessageWindow("Dispatch");
            dialog.ShowDialog();
            try
            {
              //  await _httpService.SendPutAsync(route, new { });
            }
            catch { }
        }
        #endregion

        #region Robot
        // 加入：目前指向 NextAction 的索引（-1 代表尚未初始化）
        private int _robotActionIndex = 1;
        private int _robotActionNum = 1;

        [RelayCommand]
        private void NextRobot()
        {
            if (Robot == null) return;
            /*
            if (_robotActionIndex == 1) _robotActionIndex = 1;
            else _robotActionIndex++;
            */
            Robot.Name = "名稱" + _robotActionIndex.ToString();
            Robot.CurrentLocation = "目前位置" + _robotActionIndex.ToString();
            Robot.CurrentAction = "目前動作 " + _robotActionIndex.ToString();
            Robot.NextAction = "下個動作 " + _robotActionIndex.ToString();
            Robot.SelectedRobotIndexDisplay = _robotActionIndex.ToString() + " / " + _robotActionNum.ToString();
            Robot.IsMultipleRobotVisible = false;
        }
        [RelayCommand]
        private void LastRobot()
        {
            if (Robot == null) return;
            if (_robotActionIndex == 1) _robotActionIndex = 3;
            else _robotActionIndex--;
            Robot.Name = "名稱" + _robotActionIndex.ToString();
            Robot.CurrentLocation = "目前位置" + _robotActionIndex.ToString();
            Robot.CurrentAction = "目前動作 " + _robotActionIndex.ToString();
            Robot.NextAction = "下個動作 " + _robotActionIndex.ToString();
            Robot.SelectedRobotIndexDisplay = _robotActionIndex.ToString() + " / " + _robotActionNum.ToString();
            Robot.IsMultipleRobotVisible = true;
        }
        #endregion

        #region DoorLight
        // 新增屬性：綁定 ToggleButton 狀態
        [ObservableProperty]
        private bool _isDoorLightOn;

        // 新增命令：ToggleButton 切換時呼叫
        [RelayCommand]
        private async Task DoorLightSwitchChanged(bool isChecked)
        {
            // 0: 關閉, 1: 開啟
            int lightSwitch = isChecked ? 1 : 0;
            string url = $"http://localhost:5032/PLC/EleMagazineDoorLightSwitch/0/{lightSwitch}";
            try
            {
               // await _httpService.SendPutAsync(url, new { });
            }
            catch
            {
                // 可加上錯誤提示
            }
        }
        #endregion

        #region Title

        #endregion
    }
}
