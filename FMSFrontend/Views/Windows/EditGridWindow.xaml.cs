using FMSFrontend.ViewModels.Windows;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace FMSFrontend.Views.Windows
{
    /// <summary>
    /// EditGridWindow.xaml 的互動邏輯
    /// </summary>
    public partial class EditGridWindow : Window
    {
        public EditGridViewModel ViewModel { get; }

        public EditGridWindow(List<EditGridViewModel.EditGridInfo> items, string title = "編輯")
        {
            InitializeComponent();
            ViewModel = new EditGridViewModel(items, title);
            DataContext = ViewModel;
        }

        // 視窗拖曳事件處理
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
    }
}
