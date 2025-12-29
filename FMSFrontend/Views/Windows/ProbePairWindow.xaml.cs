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
using FMSFrontend.Features.Services; // 新增: 各種服務介面/實作
using FMSFrontend.Features.Singleton; // 新增: RFIDBindStore
using FMSFrontend.Features.Threading; // 新增: RFIDBindLiveUpdater

namespace FMSFrontend.Views.Windows
{
    /// <summary>
    /// ProbePairWindow.xaml 的互動邏輯
    /// </summary>
    public partial class ProbePairWindow : Window
    {
        public ProbePairWindow(ProbePairViewModel probePairViewModel)
        {   
            InitializeComponent();

            DataContext = probePairViewModel;
            Loaded += (_, __) => probePairViewModel.OnPageActivated();
            Unloaded += (_, __) => probePairViewModel.OnPageDeactivated();
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
