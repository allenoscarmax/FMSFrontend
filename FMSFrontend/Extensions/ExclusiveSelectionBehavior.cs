using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FMSFrontend.Extensions
{
    public static class ExclusiveSelectionBehavior
    {
        // 是否啟用「跨 DataGrid 互斥選取」
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(ExclusiveSelectionBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static void SetIsEnabled(DependencyObject element, bool value) =>
            element.SetValue(IsEnabledProperty, value);

        public static bool GetIsEnabled(DependencyObject element) =>
            (bool)element.GetValue(IsEnabledProperty);

        // 群組名稱：同一群組內的 DataGrid 互斥
        public static readonly DependencyProperty GroupProperty =
            DependencyProperty.RegisterAttached(
                "Group",
                typeof(string),
                typeof(ExclusiveSelectionBehavior),
                new PropertyMetadata("Default", OnGroupChanged));

        public static void SetGroup(DependencyObject element, string value) =>
            element.SetValue(GroupProperty, value);

        public static string GetGroup(DependencyObject element) =>
            (string)element.GetValue(GroupProperty);

        // ========== Internal Registry ==========
        private static readonly Dictionary<string, List<WeakReference<DataGrid>>> _groups = new();
        [ThreadStatic] private static bool _suppress;

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid grid) return;

            if ((bool)e.NewValue)
            {
                Register(grid);
                grid.SelectionChanged += Grid_SelectionChanged;
                grid.Unloaded += Grid_Unloaded;
            }
            else
            {
                grid.SelectionChanged -= Grid_SelectionChanged;
                grid.Unloaded -= Grid_Unloaded;
                Unregister(grid);
            }
        }

        private static void OnGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid grid) return;
            if (!GetIsEnabled(grid)) return;

            // 換群組：先移除舊的再加入新的
            Unregister(grid, oldGroup: (string)e.OldValue);
            Register(grid);
        }

        private static void Grid_Unloaded(object? sender, RoutedEventArgs e)
        {
            if (sender is DataGrid grid)
            {
                grid.SelectionChanged -= Grid_SelectionChanged;
                grid.Unloaded -= Grid_Unloaded;
                Unregister(grid);
            }
        }

        private static void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppress) return;

            var grid = (DataGrid)sender;

            // 沒有實際選取就不處理（清空 selection 不需要再去清別人）
            if (grid.SelectedItem == null) return;

            string group = GetGroup(grid);

            _suppress = true;
            try
            {
                foreach (var other in GetAliveGrids(group).Where(g => !ReferenceEquals(g, grid)))
                {
                    // 清掉其他表格的選取
                    other.SelectedItem = null;
                    other.UnselectAll();
                }
            }
            finally
            {
                _suppress = false;
            }
        }

        private static void Register(DataGrid grid)
        {
            string group = GetGroup(grid);

            if (!_groups.TryGetValue(group, out var list))
                _groups[group] = list = new List<WeakReference<DataGrid>>();

            // 清理死掉的 reference，並避免重複註冊
            list.RemoveAll(wr => !wr.TryGetTarget(out _));
            if (!list.Any(wr => wr.TryGetTarget(out var g) && ReferenceEquals(g, grid)))
                list.Add(new WeakReference<DataGrid>(grid));
        }

        private static void Unregister(DataGrid grid, string? oldGroup = null)
        {
            string group = oldGroup ?? GetGroup(grid);
            if (!_groups.TryGetValue(group, out var list)) return;

            list.RemoveAll(wr => !wr.TryGetTarget(out var g) || ReferenceEquals(g, grid));
            if (list.Count == 0)
                _groups.Remove(group);
        }

        private static IEnumerable<DataGrid> GetAliveGrids(string group)
        {
            if (!_groups.TryGetValue(group, out var list))
                yield break;

            // 清掉死的
            list.RemoveAll(wr => !wr.TryGetTarget(out _));

            foreach (var wr in list)
                if (wr.TryGetTarget(out var g))
                    yield return g;
        }
    }
}
