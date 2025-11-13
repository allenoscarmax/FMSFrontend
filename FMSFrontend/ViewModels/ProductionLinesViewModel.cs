using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging; // ← 新增
using CommunityToolkit.Mvvm.Messaging.Messages; // ← 新增：Message 型別
using FMSFrontend.Controls;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
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
using OSCARMAXFMS_V3.DBmodels; // ← 反序列化 Electrode.cs / Workpiece.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
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
        private readonly IElectrodeService _ElectrodeService;
        private readonly IWorkpieceService _WorkpieceService;
        private readonly IProbeService _probeService;

        // === Singleton ===
        private readonly StorageStore _storageStore;
        public StorageGroupModel StorageGroup => _storageStore.StorageGroup;
        // ==LiveUpdater===
        public StorageLiveUpdater _storageUpdater;


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

        public ProductionLinesViewModel(IWindowService windowService, IHttpService httpService,
            IElectrodeService electrodeService, IWorkpieceService workpieceService, IProbeService probeService,
            StorageStore storageStore, StorageLiveUpdater storageLiveUpdater) 
        {
            _windowService = windowService;
            _httpService = httpService;
            _probeService = probeService;

            _ElectrodeService = electrodeService;
            _WorkpieceService = workpieceService;
            _storageStore = storageStore;
            _storageUpdater = storageLiveUpdater;

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
        }
        public void OnPageDeactivated()
        {
            _storageUpdater.Stop();
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
        public async void OpenMaterial(Slot slot)
        {
            if (string.IsNullOrEmpty(slot.Serial))
            {
                _windowService.ShowMaterialEmpty(_httpService);
            }
            //try
            //{
            bool NullFlag = false;
            switch (slot.Kind)
            {
                case MaterialType.Electrode:
                case MaterialType.Probe:
                    List<ElectrodeDto>? es = await _ElectrodeService.DB_GetElectrodesByTagSerialAsync(slot.Serial);
                    if (es != null) //檢查是否為電極
                    {
                        var e = es.FirstOrDefault();
                        List<EleTimelineDto>? eleTimelineDto = await _ElectrodeService.DB_GetElectrodeTimelineByIdAsync(e._id);
                       // eleTimelineDto = null;
                        var timelineModels = eleTimelineDto?.Select(MapElectrodeTimeline).ToList() ?? new List<TimelineItemModel>();
                        _windowService.ShowElectrode(MapElectrode(e), timelineModels, _httpService, slot.SlotCode);
                    }
                    else //檢查是否為探針
                    {
                        ProbeDto? prrobe = await _probeService.DB_GetProbeByTagSerialAsync(slot.Serial);
                        if (prrobe != null) 
                            _windowService.ShowElectrode(MapProbe(prrobe), new List<TimelineItemModel>(), _httpService, slot.SlotCode);
                        else // 皆非 則顯示空資料
                            _windowService.ShowMaterialEmpty(_httpService);
                    }
                    break;
                case MaterialType.Workpiece:
                    WorkpieceDto? wp = await _WorkpieceService.GetWorkpieceByTagSerialAsync(slot.Serial);
                    if (wp != null) //檢查是否為工件
                    {
                        List<WpTimelineDto>? wpTimelineDto = await _WorkpieceService.GetWorkpieceTimelineByWorkpieceIdAsync(wp._id);
                        var wpTimelineModels = wpTimelineDto?.Select(MapWorkpieceTimeline).ToList() ?? new List<TimelineItemModel>();
                        _windowService.ShowWorkpiece(MapWorkpiece(wp), wpTimelineModels, _httpService, slot.SlotCode);
                    }
                    else // 皆非 則顯示空資料
                    {
                        _windowService.ShowMaterialEmpty(_httpService);
                    }
                    break;
            }
            /*
            // 1. 資料
            var elecList = await _httpService.GetJsonAsync<List<Electrode>>($"Electrode/DB_GetElectrodesByTagSerial/{tagSerial}");
                var dbElec = elecList?.FirstOrDefault();
                // 映射 DB → View Model（若 API 無部份欄位，用舊值補）
                var elecVm = MapElectrode(dbElec, material.Electrode);
                var id = elecVm.Id;
                elecVm.StorageRestriction = material.Electrode?.StorageRestriction ?? false;
                  // 2. 時間軸
                  var elecTimeline = await _httpService.GetJsonAsync<IEnumerable<TimelineItemModel>>($"Electrode/DB_GetElectrodeTimelinebyId/{id}")
                                       ?? Array.Empty<TimelineItemModel>();
                //3.顯示資料
                _windowService.ShowElectrode(elecVm, elecTimeline, _httpService, slotCode);
            }
            else // Workpiece
            {
                var tagSerial = material.Workpiece?.SerialCode;
                if (string.IsNullOrWhiteSpace(tagSerial))
                {
                    WorkpieceModel wpFallback = material.Workpiece ?? new WorkpieceModel();
                    _windowService.ShowWorkpiece(wpFallback, material.Timeline ?? Array.Empty<TimelineItemModel>(), _httpService, slotCode);
                    return;
                }
                // 1. 資料
                string route = $"Workpiece/DB_GetWorkpieceByTagSerial/{tagSerial}";
                //JsonElement? json = await _httpService.GetJsonAsync<JsonElement>(route, default);
                //Workpiece dbWp = (json.HasValue && json.Value.ValueKind != JsonValueKind.Undefined) ?
                //JsonSerializer.Deserialize<Workpiece>(json.Value.GetRawText()) ?? new Workpiece() :
                //new Workpiece();
                var wpList = await _httpService.GetJsonAsync<List<Workpiece>>(route);
                var dbWp = wpList?.FirstOrDefault();
                var wpVm = MapWorkpiece(dbWp, material.Workpiece);
                wpVm.StorageRestriction = material.Workpiece?.StorageRestriction ?? false;
                var id = wpVm.Id;
                // 2. 時間軸
                var wpTimeline = await _httpService.GetJsonAsync<IEnumerable<TimelineItemModel>>($"Workpiece/DB_GetWorkpieceTimelineByWorkpieceId/{id}")
                                 ?? Array.Empty<TimelineItemModel>();
                //3.顯示資料
               
            }
            */

            //}catch{}
        }

        private static ElectrodeModel MapElectrode(ElectrodeDto db)
        {
            return new ElectrodeModel
            {
                // 以 API 為主，缺的用舊值補
                Id = db._id,
                JigSerial = "",
                Name = db.electrodeName ?? "",
                No = "", // DB 未提供 → 沿用舊值
                Type = db.electrodeType ?? "",
                Status = db.state ?? "",
                HolderNo = "",
                TagSerial = db.tagSerial ?? "",
                MaxDischargeCount = db.lifeTimes.ToString() ?? "",
                UsageRate = "",
                Compensation = db.offset ?? "",
                ProcessedCount = db.useTimes?.ToString() ?? "",
                ElecRestriction = db.restriction ?? false
            };
        }
        private static ElectrodeModel MapProbe(ProbeDto? db)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));
            return new ElectrodeModel
            {
                // 以 API 為主，缺的用舊值補
                Id = db._id,
                JigSerial = "",
                Name = db.probeName ?? "",
                No = "", // DB 未提供 → 沿用舊值
                Type = db.probeType ?? "",
                Status = db.state ?? "",
                HolderNo = "",
                TagSerial = db.tagSerial ?? "",
                MaxDischargeCount = "",
                UsageRate = "",
                Compensation = "",
                ProcessedCount = "",
                ElecRestriction = db.restriction ?? false
            };
        }

        private static WorkpieceModel MapWorkpiece(WorkpieceDto db)
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
                WorkRestriction = db?.restriction ?? false
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


        [RelayCommand]
        public void ShowDetail(string storageId)
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
        public void ShowOverview()
        {
            var overviewVM = new StorageOverviewViewModel(this, _httpService, _storageStore); // ← 傳入 httpService
            var overviewView = new StorageOverviewControl
            {
                DataContext = overviewVM // 這一步非常重要！
            };
            CurrentStorageView = overviewView;
        }

        [RelayCommand]
        public void ShowMachineDetail()
        {
            // TODO: 傳入 storageId 給 DetailControl，如果要的話
            //if(machineDetailViewModel == null) 
            var machineDetailViewModel = new MachineDetailViewModel(this, _httpService); // 傳入自己當 parent 與 httpService
            var overviewView = new MachineDetailControl
            {
                DataContext = machineDetailViewModel // 這一步非常重要！
            };
            CurrentWorkingZoneView = overviewView;
        }
        [RelayCommand]
        public void ShowMachineOverview()
        {
            //  if (machineOverviewViewModel == null)
            var machineOverviewViewModel = new MachineOverviewViewModel(this, _httpService); // 傳入自己當 parent
            var overviewView = new MachineOverviewControl
            {
                DataContext = machineOverviewViewModel // 這一步非常重要！
            };
            CurrentWorkingZoneView = overviewView;
        }
        [RelayCommand]
        private void OpenRobotWindow()
        {
            var win = new ShowRobotWindow();
            var owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            if (owner != null) win.Owner = owner;

            // 需要傳當前選擇的機器人資料時：
            // win.DataContext = new ShowRobotViewModel { DisplayData = SelectedRobotDisplayData };

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
            var m = SelectedSlot?.Material;

            // 👉 沒資料就用一筆假資料打開
            if (m == null)
            {
                var demoElec = new ElectrodeModel
                {
                    No = "E-TEST-001",
                    Name = "示範電極"
                    // 其他屬性依你的模型可再補
                };
                _windowService.ShowMaterialInformation(demoElec, Array.Empty<TimelineItemModel>(), _httpService);
                return;
            }

            var tl = m.Timeline ?? Enumerable.Empty<TimelineItemModel>();

            if (m.Kind == MaterialKind.Electrode && m.Electrode != null)
                _windowService.ShowMaterialInformation(m.Electrode, tl, _httpService);
            else if (m.Kind == MaterialKind.Workpiece && m.Workpiece != null)
                _windowService.ShowMaterialInformation(m.Workpiece, tl, _httpService);
        }


    }
}

