// ViewModels/TaskListViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using FMSFrontend.Models;
using FMSFrontend.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace FMSFrontend.ViewModels.Factory
{
    public partial class TaskListViewModel : ObservableObject
    {
        public ObservableCollection<TaskItem> TaskItems { get; } = new();

        // 給 XAML 綁 ItemsSource
        public ICollectionView TasksView { get; }

        public TaskListViewModel()
        {
            // 假資料
            TaskItems.Add(new TaskItem
            {
                Priority = 1,
                MachineName = "EDM1",
                ProgressPercent = 80,
                Summary = "安裝電極到放電加工機 (EDM)",
                WorkOrderId = "WO-2025-0001",
                MaterialText = "電極 E-001"
            });
            TaskItems.Add(new TaskItem
            {
                Priority = 3,
                MachineName = "EDM1",
                ProgressPercent = 0,
                Summary = "安裝電極到放電加工機 (EDM)",
                WorkOrderId = "WO-2025-0003",
                MaterialText = "工件 W-112"
            });
            TaskItems.Add(new TaskItem
            {
                Priority = 2,
                MachineName = "EDM3",
                ProgressPercent = 0,
                Summary = "安裝電極到放電加工機 (EDM)",
                WorkOrderId = "WO-2025-0002",
                MaterialText = "電極 E-014"
            });

            TaskItems.Add(new TaskItem
            {
                Priority = 1,
                MachineName = "CNC",
                ProgressPercent = 0,
                Summary = "加工電極 (CNC)",
                WorkOrderId = "WO-2025-0002",
                MaterialText = "電極 E-014"
            });

            // 建立 View 並依 Priority 由小到大排序
            TasksView = CollectionViewSource.GetDefaultView(TaskItems);
            TasksView.SortDescriptions.Clear();
            TasksView.SortDescriptions.Add(
                new SortDescription(nameof(TaskItem.Priority), ListSortDirection.Ascending));

            // 啟用 Live Sorting（Priority 改變或有新任務加入時即時重排）
            if (TasksView is ICollectionViewLiveShaping live)
            {
                live.IsLiveSorting = true;
                live.LiveSortingProperties.Add(nameof(TaskItem.Priority));
            }
        }

        // 之後你要插入新任務 → 直接 Add；View 會依 Priority 自動重排
        public void EnqueueTask(TaskItem task) => TaskItems.Add(task);
    }

    public partial class TaskItem : ObservableObject
    {
        [ObservableProperty] public int priority;            // 數字越小越前面
        [ObservableProperty] public string machineName = ""; // EDM1、WEDM2...
        [ObservableProperty] public double progressPercent;  // 0–100
        [ObservableProperty] public string summary = "";     // 任務簡述
        [ObservableProperty] public string workOrderId = ""; // WO-XXXX
        [ObservableProperty] public string materialText = ""; // 工件/電極 顯示文字

        // 方便直接綁定顯示
        public string WorkOrderDisplay => $"工單編號：{WorkOrderId}";
        public string MaterialDisplay => $"工件/電極：{MaterialText}";
    }
}
