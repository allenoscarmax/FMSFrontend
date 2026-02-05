using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Interfaces;
using FMSFrontend.Services;
using FMSFrontend.ViewModels;
using FMSFrontend.ViewModels.Production;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;


namespace FMSFrontend.ViewModels.Windows
{
    public enum MaterialKind { None, Electrode, Workpiece, Probe }

    public sealed partial class ShowMaterialWindowViewModel : ObservableObject
    {
        private readonly IWindowService _windowService;

        // === Services ===
        private readonly IElectrodeService _electrodeService;
        private readonly IWorkpieceService _workpieceService;
        private readonly IProbeService _probeService;
        private readonly IStorageService _storageService;
        private readonly IAuthorizationService _authorizationService;

        // 空畫面
        public ShowMaterialWindowViewModel(IWindowService windowService,
        IElectrodeService electrodeService,
        IWorkpieceService workpieceService,
        IProbeService probeService,
        IStorageService storageService,
        IAuthorizationService authorizationService)
        {
            Kind = MaterialKind.None;

            DetailViewModel = new EmptyMaterialDetailViewModel();

            _electrodeService = electrodeService;
            _workpieceService = workpieceService;
            _probeService = probeService;
            _storageService = storageService;
            _windowService = windowService;
            _authorizationService = authorizationService;
        }

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
        private bool CanElectrodeClearBooked()
        {
            if (DetailViewModel is ElectrodeDetailViewModel)
            {
                ElectrodeDetailViewModel e = (ElectrodeDetailViewModel)DetailViewModel;
                return e != null && e.Status == "Booked";
            }
            return false;
        }
        [RelayCommand(CanExecute = nameof(CanElectrodeClearBooked))]
        private async Task ElectrodeClearBooked()
        {
            if (DetailViewModel is ElectrodeDetailViewModel)
            {
                ElectrodeDetailViewModel e = (ElectrodeDetailViewModel)DetailViewModel;
                try
                {
                    bool ok = false;
                    if (!_authorizationService.RequireLoginAndWriteOperation(37))
                    {
                        IsLocked = !IsLocked; //還原勾選狀態
                        return;
                    }
                    if (e.TagSerial == null) return;
                    if (e.ElectrodeName.Contains("Probe"))
                    {
                        //取得探針資料
                        var dto = await _probeService.DB_GetProbeByTagSerialAsync(e.TagSerial);
                        if (dto == null) return;
                        
                        //取消預約
                        dto.state = "Verified";
                        ok = await _probeService.DB_UpdateProbeDataAsync(dto);

                        // 回傳更新UI
                        ((ElectrodeDetailViewModel)DetailViewModel).Status = "Verified";
                    }
                    else
                    {
                        //取得電極資料
                        var dtos = await _electrodeService.DB_GetElectrodesByTagSerialAsync(e.TagSerial);
                        if (dtos == null) return;
                        var dto = dtos.FirstOrDefault();
                        if (dto == null) return;
                        
                        //取消預約
                        dto.state = "Verified";
                        ok = await _electrodeService.DB_UpdateElectrodeDataAsync(dto);

                        // 回傳更新
                        ((ElectrodeDetailViewModel)DetailViewModel).Status = "Verified";
                    }
                    if (!ok)
                        _windowService.ShowMessage("回傳失敗");
                }
                catch (Exception ex)
                {
                    _windowService.ShowMessage($"例外狀況: {ex.Message}");
                }
            }
        }
        private bool CanWorkpieceClearBooked()
        {
            if (DetailViewModel is WorkpieceDetailViewModel)
            {
                WorkpieceDetailViewModel w = (WorkpieceDetailViewModel)DetailViewModel;
                return w != null && w.Status == "Booked";
            }
            return false;
        }
        [RelayCommand(CanExecute = nameof(CanWorkpieceClearBooked))]
        private async Task WorkpieceClearBooked()
        {
            if (DetailViewModel is WorkpieceDetailViewModel)
            {
                WorkpieceDetailViewModel w = (WorkpieceDetailViewModel)DetailViewModel;
                try
                {
                    if (!_authorizationService.RequireLoginAndWriteOperation(38))
                    {
                        IsLocked = !IsLocked; //還原勾選狀態
                        return;
                    }

                    //取得工件資料
                    bool ok = false;
                    var dto = await _workpieceService.GetWorkpieceByTagSerialAsync(w.SerialCode);
                    if (dto == null) return;

                    //取消預約
                    dto.status = "Verified";
                    ok = await _workpieceService.UpdateWorkpieceDataAsync(dto);
                    if (!ok)
                    {
                        _windowService.ShowMessage("回傳失敗");
                        return;
                    }
                    // 回傳更新UI
                    ((WorkpieceDetailViewModel)DetailViewModel).Status = "Verified";
                }
                catch (Exception ex)
                {
                    _windowService.ShowMessage($"例外狀況: {ex.Message}");
                }
            }
        }


