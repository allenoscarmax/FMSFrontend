using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Extensions;
using FMSFrontend.Helpers;
using FMSFrontend.Models;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls; // 放在你的 ViewModel 上方
using System.Windows.Media.Animation;

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
                Name = "主線機器人",
                CurrentLocation = "EDM1",
                CurrentAction = "搬運",
                NextAction = "上架"
            };
            AlarmVM = alarmVM;

        }


        #region PageChange
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
        private void GoToProductionLines()
        {
            CurrentPageView = new ProductionLines();
            CurrentPageKey = "ProductionLines";
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
        private void GoToMaterail()
        {
            CurrentPageView = new InventoryInformationPage();
            CurrentPageKey = "Materail";
        }
        [RelayCommand]
        private void GoToSettingsview()
        {
            CurrentPageView = new Settingsview();
            CurrentPageKey = "Settingsview";
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


    }
}
