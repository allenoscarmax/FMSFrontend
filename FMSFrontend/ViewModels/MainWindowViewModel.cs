using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Controls;
using FMSFrontend.Extensions;
using FMSFrontend.Helpers;
using FMSFrontend.Models;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views;
using OSCARMAXFMS_V3.DBmodels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls; // 放在你的 ViewModel 上方
using System.Windows.Media;
using System.Windows.Media.Animation;
using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace FMSFrontend.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
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
        private bool _isAlarm   = false;

        [ObservableProperty]
        private string _summaryMessage = "系統正常運作";

        [ObservableProperty]
        private string _sDispatchText = "派工啟動";

        [ObservableProperty]
        private bool _isDispatch;

        [ObservableProperty]
        public ObservableCollection<Brush> _upperDoorLights1 =
            new ObservableCollection<Brush>(Enumerable.Repeat(Brushes.Gray, 8));

        [ObservableProperty]
        public ObservableCollection<Brush> _upperDoorLights2 =
            new ObservableCollection<Brush>(Enumerable.Repeat(Brushes.Gray, 8));

        [ObservableProperty]
        public ObservableCollection<Brush> _lowerDoorLights1 =
            new ObservableCollection<Brush>(Enumerable.Repeat(Brushes.Gray, 8));

        [ObservableProperty]
        public ObservableCollection<Brush> _lowerDoorLights2 =
            new ObservableCollection<Brush>(Enumerable.Repeat(Brushes.Gray, 8));

        // 新增：ASRS 參數
        [ObservableProperty]
        private ObservableCollection<AppointmentMaintenance> _asrsParameters = new();

        // 新增：天氣資料（對應 DBmodels\WeatherData.cs）
        [ObservableProperty]
        private WeatherData? _weather;

        // 既有：每3秒輪詢控制
        private readonly DispatcherTimer _asrsTimer = new() { Interval = TimeSpan.FromSeconds(3) };
        private bool _isPollingAsrs;

        // 新增：最高優先請求佇列（有東西時優先執行）
        private readonly Queue<Func<Task>> _highPriorityRequests = new();

        // 便捷加入高優先請求的方法（可依需求在外部呼叫）
        public void EnqueueHighPriorityAsrs() => _highPriorityRequests.Enqueue(FetchAsrsParametersAsync);
        public void EnqueueHighPriorityWeather() => _highPriorityRequests.Enqueue(FetchWeatherAsync);

        public bool IsLoggedIn => !string.IsNullOrEmpty(LoggedInUser);

        public MainWindowViewModel(IHttpService httpService, AlarmPageViewModel alarmVM)
        {
            _httpService = httpService;
            //currentPageView = new MachineOverviewPage();
            StorageControlPage = new StorageUnitMiniControlPage();
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
                SelectedRobotIndexDisplay = _robotActionIndex.ToString() + " / 3",
                IsMultipleRobotVisible = true
            };
            AlarmVM = alarmVM;

            // 啟動每秒輪詢
            _asrsTimer.Tick += async (_, __) => await PollAsrsAsync();
            _asrsTimer.Start();
        }

        // 改成：先跑高優先，否則依不同頁面執行不同 case
        private async Task PollAsrsAsync()
        {
            if (_isPollingAsrs) return;
            _isPollingAsrs = true;
            try
            {
                // 1) 高優先請求優先執行
                if (_highPriorityRequests.Count > 0)
                {
                    var job = _highPriorityRequests.Dequeue();
                    await job();
                    return;
                }

                // 2) 依當前頁面執行對應查詢
                switch (CurrentPageKey)
                {
                    case "ProductionLines":
                        await FetchAsrsParametersAsync();
                        break;
                    case "FactoryOverview":

                        break;
                    case "MachineOverview":

                        break;
                    case "WorkOrder":

                        break;
                    case "RFIDBind":

                        break;
                    case "OperationHistory":

                        break;
                    case "Material":

                        break;
                    case "SettingsView":
                        break;
                    // 其他頁面：預設跑 ASRS
                    default:
                      //  await FetchAsrsParametersAsync();
                        break;
                }
            }
            catch
            {
                // 忽略暫時性錯誤
            }
            finally
            {
                _isPollingAsrs = false;
            }
        }

        // 新增：封裝 ASRS 資料抓取
        private async Task FetchAsrsParametersAsync()
        {
            var list = await _httpService.GetJsonAsync<List<AppointmentMaintenance>>("ASRS/GetASRSParameter");
            if (list == null) return;

            AsrsParameters.Clear();
            foreach (var item in list)
                AsrsParameters.Add(item);
        }

        // 新增：封裝天氣資料抓取
        private async Task FetchWeatherAsync()
        {
            const string url = "https://api.open-meteo.com/v1/forecast?latitude=24.15&longitude=120.65&current=temperature_2m,wind_speed_10m";
            var data = await _httpService.GetJsonAsync<WeatherData>(url);
            if (data != null)
                Weather = data;
        }

        #region PageChange
        [RelayCommand]
        private void GoToProductionLines()
        {
            CurrentPageView = new ProductionLines();
            CurrentPageKey = "ProductionLines";
        }

        [RelayCommand]
        private void GoToFactoryOverview()
        {
            CurrentPageView = new FactoryOverviewPage();
            CurrentPageKey = "FactoryOverview";
        }
        [RelayCommand]
        private void GoToMachineOverview()
        {
            CurrentPageView = new MachineOverviewPage();
            CurrentPageKey = "MachineOverview";
        }

        [RelayCommand]
        private void GoToWorkOrder()
        {
            CurrentPageView = new WorkOrder();
            CurrentPageKey = "WorkOrder";
        }
        [RelayCommand]
        private void GoToRFIDBind()
        {
            CurrentPageView = new RFIDBind();
            CurrentPageKey = "RFIDBind";
        }
        [RelayCommand]
        private void GoToOperationHistory()
        {
            CurrentPageView = new OperationHistory();
            CurrentPageKey = "OperationHistory";
        }
        [RelayCommand]
        private void GoToMaterial()
        {
            CurrentPageView = new InventoryInformationPage();
            CurrentPageKey = "Material";
        }
        [RelayCommand]
        private void GoToSettingsView()
        {
            CurrentPageView = new SettingsView();
            CurrentPageKey = "SettingsView";
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
                await _httpService.SendPutAsync(route, new { });
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
                await _httpService.SendPutAsync(route, new { });
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
                await _httpService.SendPutAsync(route, new { });
            }
            catch{ }
        }
        [RelayCommand]
        private async Task RobotResetButtonClick()
        {
            const string route = "http://localhost:5032/ASRS/SetASRSRobotReset";
            var dialog = new DialogMessageWindow("Reset");
            dialog.ShowDialog();
            try
            {
                await _httpService.SendPutAsync(route, new { });
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
                await _httpService.SendPutAsync(route, new { });
            }
            catch { }
        }
        #endregion

        #region Robot
        // 加入：目前指向 NextAction 的索引（-1 代表尚未初始化）
        private int _robotActionIndex = 1;
        [RelayCommand]
        private void NextRobot()
        {
            if (Robot == null) return;
            if (_robotActionIndex == 3) _robotActionIndex = 1;
            else _robotActionIndex++;
            Robot.Name = "名稱" + _robotActionIndex.ToString();
            Robot.CurrentLocation = "目前位置" + _robotActionIndex.ToString();
            Robot.CurrentAction = "目前動作 " + _robotActionIndex.ToString();
            Robot.NextAction = "下個動作 " + _robotActionIndex.ToString();
            Robot.SelectedRobotIndexDisplay = _robotActionIndex.ToString() + " / " + "3";
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
            Robot.SelectedRobotIndexDisplay = _robotActionIndex.ToString() + " / " + "3";
            Robot.IsMultipleRobotVisible = true;
        }
        #endregion
    }
}
