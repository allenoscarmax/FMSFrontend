using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Extensions;
using FMSFrontend.Services;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows;
using FMSFrontend.Helpers;
using System.Windows.Controls; // 放在你的 ViewModel 上方
using FMSFrontend.Views;

namespace FMSFrontend.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly IHttpService _httpService;

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

        public bool IsLoggedIn => !string.IsNullOrEmpty(LoggedInUser);

        public MainWindowViewModel(IHttpService httpService)
        {
            _httpService = httpService;
            currentPageView = new MachineOverviewPage();
            // 初始化時間更新
            Task.Run(async () =>
            {
                while (true)
                {
                    CurrentDateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    await Task.Delay(1000);
                }
            });
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
        private void GoToManual()
        {
            CurrentPageView = new Manual();
            CurrentPageKey = "Manual";
        }
        [RelayCommand]
        private void GoToSettingsview()
        {
            CurrentPageView = new Settingsview();
            CurrentPageKey = "Settingsview";
        }

            #endregion
            #region PowerButton
            [RelayCommand]
            private void PowerButtonClick()
            {
                // MessageBox.Show("關機囉！");

                // 切換風格的測試邏輯
                string current = ThemeManager.CurrentThemeName;
                string nextTheme = current == "Dark" ? "Light" : "Dark";
                ThemeManager.ApplyTheme(nextTheme);

                MessageBox.Show($"套用主題：{nextTheme}");
                //System.Windows.Application.Current.Shutdown();
            }
            #endregion
            #region Logout
            [RelayCommand]
            private void Logout()
            {
                // 這裡可以補上實際的登出處理，例如呼叫 API 或清除 token
                LoggedInUser = string.Empty; // 清除登入者資訊
                MessageBox.Show("您已成功登出！");
            }
            #endregion
            #region Login
            [RelayCommand]
            private void Login()
            {
                // TODO: 改為呼叫後台 API 取得使用者資訊
                LoggedInUser = "王小明"; // 登入成功後設定使用者名稱
                MessageBox.Show($"歡迎登入，{LoggedInUser}！");
            }
            #endregion


        } 
    }
