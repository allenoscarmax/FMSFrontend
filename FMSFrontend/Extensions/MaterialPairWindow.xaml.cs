using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;

namespace FMSFrontend.Extensions
{
    /// <summary>
    /// MaterialPairWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MaterialPairWindow : Window
    {
        public MaterialPairWindow( MaterialPairViewModel materialPairViewModel)
        {
            InitializeComponent();
            DataContext = materialPairViewModel;
            // 讓 ViewModel 可以關閉這個視窗（但不用知道 Window 類別）
            materialPairViewModel.CloseAction = this.Close;
            Loaded += (_, __) => ((MaterialPairViewModel)DataContext).OnPageActivated();
            Unloaded += (_, __) => ((MaterialPairViewModel)DataContext).OnPageDeactivated();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }
    }
}
