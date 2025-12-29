using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace FMSFrontend.Extensions
{
    public static class RowDetailsToggleBehavior
    {

        public static readonly DependencyProperty ExpandColumnIndexProperty =
    DependencyProperty.RegisterAttached(
        "ExpandColumnIndex",
        typeof(int),
        typeof(RowDetailsToggleBehavior),
        new PropertyMetadata(0));

        public static void SetExpandColumnIndex(DependencyObject element, int value) =>
            element.SetValue(ExpandColumnIndexProperty, value);

        public static int GetExpandColumnIndex(DependencyObject element) =>
            (int)element.GetValue(ExpandColumnIndexProperty);

        // 是否啟用：點擊展開/收合
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(RowDetailsToggleBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static readonly DependencyProperty KeepSelectionProperty =
    DependencyProperty.RegisterAttached(
        "KeepSelection",
        typeof(bool),
        typeof(RowDetailsToggleBehavior),
        new PropertyMetadata(true));

        public static void SetKeepSelection(DependencyObject element, bool value) =>
            element.SetValue(KeepSelectionProperty, value);

        public static bool GetKeepSelection(DependencyObject element) =>
            (bool)element.GetValue(KeepSelectionProperty);

        public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);
        public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

        // 是否只允許點到特定 Tag 才展開（例如 ExpandClickArea）
        public static readonly DependencyProperty ToggleTagProperty =
            DependencyProperty.RegisterAttached(
                "ToggleTag",
                typeof(string),
                typeof(RowDetailsToggleBehavior),
                new PropertyMetadata(""));

        public static void SetToggleTag(DependencyObject element, string value) => element.SetValue(ToggleTagProperty, value);
        public static string GetToggleTag(DependencyObject element) => (string)element.GetValue(ToggleTagProperty);

        // 是否點整列就展開
        public static readonly DependencyProperty ToggleOnRowClickProperty =
            DependencyProperty.RegisterAttached(
                "ToggleOnRowClick",
                typeof(bool),
                typeof(RowDetailsToggleBehavior),
                new PropertyMetadata(true));

        public static void SetToggleOnRowClick(DependencyObject element, bool value) => element.SetValue(ToggleOnRowClickProperty, value);
        public static bool GetToggleOnRowClick(DependencyObject element) => (bool)element.GetValue(ToggleOnRowClickProperty);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid grid) return;
            System.Diagnostics.Debug.WriteLine($"RowDetailsToggleBehavior hooked: {grid.Name}");
            if ((bool)e.NewValue)
                grid.PreviewMouseLeftButtonDown += Grid_PreviewMouseLeftButtonDown;
            else
                grid.PreviewMouseLeftButtonDown -= Grid_PreviewMouseLeftButtonDown;
        }

        private static void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGrid grid) return;
            System.Diagnostics.Debug.WriteLine($"Grid_PreviewMouseLeftButtonDown fired: {grid.Name}");

            // 1) 避免點到 Button/CheckBox/TextBox 等互動控制項時誤觸展開
            if (IsInsideInteractiveControl(e.OriginalSource as DependencyObject))
                return;

            // 2) 找到所屬的 DataGridRow
            var row = FindAncestor<DataGridRow>(e.OriginalSource as DependencyObject);
            if (row?.Item == null) return;

            // 3) 決定是否允許觸發展開：支援「只點 Tag 區域才展開」
           // string toggleTag = GetToggleTag(grid);
            bool toggleOnRowClick = GetToggleOnRowClick(grid);
            // 3) 必須點到指定欄位(預設第 0 欄)才展開
            if (!toggleOnRowClick)
            {
                // 只允許點指定欄位才展開
                var cell = FindAncestor<DataGridCell>(e.OriginalSource as DependencyObject);
                if (cell == null) return;

                int expandIndex = GetExpandColumnIndex(grid);
                int clickedIndex = cell.Column?.DisplayIndex ?? -1;

                if (clickedIndex != expandIndex)
                    return;
            }

            // 4) 先讓 DataGrid 正常選取（不要吃事件），但你也可以主動設定：
            bool keepSelection = GetKeepSelection(grid);

            if (keepSelection)
            {
                // ✅ Processing：要能選取，用於 Revise
                // 這行可留可不留；留著可確保 VM 的 SelectedItem 立刻同步
                grid.SelectedItem = row.Item;
                grid.Focus();

                // ✅ 關鍵：不要 e.Handled，讓 DataGrid 原生選取/反白流程繼續
            }
            else
            {
                // ✅ New：只要展開，不要反白
                e.Handled = true;
            }
            /*
            if (keepSelection)
            {
                grid.SelectedItem = row.Item;
                row.IsSelected = true;
                grid.Focus();
            }
            else
            {
                grid.SelectedItem = row.Item;
                // 不保留選取時，阻止 DataGrid 處理 Selection
                e.Handled = true;
            }*/

            // 5) 切換 IsExpanded，並同步更新 row.DetailsVisibility（不依賴 PropertyChanged）
            var prop = row.Item.GetType().GetProperty("IsExpanded");
            if (prop?.PropertyType == typeof(bool) && prop.CanWrite)
            {
                bool current = (bool)prop.GetValue(row.Item)!;
                bool next = !current;

                prop.SetValue(row.Item, next);

                // 這行是關鍵：讓效果跟你原本 code-behind 一樣
                row.DetailsVisibility = next ? Visibility.Visible : Visibility.Collapsed;

            }
        //    
            // 重要：不要 e.Handled = true; 讓 DataGrid 繼續跑原生選取流程
        }

        private static bool IsInsideInteractiveControl(DependencyObject? source)
        {
            while (source != null)
            {
                if (source is ButtonBase || source is TextBoxBase || source is Hyperlink)
                    return true;
                source = VisualTreeHelper.GetParent(source);
            }
            return false;
        }

        private static bool IsInsideTag(DependencyObject? source, string tag)
        {
            while (source != null)
            {
                if (source is FrameworkElement fe && fe.Tag is string s && s == tag)
                    return true;
                source = VisualTreeHelper.GetParent(source);
            }
            return false;
        }

        private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T t) return t;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
