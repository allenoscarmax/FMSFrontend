using FMSFrontend.Services;
using FMSFrontend.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FMSFrontend.Views
{
    /// <summary>
    /// AlarmPage.xaml 的互動邏輯
    /// </summary>
    public partial class AlarmPage : UserControl
    {
        public AlarmPage(AlarmPageViewModel viewModel)
        {
            InitializeComponent();
            // 建立服務實例
            var windowService = new WindowService();

            //// 建立 ViewModel 並注入服務
            //var viewModel = new AlarmPageViewModel();

            //this.DataContext = viewModel;

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                // 執行時：從 DI 取同一個 Singleton VM
                DataContext = viewModel;
            }
            else
            {
                // 設計時：給一個乾淨 VM 或 stub，避免設計器爆紅
                DataContext = new AlarmPageViewModel(windowService);
            }
        }
    }
}
