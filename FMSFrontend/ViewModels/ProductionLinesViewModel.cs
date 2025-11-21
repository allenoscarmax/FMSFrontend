using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging; // ← 新增
using CommunityToolkit.Mvvm.Messaging.Messages; // ← 新增：Message 型別
using FMSFrontend.Controls;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Services.FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Interfaces;
using FMSFrontend.Models;
using FMSFrontend.Services; // ← 新增
using FMSFrontend.ViewModels.Production;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views;
using FMSFrontend.Views.Windows;
using IniFile;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using static FMSFrontend.ViewModels.ElectrodeDetailViewModel;

namespace FMSFrontend.ViewModels
{
    public partial class ProductionLinesViewModel : ObservableObject
    {
        public readonly IWindowService _windowService;
        private readonly IHttpService _httpService;

        // === Services ===
        private readonly IStorageService _StorageService;
        private readonly IElectrodeService _ElectrodeService;
        private readonly IWorkpieceService _WorkpieceService;
        private readonly IProbeService _ProbeService;
        private readonly IMachinesService _MachinesService;
        public readonly IWorksheetsService _worksheetsService;

        private CancellationTokenSource? _currentUpdateCts; // 取消目前更新的 CancellationTokenSource

        // === Singleton ===
        private readonly StorageStore _storageStore;
        public StorageGroupModel StorageGroup => _storageStore.StorageGroup;

        private readonly MachineStore _machineStore;

        private readonly RobotStore _robotStore;
        public Robot RobotModel => _robotStore.Robot;

        // ==LiveUpdater===
        public StorageLiveUpdater _storageUpdater;
        public MachineLiveUpdater _machineLiveUpdater;

        private object? _currentStorageView;
        public object? CurrentStorageView
        {
            get => _currentStorageView;
            set => SetProperty(ref _currentStorageView, value);
        }

        private object? _currentWorkingZoneView;
        public object? CurrentWorkingZoneView
        {
            get => _currentWorkingZoneView;
            set => SetProperty(ref _currentWorkingZoneView, value);
        }
        
        public ProductionLinesViewModel(IWindowService windowService, 
           // IHttpService httpService,
            StorageStore storageStore, 
            MachineStore machineStore, 
            RobotStore robotStore,
            StorageLiveUpdater storageLiveUpdater, 
            MachineLiveUpdater machineLiveUpdater) 
        {
            _windowService = windowService;
            _httpService = new HttpService();
            _StorageService = new StorageService(_httpService);
            _ElectrodeService = new ElectrodeService(_httpService);
            _WorkpieceService = new WorkpieceService(_httpService);
            _MachinesService = new MachinesService(_httpService);
            _worksheetsService = new WorksheetsService(_httpService);
            _ProbeService = new ProbeService(_httpService);

            _storageStore = storageStore;
            _machineStore = machineStore;
            _robotStore = robotStore;

            _storageUpdater = storageLiveUpdater;
            _machineLiveUpdater = machineLiveUpdater;

            INIFile ini = new INIFile(AppDomain.CurrentDomain.BaseDirectory + "\\Basesitting.ini");
            bool b = ini.Read("Prarm", "IsStorageOverviewControl") == "True";
            if (b)
                ShowOverview();
            else
                ShowDetail("0");
            ShowMachineOverview();

        }
        public void OnPageActivated()
        {
            _storageUpdater.Start();
            _machineLiveUpdater.Start();
        }
        public void OnPageDeactivated()
        {
            _storageUpdater.Stop();
            _machineLiveUpdater.Stop();
        }

        public class RobotStatusViewModel
        {
            public MachineStatus Status { get; set; }
        }
        public enum MachineStatus
        {
            Idle,
            Running,
            Alarm
        }
        
        //開啟工件或電極視窗
        public void OpenMaterialbySerial(string serial, MaterialType kind)
        {
            Slot slot = new Slot
            {
                Serial = serial,
                Kind = kind,
                SlotCode = "",
                StorageRestriction = false
            };
            OpenMaterial(slot);
        }

