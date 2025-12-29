using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Models;
using System.Collections.ObjectModel;

namespace FMSFrontend.ViewModels.Windows
{
    /// <summary>
    /// 純資訊用：從 Robot/機台/搜尋等情境開啟
    /// - 不顯示倉位/操作列（SlotCode 預期為 null）
    /// - 不提供鎖定/解除預約/更新倉位等操作
    /// </summary>
    public sealed partial class ShowMaterialInformationViewModel : ObservableObject
    {
        public ShowMaterialInformationViewModel()
        {
            Kind = MaterialKind.None;
            DetailViewModel = new EmptyMaterialDetailViewModel();
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(KindText))]
        private MaterialKind kind;

        [ObservableProperty]
        private object? detailViewModel;

        /// <summary>
        /// 資訊視窗預期為 null（不顯示倉位/操作列）
        /// </summary>
        [ObservableProperty]
        private string? slotCode;

        /// <summary>
        /// 右側時間軸（共用）
        /// </summary>
        public ObservableCollection<TimelineItemViewModel> Timeline { get; } = new();

        public string KindText => Kind switch
        {
            MaterialKind.Electrode => "電極",
            MaterialKind.Workpiece => "工件",
            MaterialKind.Probe => "探針",
            _ => "物料"
        };

        // 可選：提供一個 Close 命令（若你的視窗需要）
        [RelayCommand]
        private void Close(object? window)
        {
            if (window is System.Windows.Window w) w.Close();
        }

        /// <summary>
        /// 供 WindowService 填資料用（你也可以不用這個方法，仍用你既有的 _infoVm.Kind/_infoVm.DetailViewModel 直接指定）
        /// </summary>
        public void SetTimeline(IEnumerable<TimelineItemModel> timeline)
        {
            Timeline.Clear();
            foreach (var t in timeline)
            {
                Timeline.Add(new TimelineItemViewModel
                {
                    Text = t.Text,
                    Time = t.Time,
                    Status = t.Status
                });
            }
        }
    }
}
