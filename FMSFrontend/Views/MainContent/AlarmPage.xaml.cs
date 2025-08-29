using FMSFrontend.Services;
using FMSFrontend.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FMSFrontend.Views
{
    /// <summary>
    /// AlarmPage.xaml 的互動邏輯
    /// </summary>
    public partial class AlarmPage : UserControl
    {
        public AlarmPage()
        {
            InitializeComponent();
            // 建立服務實例
            var windowService = new WindowService();

            // 建立 ViewModel 並注入服務
            var viewModel = new AlarmPageViewModel(windowService);

            this.DataContext = viewModel;
        }
    }
}
