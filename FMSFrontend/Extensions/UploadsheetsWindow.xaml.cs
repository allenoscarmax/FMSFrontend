using FMSFrontend.Features.Services;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FMSFrontend.Extensions
{
    /// <summary>
    /// UploadsheetsWindow.xaml 的互動邏輯
    /// </summary>
    public partial class UploadsheetsWindow : Window
    {
        public UploadsheetsWindow(UploadSheetViewModel uploadSheetViewModel)
        {
            this.DataContext = uploadSheetViewModel;
            InitializeComponent();
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove(); // 支援拖曳視窗
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }
    }
}
