// ViewModels/TaskListViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Models;
using FMSFrontend.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace FMSFrontend.ViewModels.Factory
{
    public partial class TaskListViewModel : ObservableObject
    {
        private readonly ICommandScheduleService _commandScheduleService;
        // === Singleton ===
        private readonly CommandScheduleStore _commandSchedulesStore;
        public ObservableCollection<CommandScheduleModel> commandSchedules
            => _commandSchedulesStore.CommandSchedules.CommandSchedules;
        // ==LiveUpdater===
        private readonly CommandScheduleLiveUpdater _commandScheduleLiveUpdater;

        public ObservableCollection<TaskItem> TaskItems { get; } = new();
        

        public TaskListViewModel(ICommandScheduleService commandScheduleService,
            CommandScheduleStore commandScheduleStore,
            CommandScheduleLiveUpdater commandScheduleLiveUpdater)
        {
            _commandScheduleService = commandScheduleService;
            _commandSchedulesStore = commandScheduleStore;
            _commandScheduleLiveUpdater = commandScheduleLiveUpdater;
            _commandSchedulesStore.CommandSchedules.PropertyChanged += (_, __) => RefreshFromStore();
            // 假資料
            
        }
        public void OnPageActivated()
        {
            _commandScheduleLiveUpdater.Start();
        }
        public void OnPageDeactivated()
        {
            _commandScheduleLiveUpdater.Stop();
        }
        void RefreshFromStore()
        {
            var sorted = commandSchedules.OrderBy(x => x.Priority).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                if (TaskItems.Count == i) TaskItems.Add(new TaskItem());
                TaskItems[i].Priority        = sorted[i].Priority;
                TaskItems[i].MachineName     = sorted[i].TaskSource;
                TaskItems[i].ProgressPercent = sorted[i].ProgressPercent;
                TaskItems[i].Summary         = sorted[i].CommandString;
                TaskItems[i].InsertTimeString= sorted[i].InsertTimeString;
                TaskItems[i].StartPoint      = sorted[i].StartPoint;
                TaskItems[i].EndPoint        = sorted[i].EndPoint;
            }
            while (TaskItems.Count>sorted.Count)
            {
                TaskItems.RemoveAt(TaskItems.Count - 1);
            }
        }

        //以下假資料
        //public ICollectionView TasksView { get; }  // 給 XAML 綁 ItemsSource
        void TestData()
        {
            /*
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
            /*
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
            */
        }

        // 之後你要插入新任務 → 直接 Add；View 會依 Priority 自動重排
        public void EnqueueTask(TaskItem task) => TaskItems.Add(task);
    }

    public partial class TaskItem : ObservableObject
    {
        [ObservableProperty] public int priority;            // 數字越小越前面 0-100
        [ObservableProperty] public string machineName = ""; // EDM1、WEDM2...
        [ObservableProperty] public double progressPercent;  // 0–100
        [ObservableProperty] public string summary = "";     // 任務簡述 //CommandType
        [ObservableProperty] public string insertTimeString = ""; // WO-XXXX
        [ObservableProperty] public string startPoint = ""; // 工件/電極 顯示文字
        [ObservableProperty] public string endPoint = ""; // 工件/電極 顯示文字


        // 方便直接綁定顯示
        //public string WorkOrderDisplay => $"工單編號：{WorkOrderId}";
        //public string MaterialDisplay => $"工件/電極：{MaterialText}";
    }
}
