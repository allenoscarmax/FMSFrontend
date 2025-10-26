using FMSFrontend.Extensions;
using FMSFrontend.ViewModels;
using System.Text;
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

namespace FMSFrontend.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private bool _isMenuBarVisible = true;
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void ToggleMenuBar_Click(object sender, RoutedEventArgs e)
        {
            //Storyboard sb;

            //if (_isMenuBarVisible)
            //{
            //    sb = (Storyboard)FindResource("CollapseMenuBar");

            //    // 換成向下箭頭：▼
            //    ArrowPath.Data = Geometry.Parse("M 0 0 L 10 10 L 20 0 Z");
            //}
            //else
            //{
            //    sb = (Storyboard)FindResource("ExpandMenuBar");

            //    // 換回向上箭頭：▲
            //    ArrowPath.Data = Geometry.Parse("M 0 10 L 10 0 L 20 10 Z");
            //}

            //sb.Begin();
            //_isMenuBarVisible = !_isMenuBarVisible;
        }

        // 修正：補上 XAML 綁定的 Loaded 事件處理函式
        private void PageMenuBar_Loaded(object sender, RoutedEventArgs e)
        {
            // 若需要在 PageMenuBar 載入後執行初始化，可在此加入邏輯。
            // 目前不需特別處理可留空，避免 XAML 解析錯誤。
        }

    }
}