        [RelayCommand]
        private async Task UpdataElectrode()  //電極鎖定
        {
            if (DetailViewModel is ElectrodeDetailViewModel)
            {
                ElectrodeDetailViewModel e = (ElectrodeDetailViewModel)DetailViewModel;
                try
                {
                    bool ok;
                    if (!_authorizationService.RequireLoginAndWriteOperation(12, " " + SlotCode + ": " + IsLocked.ToString()))
                    {
                        IsLocked = !IsLocked; //還原勾選狀態
                        return;
                    }

                    if (e.ElectrodeName.Contains("Probe"))
                    {
                        ok = await _probeService.DB_SetProbeRestrictionByTagSerialAsync(e.TagSerial, IsLocked);
                    }
                    else
                        ok = await _electrodeService.DB_SetElectrodeRestrictionByTagSerialAsync(e.TagSerial, IsLocked);
                    if (!ok)
                        _windowService.ShowMessage("回傳失敗");
                }
                catch (Exception ex)
                {
                    _windowService.ShowMessage($"例外狀況: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private async Task UpdataWorkpiece() //工件鎖定
        {
            if (DetailViewModel is WorkpieceDetailViewModel)
            {
                WorkpieceDetailViewModel w = (WorkpieceDetailViewModel)DetailViewModel;
                try
                {
                    if (!_authorizationService.RequireLoginAndWriteOperation(13, " " + SlotCode + ": " + IsLocked.ToString()))
                    {
                        IsLocked = !IsLocked; //還原勾選狀態
                        return;
                    }
                    var ok = await _workpieceService.SetWorkpieceRestrictionByTagSerialAsync(w.SerialCode, IsLocked);
                    if (!ok)
                        _windowService.ShowMessage("回傳失敗");
                }
                catch (Exception ex)
                {
                    _windowService.ShowMessage($"例外狀況: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private async Task UpdataStorage() //電極庫禁用
        {
            if (SlotCode == null) return;
            string[] code = SlotCode.Split(":");
            if (code.Length >= 5)
            {
                try
                {
                    if (!_authorizationService.RequireLoginAndWriteOperation(14, " " + SlotCode + ": " + IsDisabled.ToString()))
                    {
                        IsDisabled = !IsDisabled; //還原勾選狀態
                        return;
                    }
                    string storageName = code[0];
                    string storageNumber = code[1];
                    int region = int.Parse(code[2]);
                    int column = int.Parse(code[3]);
                    int row = int.Parse(code[4]);
                    var ok = await _storageService.SetRestrictionByLocationAsync(storageName, storageNumber, region, column, row, IsDisabled);
                    if (!ok)
                        _windowService.ShowMessage("回傳失敗");
                }
                catch (Exception ex)
                {
                    _windowService.ShowMessage($"例外狀況: {ex.Message}");
                }
            }
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CancelBookStorageCommand))]
        private string? currentStorageState; // Booked / Vacant / Occupy


        private bool CanCancelBookStorage()
            => string.Equals(CurrentStorageState, StorageStateEnum.Booked.ToString(), StringComparison.OrdinalIgnoreCase)
               && !string.IsNullOrWhiteSpace(SlotCode);

        [RelayCommand(CanExecute = nameof(CanCancelBookStorage))]
        private async Task CancelBookStorage(CancellationToken ct)
        {
            // 二次防呆（避免狀態不同步）
            if (!string.Equals(CurrentStorageState, StorageStateEnum.Booked.ToString(), StringComparison.OrdinalIgnoreCase))
                return;

            if (string.IsNullOrWhiteSpace(SlotCode)) return;
            var code = SlotCode.Split(":");
            if (code.Length < 5) return;

            try
            {
                string storageName = code[0];
                string storageNumber = code[1];
                int region = int.Parse(code[2]);
                int column = int.Parse(code[3]);
                int row = int.Parse(code[4]);

                // 用你現有的 API：把 restriction 跟 state 分開思考
                // 解除預約 = state 改 Vacant
                // 這裡需要 StorageDto（建議帶 _id 最穩）
                if (!_authorizationService.RequireLoginAndWriteOperation(15, " " + SlotCode))
                    return;
                var ss = await _storageService.GetStorageByLocationAsync(storageName, storageNumber, region, column, row);
                if (ss != null)
                {
                    ss.state = StorageStateEnum.Vacant.ToString();
                    var ok = await _storageService.UpdateStorageDataAsync(ss, ct);
                    if (ok)
                    {
                        CurrentStorageState = StorageStateEnum.Vacant.ToString(); // 立刻讓按鈕 disabled
                    }
                    else
                        _windowService.ShowMessage("回傳失敗");
                }
            }
            catch (Exception ex)
            {
                _windowService.ShowMessage($"例外狀況: {ex.Message}");
            }
        }

        public string KindText => Kind switch
        {
            MaterialKind.Electrode => "電極",
            MaterialKind.Workpiece => "工件",
            _ => "物料"
        };

    }
}