        public async void OpenMaterial(Slot slot)
        {
            if (string.IsNullOrEmpty(slot.Serial))
            {
                _windowService.ShowMaterialEmpty(_httpService, _ElectrodeService, _WorkpieceService, _ProbeService, _StorageService);
                return;
            }
            try
            {
                // 取消前一次仍在執行的更新,逾時設定1秒
                _currentUpdateCts?.Cancel();
                _currentUpdateCts?.Dispose();
                _currentUpdateCts = new CancellationTokenSource();
                _currentUpdateCts.CancelAfter(TimeSpan.FromMilliseconds(100));

                var ct = _currentUpdateCts.Token;
                if (slot.Kind == MaterialType.Electrode || slot.Kind == MaterialType.Probe || slot.Kind == MaterialType.None)
                {
                    List<ElectrodeDto>? es = await _ElectrodeService.DB_GetElectrodesByTagSerialAsync(slot.Serial, ct);
                    if (es != null) //檢查是否為電極
                    {
                        var e = es.FirstOrDefault();
                        if (e != null)
                        {
                            List<EleTimelineDto>? eleTimelineDto = await _ElectrodeService.DB_GetElectrodeTimelineByIdAsync(e._id, ct);
                            // eleTimelineDto = null;
                            var timelineModels = eleTimelineDto?.Select(MapElectrodeTimeline).ToList() ?? new List<TimelineItemModel>();
                            _windowService.ShowElectrode(MapElectrode(e, slot), timelineModels, _httpService,
                                _ElectrodeService, _WorkpieceService, _ProbeService, _StorageService, slot.SlotCode);
                        }
                        else
                        {
                            _windowService.ShowMaterialEmpty(_httpService, _ElectrodeService, _WorkpieceService, _ProbeService, _StorageService);
                        }
                    }
                    else //檢查是否為探針
                    {
                        ProbeDto? prrobe = await _ProbeService.DB_GetProbeByTagSerialAsync(slot.Serial, ct);
                        if (prrobe != null)
                            _windowService.ShowElectrode(MapProbe(prrobe, slot), new List<TimelineItemModel>(), _httpService,
                                 _ElectrodeService, _WorkpieceService, _ProbeService, _StorageService,
                                 slot.SlotCode);
                        else // 皆非 則顯示空資料
                        {
                            _windowService.ShowMaterialEmpty(_httpService, _ElectrodeService, _WorkpieceService, _ProbeService, _StorageService);
                        }
                    }
                }
                if (slot.Kind == MaterialType.None || slot.Kind == MaterialType.Workpiece)
                {
                    WorkpieceDto? wp = await _WorkpieceService.GetWorkpieceByTagSerialAsync(slot.Serial);
                    if (wp != null) //檢查是否為工件
                    {
                        List<WpTimelineDto>? wpTimelineDto = await _WorkpieceService.GetWorkpieceTimelineByWorkpieceIdAsync(wp._id, ct);
                        var wpTimelineModels = wpTimelineDto?.Select(MapWorkpieceTimeline).ToList() ?? new List<TimelineItemModel>();
                        _windowService.ShowWorkpiece(MapWorkpiece(wp, slot), wpTimelineModels, _httpService,
                             _ElectrodeService, _WorkpieceService, _ProbeService, _StorageService, slot.SlotCode);
                    }
                    else // 皆非 則顯示空資料
                    {
                        _windowService.ShowMaterialEmpty(_httpService, _ElectrodeService, _WorkpieceService, _ProbeService, _StorageService);
                    }
                }
            }
            catch { }
        }
        private static ElectrodeModel MapElectrode(ElectrodeDto db, Slot slot)
        {
            return new ElectrodeModel
            {
                // 以 API 為主，缺的用舊值補
                Id = db._id,
                Name = db.electrodeName ?? "",
                No = "", // DB 未提供 → 沿用舊值
                Type = db.electrodeType ?? "",
                Status = db.state ?? "",
                TagSerial = db.tagSerial ?? "",
                MaxDischargeCount = db.lifeTimes.ToString() ?? "",
                Compensation = db.offset ?? "",
                ProcessedCount = db.useTimes?.ToString() ?? "",
                ElecRestriction = db.restriction ?? false,
                StorageRestriction = slot.StorageRestriction
            };
        }
        private static ElectrodeModel MapProbe(ProbeDto? db, Slot slot)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));
            return new ElectrodeModel
            {
                // 以 API 為主，缺的用舊值補
                Id = db._id,
                Name = db.probeName ?? "",
                No = "", // DB 未提供 → 沿用舊值
                Type = db.probeType ?? "",
                Status = db.state ?? "",
                TagSerial = db.tagSerial ?? "",
                MaxDischargeCount = "",
                Compensation = "",
                ProcessedCount = "",
                ElecRestriction = db.restriction ?? false,
                StorageRestriction = slot.StorageRestriction
            };
        }

        private static WorkpieceModel MapWorkpiece(WorkpieceDto db, Slot slot)
        {
            return new WorkpieceModel
            {
                // 以 API 為主，缺的用舊值補
                Id = db._id,
                JigSerial = "",
                Name = db.workpieceName ?? "",
                No = "",
                WorkType = "",
                PartNo = "",
                OrderNo = db.worksheetNumber ?? "",
                ClampNo = "",
                Status = db.status ?? "",
                BatchNo = "",
                PartName = "",
                SerialCode = "",
                RouteNo = "",
                WorkRestriction = db?.restriction ?? false,
                StorageRestriction = slot.StorageRestriction
            };
        }

        private static TimelineItemModel MapElectrodeTimeline(EleTimelineDto dto)
        {
            // 請根據 TimelineItemModel 與 EleTimelineDto 的實際屬性做對應
            return new TimelineItemModel
            {
                Time = dto?.timeStamp ?? DateTime.MinValue,
                WorkCommand = dto?.workCommand?? "",
                Status = "", //待定義
            };
        }
        private static TimelineItemModel MapWorkpieceTimeline(WpTimelineDto dto)
        {
            // 請根據 TimelineItemModel 與 WpTimelineDto 的實際屬性做對應
            return new TimelineItemModel
            {
                Time = dto?.timeStampe ?? DateTime.MinValue,
                WorkCommand = dto?.workCommand ?? "",
                Status = "", //待定義
            };
        }
        private static TimelineItemModel MapProbeTimeline(WpTimelineDto? dto)
        {
            // 請根據 TimelineItemModel 與 WpTimelineDto 的實際屬性做對應
            return new TimelineItemModel
            {
                Time = dto?.timeStampe ?? DateTime.MinValue,
                WorkCommand = dto?.workCommand ?? "",
                Status = "", //待定義
            };
        }
        //設備資訊
        public void OpenMachineWindow(object? machine)
        {
            if (machine is MachineCardViewModel)
            {
                MachineCardViewModel m = (MachineCardViewModel)machine;
                var vm = new ShowMachineWindowViewModel(machine, this);
                var win = new ShowMachineWindow { DataContext = vm };
                var owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                if (owner != null) win.Owner = owner;
                win.ShowDialog();
            }
        }

        [RelayCommand]
        public void ShowDetail(string storageId) //切換倉儲頁面
        {
            // TODO: 傳入 storageId 給 DetailControl，如果要的話
            var overviewVM = new StorageDetailViewModel(this, _httpService, _storageStore, _storageUpdater); // 傳入自己當 parent
            var overviewView = new StorageDetailControl
            {
                DataContext = overviewVM // 這一步非常重要！
            };
            CurrentStorageView = overviewView;
        }
        [RelayCommand]
        public void ShowOverview() //切換倉儲頁面
        {
            var overviewVM = new StorageOverviewViewModel(this, _httpService, _storageStore); // ← 傳入 httpService
            var overviewView = new StorageOverviewControl
            {
                DataContext = overviewVM // 這一步非常重要！
            };
            CurrentStorageView = overviewView;
        }
        MachineDetailViewModel? machineDetailViewModel;
        MachineOverviewViewModel? machineOverviewViewModel;
        [RelayCommand]
        public void ShowMachineDetail() //切換機器頁面
        {
            machineDetailViewModel = new MachineDetailViewModel(this, _httpService,
                 _ElectrodeService, _WorkpieceService, _ProbeService, _StorageService, _MachinesService,
                 _machineLiveUpdater, _machineStore);
            var overviewView = new MachineDetailControl
            {
                DataContext = machineDetailViewModel // 這一步非常重要！
            };
            CurrentWorkingZoneView = overviewView;
        }
        [RelayCommand]
        public void ShowMachineOverview() //切換機器頁面
        {
            machineOverviewViewModel = new MachineOverviewViewModel(this, _httpService,
                 _ElectrodeService, _WorkpieceService, _ProbeService, _StorageService, _MachinesService,
                 _machineLiveUpdater, _machineStore);
            var overviewView = new MachineOverviewControl
            {
                DataContext = machineOverviewViewModel // 這一步非常重要！
            };
            CurrentWorkingZoneView = overviewView;
        }
        [RelayCommand]
        private void OpenRobotWindow() //開啟機器人視窗
        {
            var win = new ShowRobotWindow(RobotModel);
            var owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            if (owner != null) win.Owner = owner;

            // 需要傳當前選擇的機器人資料時：
            win.ShowDialog();           
        }


        // 你原本就有 SelectedSlot / 或 Current slot，這裡用 IHasMaterial 代表
        public IHasMaterial? SelectedSlot { get; set; }

        // 顯示在按鈕上的字：電極：WRP20250512 / 工件：ABC-001（可依需求調整）
        public string CurrentMaterialLabel
        {
            get
            {
                var m = SelectedSlot?.Material;
                if (m == null) return "—";
                return m.Kind == MaterialKind.Electrode
                    ? $"電極：{(m.Electrode?.No ?? m.Electrode?.Name ?? "—")}"
                    : $"工件：{(m.Workpiece?.No ?? m.Workpiece?.Name ?? "—")}";
            }
        }

        // 讓 UI 能更新文字（當 SelectedSlot/Material 改變時請 Raise）
        public void NotifyCurrentMaterialChanged()
        {
            OnPropertyChanged(nameof(CurrentMaterialLabel));
        }

        [RelayCommand]
        private void ShowCurrentMaterialInfo()
        {
            var RobotModel = this.RobotModel;

            MaterialType type = RobotModel.MaterialKind switch
            {
                "Electrode" => MaterialType.Electrode,
                "Probe" => MaterialType.Electrode,
                "Workpiece" => MaterialType.Workpiece,
                _ => MaterialType.None
            };
            OpenMaterialbySerial(RobotModel.OnDeckObjSerial, type);
        }
    }
}

