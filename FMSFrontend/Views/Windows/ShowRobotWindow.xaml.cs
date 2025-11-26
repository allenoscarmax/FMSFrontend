using FMSFrontend.Models;
using FMSFrontend.Services;
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

namespace FMSFrontend.Views.Windows
{
    /// <summary>
    /// ShowRobotWindow.xaml 的互動邏輯
    /// </summary>
    public partial class ShowRobotWindow : Window
    {
        public ShowRobotWindow(IHttpService httpService, Robot robot)
        {
            InitializeComponent();
            this.DataContext = new ViewModels.Windows.ShowRobotViewModel(httpService, robot);
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove(); // 支援拖曳視窗
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
