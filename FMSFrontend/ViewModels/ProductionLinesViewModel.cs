using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging; // ← 新增
using CommunityToolkit.Mvvm.Messaging.Messages; // ← 新增：Message 型別
using FMSFrontend.Controls;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Helpers;
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
using System.Reflection;
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
        public readonly IMachinesService _MachinesService;
        public readonly IWorksheetsService _worksheetsService;

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
            IHttpService httpService,
            IStorageService storageService,
            IElectrodeService electrodeService,
            IWorkpieceService workpieceService,
            IProbeService probeService,
            IMachinesService machinesService,
            IWorksheetsService worksheetsService,
            StorageStore storageStore,
            MachineStore machineStore,
            RobotStore robotStore,
            StorageLiveUpdater storageLiveUpdater,
            MachineLiveUpdater machineLiveUpdater)
        {
            _windowService = windowService;
            _httpService = httpService;


            _StorageService = storageService;
            _ElectrodeService = electrodeService;
            _WorkpieceService = workpieceService;
            _MachinesService = machinesService;
            _worksheetsService = worksheetsService;
            _ProbeService = probeService;

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
        public Task OpenMaterialInformationBySerialAsync(string serial, MaterialType kind)
        {
            Slot slot = new Slot
            {
                Serial = serial,
                Kind = kind,
                SlotCode = "",
                StorageRestriction = false
            };
            return OpenMaterialAsync(slot, openMode: MaterialOpenMode.Information);
        }
        // 你可以用 enum 把「從哪裡開」的語意釘死，避免未來又走錯視窗
        public enum MaterialOpenMode
        {
            Information,   // Robot/搜尋/任何非倉位情境：用 MaterialInformation window
            SlotWindow     // 倉位格子點擊：用有倉位的 window
        }

        public async Task OpenMaterialAsync(Slot slot, MaterialOpenMode openMode)
        {
            if (string.IsNullOrEmpty(slot.Serial))
            {
                if (openMode == MaterialOpenMode.Information)
                {
                    _windowService.ShowMessage(LanguageManager.GetString("ProductionLines_Message_NoMaterial", "無物料"));
                    return;
                }

                _windowService.ShowMaterialEmpty(slot); // 倉位才顯示空視窗
                return;
            }
            try
            {
                // 取消前一次仍在執行的更新,逾時設定1秒

                if (slot.Kind == MaterialType.Electrode || slot.Kind == MaterialType.Probe || slot.Kind == MaterialType.None)
                {
                    List<ElectrodeDto>? es = await _ElectrodeService.DB_GetElectrodesByTagSerialAsync(slot.Serial);
                    if (es != null) //檢查是否為電極
                    {
                        var e = es.FirstOrDefault();
                        if (e != null)
                        {
                            List<EleTimelineDto>? eleTimelineDto = await _ElectrodeService.DB_GetElectrodeTimelineByIdAsync(e._id);
                            // eleTimelineDto = null;
                            var timelineModels = eleTimelineDto?.Select(MapElectrodeTimeline).ToList() ?? new List<TimelineItemModel>();

                            var model = MapElectrode(e, slot);

                            if (openMode == MaterialOpenMode.Information)
                                _windowService.ShowMaterialInformation(model, timelineModels);
                            else
                                _windowService.ShowElectrode(model, timelineModels, slot.SlotCode);

                            return;
                            //   _windowService.ShowElectrode(MapElectrode(e, slot), timelineModels);
                        }
                        else
                        {
                            _windowService.ShowMaterialEmpty(slot);
                        }
                    }
                    else //檢查是否為探針
                    {
                        ProbeDto? probe = await _ProbeService.DB_GetProbeByTagSerialAsync(slot.Serial);
                        if (probe != null)
                        {
                            var model = MapProbe(probe, slot);

                            if (openMode == MaterialOpenMode.Information)
                                _windowService.ShowMaterialInformation(model, Enumerable.Empty<TimelineItemModel>());
                            else
                                _windowService.ShowElectrode(model, Enumerable.Empty<TimelineItemModel>(), slot.SlotCode);

                            return;

                            //  _windowService.ShowElectrode(MapProbe(prrobe, slot), new List<TimelineItemModel>(), slot.SlotCode);
                        }
                        else // 皆非 則顯示空資料
                        {
                            _windowService.ShowMaterialEmpty(slot);
                        }
                    }
                }
                if (slot.Kind == MaterialType.None || slot.Kind == MaterialType.Workpiece)
                {
                    WorkpieceDto? wp = await _WorkpieceService.GetWorkpieceByTagSerialAsync(slot.Serial);
                    if (wp != null) //檢查是否為工件
                    {
                        List<WpTimelineDto>? wpTimelineDto = await _WorkpieceService.GetWorkpieceTimelineByWorkpieceIdAsync(wp._id);
                        var wpTimelineModels = wpTimelineDto?.Select(MapWorkpieceTimeline).ToList() ?? new List<TimelineItemModel>();
                        //  _windowService.ShowWorkpiece(MapWorkpiece(wp, slot), wpTimelineModels);

                        var model = MapWorkpiece(wp, slot);

                        if (openMode == MaterialOpenMode.Information)
                            _windowService.ShowMaterialInformation(model, wpTimelineModels);
                        else
                            _windowService.ShowWorkpiece(model, wpTimelineModels, slot.SlotCode);

                        return;
                    }
                    else // 皆非 則顯示空資料
                    {
                        _windowService.ShowMaterialEmpty(slot);
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
                SerialCode = db.tagSerial,
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
                WorkCommand = dto?.workCommand ?? "",
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
        //public void OpenMachineWindow(object? machine)
        //{
        //    //if (machine is MachineCardViewModel)
        //    //{
        //    //    MachineCardViewModel m = (MachineCardViewModel)machine;
        //    //    var vm = new ShowMachineWindowViewModel(machine, this);
        //    //    var win = new ShowMachineWindow { DataContext = vm };
        //    //    var owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
        //    //    if (owner != null) win.Owner = owner;
        //    //    win.ShowDialog();
        //    //}
        //}
        public void OpenMachineWindow(MachineCardViewModel card)
        {
            // 統一交給 WindowService 管理開窗
            _windowService.ShowMachineWindow(card);
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
        private void OpenRobotWindow()
        {
            _windowService.ShowRobotWindow();
        }


        // 你原本就有 SelectedSlot / 或 Current slot，這裡用 IHasMaterial 代表
        public IHasMaterial? SelectedSlot { get; set; }

        // 顯示在按鈕上的字：電極：WRP20250512 / 工件：ABC-001（可依需求調整）
        public string CurrentMaterialLabel
        {
            get
            {
                var m = SelectedSlot?.Material;
                if (m == null) return LanguageManager.GetString("ProductionLines_Label_None", "—");
                return m.Kind == MaterialKind.Electrode
                    ? string.Format(
                        LanguageManager.GetString("ProductionLines_Label_Electrode", "電極：{0}"),
                        m.Electrode?.No ?? m.Electrode?.Name ?? LanguageManager.GetString("ProductionLines_Label_None", "—"))
                    : string.Format(
                        LanguageManager.GetString("ProductionLines_Label_Workpiece", "工件：{0}"),
                        m.Workpiece?.No ?? m.Workpiece?.Name ?? LanguageManager.GetString("ProductionLines_Label_None", "—"));
            }
        }

        // 讓 UI 能更新文字（當 SelectedSlot/Material 改變時請 Raise）
        public void NotifyCurrentMaterialChanged()
        {
            OnPropertyChanged(nameof(CurrentMaterialLabel));
        }

        [RelayCommand]
        private async Task ShowCurrentMaterialInfo()
        {
            var robotModel = this.RobotModel;

            MaterialType type = robotModel.MaterialKind switch
            {
                "Electrode" => MaterialType.Electrode,
                "Probe" => MaterialType.Probe,
                "Workpiece" => MaterialType.Workpiece,
                _ => MaterialType.None
            };

            await OpenMaterialInformationBySerialAsync(robotModel.OnDeckObjSerial, type);
        }
    }
}

