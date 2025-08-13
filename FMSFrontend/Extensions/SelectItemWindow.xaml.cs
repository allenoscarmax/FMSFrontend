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
    /// SelectItemWindow.xaml 的互動邏輯
    /// </summary>
    public partial class SelectItemWindow : Window
    {
        public SelectItemWindow(SelectItemType type,
             Func<SelectItemType, IEnumerable<SelectItem>> dataLoader = null)
        {
            InitializeComponent();

            // 直接設置 DataContext（你的 XAML 沒有設 DataContext）
            DataContext = new SelectItemWindowViewModel(type, dataLoader);
        }
    }
}
