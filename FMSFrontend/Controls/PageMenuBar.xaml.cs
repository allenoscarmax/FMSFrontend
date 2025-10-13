using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace FMSFrontend.Controls
{
    /// <summary>
    /// PageMenuBar.xaml 的互動邏輯
    /// </summary>
    public partial class PageMenuBar : UserControl
    {
        public static readonly DependencyProperty CurrentPageKeyProperty =
            DependencyProperty.Register(nameof(CurrentPageKey), typeof(string), typeof(PageMenuBar),
                new PropertyMetadata(null, OnCurrentPageKeyChanged));

        public string CurrentPageKey
        {
            get => (string)GetValue(CurrentPageKeyProperty);
            set => SetValue(CurrentPageKeyProperty, value);
        }

        public PageMenuBar()
        {
            InitializeComponent();

            // 確保第一次載入後也會套用選取狀態
            Loaded += (_, __) => UpdateSelection();
        }

        private static void OnCurrentPageKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PageMenuBar bar)
            {
                // 在 UI 已載入後更新；若尚未載入，排程到 Loaded 時機
                if (bar.IsLoaded)
                {
                    bar.UpdateSelection();
                }
                else
                {
                    bar.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(bar.UpdateSelection));
                }
            }
        }

        private void UpdateSelection()
        {
            if (!IsLoaded || MenuItemsContainer == null) return;

            var items = MenuItemsContainer.Children.OfType<PageMenuItem>().ToList();

            // Step 1：找出選中的 index，同時設定 IsSelected
            int selectedIndex = -1;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                // 不要覆蓋 XAML 上的 Binding：移除 item.CurrentPageKey = this.CurrentPageKey;
                item.IsSelected = string.Equals(item.PageKey, this.CurrentPageKey, StringComparison.Ordinal);

                if (item.IsSelected)
                    selectedIndex = i;
            }

            // Step 2：根據選中位置設圓角鄰接旗標
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                item.IsPreviousToSelected = (i + 1 == selectedIndex);
                item.IsNextToSelected = (i - 1 == selectedIndex);

                item.UpdateCornerRadius();
            }
        }
    }
}
