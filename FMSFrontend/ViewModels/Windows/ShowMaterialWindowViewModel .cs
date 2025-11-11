using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Features.Services;
using FMSFrontend.Interfaces;
using FMSFrontend.Services;
using FMSFrontend.ViewModels;
using FMSFrontend.ViewModels.Production;
using OSCARMAXFMS_V3.DBmodels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static FMSFrontend.ViewModels.ElectrodeDetailViewModel;

namespace FMSFrontend.ViewModels.Windows
{
    public enum MaterialKind { None, Electrode, Workpiece, Probe }

    public sealed partial class ShowMaterialWindowViewModel : ObservableObject
    {
       // private readonly IWindowService _windowService;
        private readonly IHttpService _httpService;
        // === Services ===
        private readonly IElectrodeService _ElectrodeService;
        private readonly IWorkpieceService _WorkpieceService;
        
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

        [ObservableProperty]
        private bool isDisabled;

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
        }
        [RelayCommand]
        private async Task UpdataElectrode()  //電極鎖定
        {
            if (DetailViewModel is ElectrodeDetailViewModel )
            {
                ElectrodeDetailViewModel e = (ElectrodeDetailViewModel)DetailViewModel;

                // 使用匿名物件只傳需要的欄位，避免把整個 DB model 序列化
                var path = $"DB_SetWorkpieceRestrictionbyTagSerial /{e.TagSerial}/{IsLocked}";
                var ok = await _httpService.SendPutAsync(path, "");
            }
        }

        [RelayCommand]
        private async Task  UpdataWorkpiece() //工件鎖定
        {
            if (DetailViewModel is WorkpieceDetailViewModel)
            {
                WorkpieceDetailViewModel w = (WorkpieceDetailViewModel)DetailViewModel;

                // 同樣只傳必要欄位
                var payload = new { _id = w.Id, restriction = IsLocked };
                var path = $"DB_SetWorkpieceRestrictionbyTagSerial /{w.SerialCode}/{IsLocked}";
                var ok = await _httpService.SendPutAsync(path, "");
            }
        }

        [RelayCommand]
        private async Task UpdataStorage() //電極庫鎖定
        {
            bool ok;
            if (DetailViewModel is WorkpieceDetailViewModel)
            {
                WorkpieceDetailViewModel w = (WorkpieceDetailViewModel)DetailViewModel;
                var wpayload = new { _id = w.StorageId, restriction = IsDisabled };
                ok = await _httpService.SendPutAsync("Storage/DB_UpdateStorageData", wpayload);
                return;
            }
            else if (DetailViewModel is ElectrodeDetailViewModel)
            {
                ElectrodeDetailViewModel e = (ElectrodeDetailViewModel)DetailViewModel;
                var epayload = new { _id = e.StorageId, restriction = IsDisabled };
                ok = await _httpService.SendPutAsync("Storage/DB_UpdateStorageData", epayload);
                return;
            }
        }
        [RelayCommand]
        private async Task CancelBookStorage() //解除預約
        {
            bool ok;
            if (DetailViewModel is WorkpieceDetailViewModel)
            {
                WorkpieceDetailViewModel w = (WorkpieceDetailViewModel)DetailViewModel;
                var wpayload = new { _id = w.StorageId, state = "Vacant" };
                ok = await _httpService.SendPutAsync("Storage/DB_UpdateStorageData", wpayload);
                return;
            }
            else if (DetailViewModel is ElectrodeDetailViewModel)
            {
                ElectrodeDetailViewModel e = (ElectrodeDetailViewModel)DetailViewModel;
                var epayload = new { _id = e.StorageId, state = "Vacant" };
                ok = await _httpService.SendPutAsync("Storage/DB_UpdateStorageData", epayload);
                return;
            }
        }

        public string KindText => Kind switch
        {
            MaterialKind.Electrode => "電極",
            MaterialKind.Workpiece => "工件",
            _ => "物料"
        };

        // 空畫面
        public ShowMaterialWindowViewModel(IHttpService httpService)
        {
            Kind = MaterialKind.None;
            DetailViewModel = new EmptyMaterialDetailViewModel();
            _httpService = httpService;
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
        /*
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
        */
    }
}
