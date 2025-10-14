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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static FMSFrontend.ViewModels.ProductionLinesViewModel;

namespace FMSFrontend.Views
{
    /// <summary>
    /// ProductionLines.xaml 的互動邏輯
    /// </summary>
    public partial class ProductionLines : UserControl
    {
        public ProductionLines()
        {
            InitializeComponent();
            // 建立服務實例
            var windowService = new WindowService();
            this.DataContext = new ProductionLinesViewModel(windowService);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            if (this.Resources["MoveBorderStoryboard"] is Storyboard sb)
            {
                sb.Begin(this, true); // 第二個參數 true 讓動畫可以重複執行
            }
        }
    }
}
