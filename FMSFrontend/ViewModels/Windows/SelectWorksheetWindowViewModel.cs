using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FMSFrontend.ViewModels.Windows
{
    public class WorksheetItem
    {
        public string PartName { get; set; } = "";
        public string WorkOrderNo { get; set; } = "";
        public override string ToString() => $"{PartName} ({WorkOrderNo})";
    }

    public class SelectWorksheetWindowViewModel : ObservableObject
    {
        // ===== 標題列 =====
        private string _title = "選擇工單";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private Brush _titleBrush =
            (SolidColorBrush)new BrushConverter().ConvertFrom("#555555")!;
        public Brush TitleBrush
        {
            get => _titleBrush;
            set => SetProperty(ref _titleBrush, value);
        }

        // ===== 搜尋 =====
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    ItemsView.Refresh();
            }
        }

        // ===== 清單 =====
        public ObservableCollection<WorksheetItem> WorksheetItems { get; }
        public ICollectionView ItemsView { get; }

        private WorksheetItem? _selectedWorksheet;
        public WorksheetItem? SelectedWorksheet
        {
            get => _selectedWorksheet;
            set
            {
                if (SetProperty(ref _selectedWorksheet, value))
                {
                    // 直接對 IRelayCommand 呼叫，無需轉型
                    ConfirmCommand.NotifyCanExecuteChanged();
                }
            }
        }

        // ===== Commands =====
        public IRelayCommand<object?> SearchCommand { get; }
        public IRelayCommand<Window?> ConfirmCommand { get; }
        public IRelayCommand<Window?> CancelCommand { get; }

        public SelectWorksheetWindowViewModel(IEnumerable<WorksheetItem>? items = null)
        {
            WorksheetItems = new ObservableCollection<WorksheetItem>(items ?? GetDesignItems());
            ItemsView = CollectionViewSource.GetDefaultView(WorksheetItems);
            ItemsView.Filter = FilterItem;

            // ✅ 明確使用 CommunityToolkit 的 RelayCommand
            SearchCommand = new CommunityToolkit.Mvvm.Input.RelayCommand<object?>(OnSearch);
            ConfirmCommand = new CommunityToolkit.Mvvm.Input.RelayCommand<Window?>(OnConfirm, _ => SelectedWorksheet != null);
            CancelCommand = new CommunityToolkit.Mvvm.Input.RelayCommand<Window?>(OnCancel);
        }

        private bool FilterItem(object obj)
        {
            if (obj is not WorksheetItem it) return false;
            if (string.IsNullOrWhiteSpace(SearchText)) return true;

            var q = SearchText.Trim();
            return it.PartName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                   it.WorkOrderNo.Contains(q, StringComparison.OrdinalIgnoreCase);
        }

        private void OnSearch(object? param)
        {
            if (param is bool enterOnly && !enterOnly) return;
            ItemsView.Refresh();
        }

        private void OnConfirm(Window? win)
        {
            if (win == null) return;

            if (SelectedWorksheet != null)
            {
                win.Tag = SelectedWorksheet; // 讓呼叫端可取回
                try { win.DialogResult = true; } catch { win.Close(); }
            }
            else
            {
                try { win.DialogResult = false; } catch { win.Close(); }
            }
        }


        private void OnCancel(Window? win)
        {
            if (win == null) return;
            try { win.DialogResult = false; } catch { win.Close(); }
        }

        private static IEnumerable<WorksheetItem> GetDesignItems() => new[]
        {
            new WorksheetItem { PartName = "", WorkOrderNo = "" },
         };
    }
}
