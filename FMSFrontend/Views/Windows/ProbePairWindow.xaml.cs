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

namespace FMSFrontend.Views.Windows
{
    /// <summary>
    /// ProbePairWindow.xaml 的互動邏輯
    /// </summary>
    public partial class ProbePairWindow : Window
    {
        public ProbePairWindow()
        {
            InitializeComponent();
            var windowService = new WindowService();
            var httpService = new HttpService();

            InitializeComponent();
            DataContext = new ProbePairViewModel(this, windowService, httpService);
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
