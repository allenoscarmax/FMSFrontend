using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FMSFrontend.Views.Windows
{
    /// <summary>
    /// AddWorrkerWindow.xaml 的互動邏輯 (登入使用者)
    /// </summary>
    public partial class AddWorrkerWindow : Window
    {
        public AddWorkerViewModel ViewModel { get; }
        public AddWorrkerWindow(List<LoginInfo> workers, string initialUserName)
        {
            InitializeComponent();
            // 傳遞 initialUserName 給 ViewModel 建構函式
            ViewModel = new AddWorkerViewModel(workers, initialUserName);
            if (!string.IsNullOrWhiteSpace(initialUserName))
                ViewModel.Name = initialUserName;
            DataContext = ViewModel;
        }

        // 新增的視窗拖曳事件處理
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        // 關閉按鈕事件處理
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        // 密碼與確認密碼 PasswordBox 內容同步到 ViewModel
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is AddWorkerViewModel vm && sender is PasswordBox pb)
            {
                if (pb.Name == nameof(PasswordInput))
                {
                    vm.Password = pb.Password;
                }
                else if (pb.Name == nameof(ConfirmPasswordInput))
                {
                    vm.ConfirmPassword = pb.Password;
                }
            }
        }
    }
}
