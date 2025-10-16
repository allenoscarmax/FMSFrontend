using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IniFile;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace FMSFrontend.ViewModels.Windows
{
    public partial class ShutdownWindowViewModel : ObservableObject
    {
        // 使用 [ObservableProperty] 屬性，它會自動生成 IsRobotRunning 屬性
        // 並處理 INotifyPropertyChanged 的通知
        [ObservableProperty]
        private bool _isRobotRunning;
        [ObservableProperty]
        private string _shutdownMessage;

        public ShutdownWindowViewModel(bool isRobotRunning)
        {
            _isRobotRunning = isRobotRunning;
            UpdateMessage();
        }

        // 這裡可以放你的指令，例如處理按鈕點擊
        [RelayCommand]
        private void ShutdownComfirm()
        {
            if (Application.Current.MainWindow?.DataContext is FMSFrontend.ViewModels.MainWindowViewModel mainVM)
            {
                mainVM.SaveCurrentStoragePageType();
            }
            System.Windows.Application.Current.Shutdown();
            // 執行關機邏輯
        }
        [RelayCommand]
        private void CloseWindow(Window window)
        {

            window?.Close();
        }

        private void UpdateMessage()
        {
            if (_isRobotRunning)
            {
                _shutdownMessage = "手臂正在運行中！確定要關機？";
            }
            else
            {
                _shutdownMessage = "確定要關閉控制系統？";
            }
        }
    }
}
