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
        }

        private static void OnCurrentPageKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PageMenuBar bar)
            {
                bar.UpdateSelection();
            }
        }

        private void UpdateSelection()
        {
            var items = MenuItemsContainer.Children.OfType<PageMenuItem>().ToList();

            // Step 1：先找選中的項目
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                item.CurrentPageKey = this.CurrentPageKey;
                item.IsSelected = item.PageKey == this.CurrentPageKey;
            }

            // Step 2：再根據選中位置設圓角
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                item.IsPreviousToSelected = (i < items.Count - 1) && items[i + 1].IsSelected;
                item.IsNextToSelected = (i > 0) && items[i - 1].IsSelected;

                item.UpdateCornerRadius();
            }
        }
    }

}
