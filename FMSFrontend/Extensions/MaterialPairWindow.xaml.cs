using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
    /// MaterialPairWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MaterialPairWindow : Window
    {
        public MaterialPairWindow(bool isElectrode)
        {
            var windowService = new WindowService();
            InitializeComponent();
            DataContext = new MaterialPairViewModel(isElectrode, this, windowService);

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
