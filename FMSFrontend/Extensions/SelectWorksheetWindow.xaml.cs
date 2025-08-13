using FMSFrontend.Services;
using FMSFrontend.ViewModels;
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
    /// SelectWorksheetWindow.xaml 的互動邏輯
    /// </summary>
    public partial class SelectWorksheetWindow : Window
    {
        public SelectWorksheetWindow()
        {
            InitializeComponent();
            // 建立 ViewModel 並注入服務
            SelectWorksheetWindowViewModel viewModel = new SelectWorksheetWindowViewModel();

            this.DataContext = viewModel;
        }
    }
}
