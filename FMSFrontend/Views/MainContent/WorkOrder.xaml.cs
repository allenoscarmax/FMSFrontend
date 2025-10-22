using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FMSFrontend.Services;
using FMSFrontend.ViewModels;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection; // ← 新增
using FMSFrontend.Interfaces;                 // ← 新增

namespace FMSFrontend.Views
{
    /// <summary>
    /// WorkOrder.xaml 的互動邏輯
    /// </summary>
    public partial class WorkOrder : UserControl
    {
        public WorkOrder()
        {
            InitializeComponent();
            this.PreviewMouseLeftButtonDown += WorkOrder_PreviewMouseLeftButtonDown;

                // 透過 DI 取得服務
            var windowService = App.ServiceProvider.GetRequiredService<IWindowService>();
            var httpService   = App.ServiceProvider.GetRequiredService<IHttpService>();

            // 建立 ViewModel 並注入服務
            var viewModel = new WorkOrderPageViewModel(windowService, httpService);

            this.DataContext = viewModel;
        }
        private void WorkOrder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 如果點擊的是 Button，就不處理展開
            if (IsInside<Button>(e.OriginalSource as DependencyObject))
                return;
            // 嘗試從事件來源往上找 DataGridRow
            DependencyObject current = e.OriginalSource as DependencyObject;

            while (current != null && !(current is DataGridRow))
            {
                current = VisualTreeHelper.GetParent(current);
            }

            if (current is DataGridRow row && row.Item is WorkOrderData data)
            {
                data.IsExpanded = !data.IsExpanded;
                row.DetailsVisibility = data.IsExpanded ? Visibility.Visible : Visibility.Collapsed;

                e.Handled = true; // 可選，防止冒泡干擾其他事件
            }
        }

        // 🔍 工具方法：檢查滑鼠是否點在 Button 或其子項內
        private bool IsInside<T>(DependencyObject source) where T : DependencyObject
        {
            while (source != null)
            {
                if (source is T) return true;
                source = VisualTreeHelper.GetParent(source);
            }
            return false;
        }


    }
}
