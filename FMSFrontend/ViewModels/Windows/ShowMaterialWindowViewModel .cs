using CommunityToolkit.Mvvm.ComponentModel;
using FMSFrontend.ViewModels;
using FMSFrontend.ViewModels.Production;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FMSFrontend.ViewModels.ElectrodeDetailViewModel;
using CommunityToolkit.Mvvm.Input;

namespace FMSFrontend.ViewModels.Windows
{
    public enum MaterialKind { None, Electrode, Workpiece }

    public sealed partial class ShowMaterialWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(KindText))]
        private MaterialKind kind;

        // 左側資訊區：會是 ElectrodeDetailViewModel 或 WorkpieceDetailViewModel 其中之一
        [ObservableProperty]
        private object? detailViewModel;

        [ObservableProperty]
        private string? slotCode;

        // 右側時間軸（兩種都共用）
        public ObservableCollection<TimelineItemViewModel> Timeline { get; } = new();

        // 鎖定狀態（供 ElectrodeDetailView 使用）
        [ObservableProperty]
        private bool isLocked;

        // 1 電極再使用（綠）
        [RelayCommand]
        private void ReuseElectrode()
        {
            // TODO: 實作再使用行為
        }

        // 2 電極再使用（藍）
        [RelayCommand]
        private void ReuseElectrodeAlt()
        {
            // TODO: 實作另一種再使用行為
        }

        // 3 修改補償值（紫）
        [RelayCommand]
        private void EditCompensation()
        {
            // TODO: 開啟補償值編輯流程
        }

        // 4 解除異常（橘）
        [RelayCommand]
        private void ClearAbnormal()
        {
            // TODO: 清除異常
        }

        // 5 解除碰撞（青藍）
        [RelayCommand]
        private void ClearCollision()
        {
            // TODO: 清除碰撞狀態
        }

        // 6 解除預約（藍綠）
        [RelayCommand]
        private void ClearReservation()
        {
            // TODO: 解除預約
        }

        public string KindText => Kind switch
        {
            MaterialKind.Electrode => "電極",
            MaterialKind.Workpiece => "工件",
            _ => "物料"
        };

        // 空畫面
        public ShowMaterialWindowViewModel()
        {
            Kind = MaterialKind.None;
            DetailViewModel = new EmptyMaterialDetailViewModel();
        }

        // ★ 由點擊的格位載入資料
        public void LoadFrom(SlotViewModel slot)
        {
            ApplyMaterial(slot.Material, slot.Kind, slot.SlotCode);
        }

        public void LoadFrom(StorageSlotViewModel slot)
        {
            ApplyMaterial(slot.Material, slot.Material?.Kind ?? MaterialKind.None, slot.Material?.Electrode?.Name ?? slot.Material?.Workpiece?.Name ?? slot.Status);
        }

        /// <summary>
        /// 把 MaterialRef 套用到視窗 VM
        /// </summary>
        private void ApplyMaterial(MaterialRef material, MaterialKind kind, string slotCode)
        {
            Kind = kind;
            SlotCode = slotCode;

            if (material == null)
            {
                DetailViewModel = new EmptyMaterialDetailViewModel();
                return;
            }

            DetailViewModel = kind switch
            {
                MaterialKind.Electrode => new ElectrodeDetailViewModel(material.Electrode),
                MaterialKind.Workpiece => new WorkpieceDetailViewModel(material.Workpiece),
                _ => new EmptyMaterialDetailViewModel()
            };

            Timeline.Clear();
            if (material.Timeline != null)
            {
                foreach (var t in material.Timeline)
                    Timeline.Add(new TimelineItemViewModel { Text = t.Text, Time = t.Time, Status = t.Status });
            }
        }

        // Demo：先看得到畫面
        public ShowMaterialWindowViewModel(object detailVm, IEnumerable<TimelineItemViewModel> tl, MaterialKind kind)
        {
            Kind = kind;
            DetailViewModel = detailVm;
            foreach (var t in tl) Timeline.Add(t);
        }

        // 之後你再用這兩個「正式」建構子
        public ShowMaterialWindowViewModel(ElectrodeDetailViewModel vm, IEnumerable<TimelineItemViewModel> tl)
        {
            Kind = MaterialKind.Electrode;
            DetailViewModel = vm;
            foreach (var t in tl) Timeline.Add(t);
        }
        public ShowMaterialWindowViewModel(WorkpieceDetailViewModel vm, IEnumerable<TimelineItemViewModel> tl)
        {
            Kind = MaterialKind.Workpiece;
            DetailViewModel = vm;
            foreach (var t in tl) Timeline.Add(t);
        }
    }
}
