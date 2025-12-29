using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace FMSFrontend.Extensions
{
    public partial class SelectItemWindowViewModel : ObservableObject
    {
        public SelectItemType Type { get; private set; }

        [ObservableProperty] private string title = "選擇物料";
        [ObservableProperty] private Brush titleBrush = Brushes.SlateGray;

        [ObservableProperty] private string searchText = "";
        [ObservableProperty] private SelectItem? selectedItem;

        public ObservableCollection<SelectItem> Items { get; } = new();
        public ICollectionView ItemsView { get; }

        public SelectItemWindowViewModel()
        {
            ItemsView = CollectionViewSource.GetDefaultView(Items);
            ItemsView.Filter = FilterItem;

            // 設計時假資料
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Initialize(SelectItemType.Electrode, GetDesignItems());
            }
        }

        /// <summary>
        /// 執行時由 WindowService 呼叫，設定類型與項目清單。
        /// </summary>
        public void Initialize(SelectItemType type, IEnumerable<SelectItem> items)
        {
            Type = type;

            SetupByType(type);

            Items.Clear();
            foreach (var it in items)
                Items.Add(it);

            ItemsView.Refresh();
        }

        private void SetupByType(SelectItemType type)
        {
            switch (type)
            {
                case SelectItemType.Electrode:
                    Title = "選擇電極";
                    TitleBrush = BrushFrom("#4078B3");
                    break;

                case SelectItemType.Workpiece:
                    Title = "選擇工件";
                    TitleBrush = BrushFrom("#E27B35");
                    break;

                default:
                    Title = "選擇物料";
                    TitleBrush = Brushes.SlateGray;
                    break;
            }
        }

        private IEnumerable<SelectItem> GetDesignItems()
        {
            yield return new SelectItem("DEMO-ITEM-01", "Idle", Brushes.Gray);
            yield return new SelectItem("DEMO-ITEM-02", "Queued", BrushFrom("#E6C229"));
        }

        private static Brush BrushFrom(string hex)
            => new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));

        private bool FilterItem(object obj)
        {
            if (obj is not SelectItem it) return false;
            if (string.IsNullOrWhiteSpace(SearchText)) return true;

            var key = SearchText.Trim();
            return it.MaterialName.Contains(key, StringComparison.OrdinalIgnoreCase)
                || it.MaterialState.Contains(key, StringComparison.OrdinalIgnoreCase);
        }

        partial void OnSearchTextChanged(string value) => ItemsView.Refresh();

        // 當選擇改變時更新 ConfirmCommand 可執行狀態
        partial void OnSelectedItemChanged(SelectItem? value)
        {
            ConfirmCommand.NotifyCanExecuteChanged();
        }

        // 你 TextBox 的 Enter 已用 Converter 篩過了，只要刷新或執行搜尋即可
        [RelayCommand]
        private void Search(object? _)
        {
            ItemsView.Refresh();
        }

        // 禁用確認按鈕直到選擇項目後才可使用
        private bool CanConfirm(Window? win) => SelectedItem != null;

        [RelayCommand(CanExecute = nameof(CanConfirm))]
        private void Confirm(Window? win)
        {
            if (win == null || SelectedItem == null) return;

            // 用 Window.Tag 回傳選擇結果（跟你現有風格一致）
            win.Tag = SelectedItem;
            win.DialogResult = true;
            win.Close();
        }

        [RelayCommand]
        private void Cancel(Window? win)
        {
            if (win == null) return;
            win.DialogResult = false;
            win.Close();
        }
    }
    public enum SelectItemType
    {
        Electrode, // 電極
        Workpiece  // 工件
    }
    public class SelectItem
    {
        public string MaterialName { get; }
        public string MaterialState { get; }
        public Brush StatusBrush { get; }

        public SelectItem(string name, string state, Brush brush)
        {
            MaterialName = name;
            MaterialState = state;
            StatusBrush = brush;
        }
    }
}
