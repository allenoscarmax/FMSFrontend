using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
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

            // 執行時：從 DI 進來的 viewModel
            DataContext = viewModel;

            Loaded += (_, __) => viewModel.OnPageActivated();
            Unloaded += (_, __) => viewModel.OnPageDeactivated();
        }
    }
}
