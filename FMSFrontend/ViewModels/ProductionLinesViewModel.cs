using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging; // ← 新增
using CommunityToolkit.Mvvm.Messaging.Messages; // ← 新增：Message 型別

using FMSFrontend.Controls;
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
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using static FMSFrontend.ViewModels.ElectrodeDetailViewModel;

namespace FMSFrontend.ViewModels
{
    public partial class ProductionLinesViewModel : ObservableObject
    {
        public readonly IWindowService _windowService;
        private readonly IHttpService _httpService;

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


         // 新增：Timer 欄位
        private readonly DispatcherTimer _refreshTimer;
        public ProductionLinesViewModel(IWindowService windowService, IHttpService httpService) // ← 變更簽章
        {
            _windowService = windowService;
            _httpService = httpService;

            INIFile ini = new INIFile(AppDomain.CurrentDomain.BaseDirectory + "\\Basesitting.ini");
            bool b = ini.Read("Prarm", "IsStorageOverviewControl") == "True";
            // 新增：建立並啟動每秒刷新 Timer
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(100)
            };
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
            // 訂閱 MainWindowViewModel 的頁面刷新訊息：當切換到 RFIDBind 時重新抓取
            WeakReferenceMessenger.Default.Register<ValueChangedMessage<string>>(this, (r, message) =>
            {
                if (string.Equals(message.Value, "ProductionLines", StringComparison.Ordinal))
                {
                    if (b)
                        ShowOverview();
                    else
                        ShowDetail("0");
                    ShowMachineOverview();
                }
            });
        }
        // 新增：Timer Tick 處理器（輕量、fire-and-forget）
        private void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                // 如果 CurrentStorageView 為 FrameworkElement（你的 View），取其 DataContext
                var storageDc = (CurrentStorageView as System.Windows.FrameworkElement)?.DataContext;
                if (storageDc is Production.StorageOverviewViewModel sovm)
                {
                    _ = sovm.RefreshAsync();
                }
                else if (storageDc is Production.StorageDetailViewModel sdvm)
                {
                    _ = sdvm.RefreshAsync();
                }

                var workingDc = (CurrentWorkingZoneView as System.Windows.FrameworkElement)?.DataContext;
                if (workingDc is Production.MachineDetailViewModel mdvm)
                {
                    _ = mdvm.RefreshAsync();
                }
                else if (workingDc is Production.MachineOverviewViewModel movm)
                {
                    _ = movm.RefreshAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RefreshTimer_Tick error: {ex}");
            }
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

        // 你原本給 Overview 用的：
        public void OpenMaterial(StorageSlotViewModel slot) => OpenMaterial((object)slot);

        // ★ Detail 也能用：
        public void OpenMaterial(SlotViewModel slot) => OpenMaterial((object)slot);

        // ★ 統一處理（Empty 也能開）＋ 先打 API 取得最新資料
        public async void OpenMaterial(object slot)
        {
            // 取得 Material 與 SlotCode
            MaterialRef? material = slot switch
            {
                SlotViewModel s => s.Material,
                StorageSlotViewModel ss => ss.Material,
                IHasMaterial ih => ih.Material,
                _ => null
            };

            var slotCode =
                    (slot as SlotViewModel)?.SlotCode ??
                    (slot as StorageSlotViewModel)?.SlotCode;

            if (material == null)
            {
                _windowService.ShowMaterialEmpty(_httpService);
                return;
            }
            //try
            //{
            // 先依 TagSerial 呼叫對應 API，更新詳細資料與時間軸
            // 1. 資料
      
            if (material.Kind == MaterialKind.Electrode)
            {
                // ElectrodeModel 使用 TagSerial
                var tagSerial = material.Electrode?.TagSerial;
                if (string.IsNullOrWhiteSpace(tagSerial))
                {
                    // 沒有 TagSerial 就 fallback
                    ElectrodeModel elecFallback = material.Electrode ?? new ElectrodeModel();
                    _windowService.ShowElectrode(elecFallback, material.Timeline ?? Array.Empty<TimelineItemModel>(),_httpService, slotCode);
                    return;
                }

                // 1. 資料
                var elecList = await _httpService.GetJsonAsync<List<Electrode>>($"Electrode/DB_GetElectrodesByTagSerial/{tagSerial}");
                var dbElec = elecList?.FirstOrDefault();
                // 映射 DB → View Model（若 API 無部分欄位，用舊值補）
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
                JsonElement? json = await _httpService.GetJsonAsync<JsonElement>(route, default);
                Workpiece dbWp = (json.HasValue && json.Value.ValueKind != JsonValueKind.Undefined) ?
                JsonSerializer.Deserialize<Workpiece>(json.Value.GetRawText()) ?? new Workpiece() :
                new Workpiece();
                var wpVm = MapWorkpiece(dbWp, material.Workpiece);
                wpVm.StorageRestriction = material.Workpiece?.StorageRestriction ?? false;
                var id = wpVm.Id;
                // 2. 時間軸
                var wpTimeline = await _httpService.GetJsonAsync<IEnumerable<TimelineItemModel>>($"Workpiece/DB_GetWorkpieceTimelineByWorkpieceId/{id}")
                                 ?? Array.Empty<TimelineItemModel>();
                //3.顯示資料
                _windowService.ShowWorkpiece(wpVm, wpTimeline, _httpService, slotCode);
            }
            /*
            }
            catch
            {
                // API 失敗 → 使用既有資料顯示，確保不影響操作
                var tl = material.Timeline ?? Array.Empty<TimelineItemModel>();
                if (material.Kind == MaterialKind.Electrode)
                    _windowService.ShowElectrode(material.Electrode, tl, slotCode);
                else
                    _windowService.ShowWorkpiece(material.Workpiece, tl, slotCode);
            }
            */
        }

        private static ElectrodeModel MapElectrode(Electrode? db, ElectrodeModel? fallback)
        {
            if (db == null) return fallback ?? new ElectrodeModel
            {
                Name = "—",
                Status = "—"
            };

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
                ElecRestriction = db.restriction 
            };
        }

        private static WorkpieceModel MapWorkpiece(Workpiece? db, WorkpieceModel? fallback)
        {
            if (db == null) return fallback ?? new WorkpieceModel
            {
                Name = "—",
                Status = "—"
            };

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
                WorkRestriction = db.restriction ?? false
            };
        }

        [RelayCommand]
        public void ShowDetail(string storageId)
        {
            // TODO: 傳入 storageId 給 DetailControl，如果要的話
            var overviewVM = new StorageDetailViewModel(this, _httpService); // 傳入自己當 parent
            var overviewView = new StorageDetailControl
            {
                DataContext = overviewVM // 這一步非常重要！
            };
            CurrentStorageView = overviewView;
        }
        [RelayCommand]
        public void ShowOverview()
        {
            var overviewVM = new StorageOverviewViewModel(this, _httpService); // ← 傳入 httpService
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
            var overviewVM = new MachineDetailViewModel(this, _httpService); // 傳入自己當 parent 與 httpService
            var overviewView = new MachineDetailControl
            {
                DataContext = overviewVM // 這一步非常重要！
            };
            CurrentWorkingZoneView = overviewView;
        }
        [RelayCommand]
        public void ShowMachineOverview()
        {
            var overviewVM = new MachineOverviewViewModel(this, _httpService); // 傳入自己當 parent
            var overviewView = new MachineOverviewControl
            {
                DataContext = overviewVM // 這一步非常重要！
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
