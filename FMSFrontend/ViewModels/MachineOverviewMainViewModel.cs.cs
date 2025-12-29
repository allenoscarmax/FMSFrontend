using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Controls;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Services.FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Interfaces;
using FMSFrontend.Models;
using FMSFrontend.ViewModels.Factory;
using FMSFrontend.ViewModels.Production;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace FMSFrontend.ViewModels
{
    public partial class MachineOverviewMainViewModel : ObservableObject
    {
        // === Services ===
        public readonly IWindowService _windowService;
        public readonly IMachinesService _machinesService;
        public readonly IWorksheetsService _worksheetsService;
        public readonly IPlcService _plcService;

        // === Singleton ===
        public readonly MachineStore _machineStore;
        public ObservableCollection<MachineModel> Machines => _machineStore.Machines;

        public readonly StationStore _stationStore;
        public StationModel Station => _stationStore.Station;
        // ==LiveUpdater===
        public MachineLiveUpdater _machineLiveUpdater;
        public StationLiveUpdater _stationLiveUpdater;
        DispatcherTimer _timer;
        
        [ObservableProperty] private MachineOverviewCard? selectedMachine;

        // UI 綁定的卡片清單（會變）
        public ObservableCollection<MachineOverviewCard> FilteredMachines { get; } = new(); 
        public ObservableCollection<MachineOverviewCard> AllMachines { get; } = new();

        [ObservableProperty] private int selectedTabIndex = 0; // 預設選 EDM


        [ObservableProperty] private double cardOpacity = 1.0; // 卡片透明度（用於淡入效果）

        // 讓 Content 能通知 UI 更新（使用手動屬性確保型別為 UserControl）
        private UserControl? _currentMachineDetailContent;
        public UserControl? CurrentMachineDetailContent
        {
            get => _currentMachineDetailContent;
            set => SetProperty(ref _currentMachineDetailContent, value);
        }
        
        public MachineOverviewMainViewModel(
            IWindowService windowService,
            IMachinesService machinesService,
            IWorksheetsService worksheetsService,
            IPlcService plcService,
            MachineStore machineStore,
            StationStore stationStore,
            MachineLiveUpdater machineLiveUpdater,
            StationLiveUpdater stationLiveUpdater
            )
        {
            _plcService = plcService;
            _windowService = windowService;
            _machinesService = machinesService;
            _worksheetsService = worksheetsService;
           
            _machineStore = machineStore;
            _stationStore = stationStore;

            _machineLiveUpdater = machineLiveUpdater;
            _stationLiveUpdater = stationLiveUpdater;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _timer.Tick += (_, __) => RefreshFromStore();
            _timer.Start();

            _ = _machineLiveUpdater.UpdateStatusAsync();
            RefreshFromStore();
            RebuildFilteredMachines();
        }
        public void OnPageActivated()
        {
            _machineLiveUpdater.Start();
            _stationLiveUpdater.Start();
        }
        public void OnPageDeactivated()
        {
            _machineLiveUpdater.Stop();
            _stationLiveUpdater.Stop();
            _timer.Stop();
            _timer = null!;
        }
        void RefreshFromStore()
        {
            // 1) 同步 AllMachines（新增 / 更新 / 移除）
            if (Machines != null)
            {
                foreach (var machine in Machines)
                {
                    var existingCard = AllMachines
                        .FirstOrDefault(c => c.MachineName == machine.MachineName &&
                                             c.Type != MachineType.STATION);

                    if (existingCard != null) // 更新機台卡片
                    {
                        existingCard.Status = machine.Status;
                        existingCard.Type = MapToMachineType(machine.Type);
                    }
                    else // 新增機台卡片
                    {
                        AllMachines.Add(new MachineOverviewCard
                        {
                            MachineName = machine.MachineName,
                            Type = MapToMachineType(machine.Type),
                            Status = machine.Status
                        });
                    }
                }

                // 移除不存在的機台卡片（用倒序迴圈避免邊走邊刪）
                for (int i = AllMachines.Count - 1; i >= 0; i--)
                {
                    var card = AllMachines[i];
                    if (card.Type == MachineType.STATION) continue;

                    if (!Machines.Any(m => m.MachineName == card.MachineName))
                    {
                        AllMachines.RemoveAt(i);
                    }
                }
            }

            // 2) Station 卡（單一張）
            if (AllMachines.Count > 0)
            {
                var stationCard = AllMachines.FirstOrDefault(c => c.Type == MachineType.STATION);
                if (stationCard == null)
                {
                    AllMachines.Add(new MachineOverviewCard
                    {
                        MachineName = "工作站",
                        Type = MachineType.STATION,
                        Status = Station != null ? "Running" : ""
                    });
                }
                else
                {
                    stationCard.Status = Station != null ? "Running" : "";
                }
            }

            // 3) 更新畫面上「已經存在」的 FilteredMachines 狀態（不重建清單）
            foreach (var card in FilteredMachines)
            {
                var src = AllMachines.FirstOrDefault(m =>
                    m.MachineName == card.MachineName &&
                    m.Type == card.Type);

                if (src != null)
                {
                    card.Status = src.Status;
                    card.Type = src.Type; // 基本上不太會變，但寫著也沒關係
                }
            }

            // 4) 如果一開始什麼都還沒有，第一次進來幫你建一次清單/預設選項
            if (FilteredMachines.Count == 0 && AllMachines.Count > 0)
            {
                RebuildFilteredMachines();
                if (FilteredMachines.Count > 0 && SelectedMachine == null)
                {
                    SelectedMachine = FilteredMachines[0];
                    _machineLiveUpdater.SelectName = SelectedMachine.MachineName;

                    CurrentMachineDetailContent = SelectedMachine.Type == MachineType.EDM
                        ? new MachineMainDetailControl(SelectedMachine, this)
                        : new MachineStationControl(SelectedMachine, this);
                }
            }
        }
        void RebuildFilteredMachines()
        {
            FilteredMachines.Clear();

            IEnumerable<MachineOverviewCard> source = SelectedTabIndex switch
            {
                0 => AllMachines, // ALL
                1 => AllMachines.Where(m => m.Type == MachineType.EDM),
                2 => AllMachines.Where(m => m.Type == MachineType.STATION),
                _ => AllMachines
            };

            foreach (var m in source)
                FilteredMachines.Add(m);
        }

        /* 先前版本
        int FilterCnt = 0;
        void FilteredMachinesInit()
        {
            FilterCnt = 0;
        }
        void UpdataFilteredMachines(MachineType type)
        {
            for (int i = 0; i < AllMachines.Count; i++)
            {
                if (AllMachines[i].Type == type) //更新資料
                {
                    if (FilteredMachines.Count == FilterCnt)
                    {
                        FilteredMachines.Add(new MachineOverviewCard());
                    }
                    FilteredMachines[FilterCnt].MachineName = AllMachines[i].MachineName;
                    FilteredMachines[FilterCnt].Type = AllMachines[i].Type;
                    FilteredMachines[FilterCnt].Status = AllMachines[i].Status;
                    FilterCnt++;
                }
            }
        }
        void RemoveFilteredMachines()
        {
            while (FilteredMachines.Count > FilterCnt)
            {
                FilteredMachines.RemoveAt(FilteredMachines.Count - 1); 
            }
        }*/
        private static MachineType MapToMachineType(string s)
        {
            return s switch
            {
                "EDM" => MachineType.EDM,
                "CNC" => MachineType.CNC,
                _ => MachineType.EDM,
            };
        }

        [RelayCommand]
        private async Task SelectMachine(MachineOverviewCard? card)
        {
            if (card is null) return;

            // ⭐ 如果點到的是同一張卡片，就什麼都不做，避免重建畫面
            if (SelectedMachine == card && CurrentMachineDetailContent != null)
                return;

            SelectedMachine = card;
            _machineLiveUpdater.SelectName = SelectedMachine.MachineName;

            if (CurrentMachineDetailContent is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else if (CurrentMachineDetailContent is IDisposable disposable)
            {
                disposable.Dispose();
            }

            CurrentMachineDetailContent = card.Type == MachineType.EDM
                ? new MachineMainDetailControl(card, this)
                : new MachineStationControl(card, this);
        }
        partial void OnSelectedTabIndexChanged(int value)
        {
            // 1) 根據 Tab 重建卡片列
            RebuildFilteredMachines();

            if (FilteredMachines.Count == 0)
                return;

            // 2) 如果原本選的機台不在新的清單裡，就選第一台
            if (SelectedMachine == null ||
                !FilteredMachines.Contains(SelectedMachine))
            {
                SelectedMachine = FilteredMachines[0];
                _machineLiveUpdater.SelectName = SelectedMachine.MachineName;

                // 重建詳細內容
                if (CurrentMachineDetailContent is IAsyncDisposable asyncDisposable)
                    _ = asyncDisposable.DisposeAsync();
                else if (CurrentMachineDetailContent is IDisposable disposable)
                    disposable.Dispose();

                CurrentMachineDetailContent = SelectedMachine.Type == MachineType.EDM
                    ? new MachineMainDetailControl(SelectedMachine, this)
                    : new MachineStationControl(SelectedMachine, this);
            }
            // 如果原本選的機台還在清單裡，就什麼都不做，下面畫面保持不動
        }

        /*  先前版本
        // 修正：移除重複定義，並避免每次切換 Tab 強制清空 SelectedMachine
        partial void OnSelectedTabIndexChanged(int value)
        {
            // 更新資料
            RefreshFromStore();

            SelectedMachine = null;

            if (SelectedMachine == null && FilteredMachines.Count > 0)
            {
                SelectedMachine = FilteredMachines[0];
                _machineLiveUpdater.SelectName = SelectedMachine.MachineName;

                // 重建詳細內容
                if (CurrentMachineDetailContent is IAsyncDisposable asyncDisposable)
                    _ = asyncDisposable.DisposeAsync();
                else if (CurrentMachineDetailContent is IDisposable disposable)
                    disposable.Dispose();

                CurrentMachineDetailContent = SelectedMachine.Type == MachineType.EDM
                    ? new MachineMainDetailControl(SelectedMachine, this)
                    : new MachineStationControl(SelectedMachine, this);
            }
        }
        */
    }

    public partial class MachineOverviewCard : ObservableObject
    {
        [ObservableProperty]
        private string machineName = "EDM-XX";

        [ObservableProperty]
        private string status = "idle";
        public Brush StatusBrush => Status switch
        {
            "Running" => new SolidColorBrush(Color.FromRgb(0x56, 0xC0, 0x6C)),
            "Stopping" => new SolidColorBrush(Color.FromRgb(0xE6, 0xB9, 0x3E)),
            "EmergencyStop" => new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B)),
            _ => new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)),
        };
        partial void OnStatusChanged(string value) => OnPropertyChanged(nameof(StatusBrush));
        [ObservableProperty]
        private MachineType type = MachineType.EDM;

        public string MachineImagePath => Type switch
        {
            MachineType.EDM =>     "pack://application:,,,/FMSFrontend;component/Image/MachineIcons/EDM.png",
            MachineType.CNC =>     "pack://application:,,,/FMSFrontend;component/Image/MachineIcons/CNC.png",
            MachineType.ZNC =>     "pack://application:,,,/FMSFrontend;component/Image/MachineIcons/ZNC.png",
            MachineType.ROBOT =>   "pack://application:,,,/FMSFrontend;component/Image/MachineIcons/Robot.png",
            MachineType.STATION => "pack://application:,,,/FMSFrontend;component/Image/MachineIcons/FMS.png",
            _ => "pack://application:,,,/FMSFrontend;component/Image/MachineIcons/RobotOff.png"
        };
        partial void OnTypeChanged(MachineType value)
        {
            OnPropertyChanged(nameof(MachineImagePath));
        }

        public string MachineTypeName => Type.ToString();

        [RelayCommand]
        private void ShowDetail()
        {
            System.Diagnostics.Debug.WriteLine($"【{MachineName}】點擊卡片顯示詳細");
        }
    }
    public enum MachineType
    {
        EDM = 0,
        CNC = 1,
        ZNC = 2,
        ROBOT = 3,
        STATION = 4
    }
}

