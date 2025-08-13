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
        public SelectItemType Type { get; }

        [ObservableProperty] private string title = "選擇物料";
        [ObservableProperty] private Brush titleBrush = Brushes.SlateGray;

        [ObservableProperty] private string searchText = "";
        [ObservableProperty] private SelectItem selectedItem;

        public ObservableCollection<SelectItem> Items { get; } = new();
        public ICollectionView ItemsView { get; }

        // 可選：讓外部注入資料載入器（例如 API/Mongo）
        private readonly Func<SelectItemType, IEnumerable<SelectItem>> _dataLoader;

        public SelectItemWindowViewModel(SelectItemType type,
                                         Func<SelectItemType, IEnumerable<SelectItem>> dataLoader = null)
        {
            Type = type;
            _dataLoader = dataLoader;

            ItemsView = CollectionViewSource.GetDefaultView(Items);
            ItemsView.Filter = FilterItem;

            SetupByType();
            LoadData();
        }

        private void SetupByType()
        {
            switch (Type)
            {
                case SelectItemType.Electrode:
                    Title = "選擇電極";
                    TitleBrush = (Brush)new BrushConverter().ConvertFrom("#4078B3");
                    break;

                case SelectItemType.Workpiece:
                    Title = "選擇工件";
                    TitleBrush = (Brush)new BrushConverter().ConvertFrom("#E27B35");
                    break;
            }
        }

        private void LoadData()
        {
            Items.Clear();

            // 1) 若外部提供 loader，優先用
            if (_dataLoader != null)
            {
                foreach (var it in _dataLoader(Type))
                    Items.Add(it);
            }
            else
            {
                // 2) Demo 假資料（你之後可移除）
                if (Type == SelectItemType.Electrode)
                {
                    Items.Add(new SelectItem("25-001-015-001A-01", "Verified", Brushes.Green));
                    Items.Add(new SelectItem("25-001-015-001B-02", "Completed", BrushFrom("#3379FF")));
                    Items.Add(new SelectItem("25-001-015-001C-03", "OffSelf", Brushes.Gray));
                }
                else // Workpiece
                {
                    Items.Add(new SelectItem("WP-2025-0001", "Verified", Brushes.Green));
                    Items.Add(new SelectItem("WP-2025-0002", "Queued", BrushFrom("#E6C229")));
                    Items.Add(new SelectItem("WP-2025-0003", "Completed", BrushFrom("#3379FF")));
                }
            }

            ItemsView.Refresh();
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

        // 你 TextBox 的 Enter 已用 Converter 篩過了，只要刷新或執行搜尋即可
        [RelayCommand]
        private void Search(object _)
        {
            ItemsView.Refresh();
        }

        [RelayCommand]
        private void Confirm(Window win)
        {
            if (SelectedItem == null) return;
            // 用 Window.Tag 回傳選擇結果（跟你同事風格一致）
            win.Tag = SelectedItem;
            win.DialogResult = true;
            win.Close();
        }

        [RelayCommand]
        private void Cancel(Window win)
        {
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
