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
            if (Machines != null)
            {
                foreach (var machine in Machines)
                {
                    var existingCard = FilteredMachines.FirstOrDefault(c => c.MachineName == machine.MachineName && c.Type != MachineType.STATION);
                    if (existingCard != null)
                    {
                        existingCard.Status = machine.Status;
                        existingCard.Type = MapToMachineType(machine.Type);
                    }
                    else
                    {
                        var card = new MachineOverviewCard
                        {
                            MachineName = machine.MachineName,
                            Type = MapToMachineType(machine.Type),
                            Status = machine.Status
                        };
                        FilteredMachines.Add(card);
                    }
                }
            }
            // Station card (single instance, update if exists)
            var stationCard = FilteredMachines.FirstOrDefault(c => c.Type == MachineType.STATION);
            if (stationCard == null)
            {
                stationCard = new MachineOverviewCard
                {
                    MachineName = "工作站",
                    Type = MachineType.STATION,
                    Status = Station != null ? "Running" : ""
                };
                FilteredMachines.Add(stationCard);
            }
            else
            {
                stationCard.Status = Station != null ? "Running" : "";
            }
            if (SelectedMachine == null && FilteredMachines.Count > 0)
            {
                SelectedTabIndex = 0;
                SelectedMachine = FilteredMachines[0];
                _machineLiveUpdater.SelectName = SelectedMachine.MachineName;
            }
            if (CurrentMachineDetailContent == null && SelectedMachine != null)
            {
                if (SelectedMachine.Type == MachineType.EDM)
                    CurrentMachineDetailContent = new MachineMainDetailControl(SelectedMachine, this);
                else
                    CurrentMachineDetailContent = new MachineStationControl(SelectedMachine, this);
            }
            
        }
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
        private void SelectMachine(MachineOverviewCard? card)
        {
            if (card is null) return;
            SelectedMachine = card;
            _machineLiveUpdater.SelectName = SelectedMachine.MachineName;
            if (card.Type == MachineType.EDM)
                CurrentMachineDetailContent = new MachineMainDetailControl(card, this);
            else
                CurrentMachineDetailContent = new MachineStationControl(card, this);
        }

        partial void OnSelectedTabIndexChanged(int value)
        {
            RefreshFromStore();
        }
    }

    public partial class MachineOverviewCard : ObservableObject
    {
        [ObservableProperty]
        private string machineName = "EDM-XX";

        [ObservableProperty]
        private string status = "idle";
        public Brush StatusBrush => Status switch
        {
            "Running" => new SolidColorBrush(Color.FromRgb(0x56, 0xC0, 0x6C)), //綠色 1
            "Stopping" => new SolidColorBrush(Color.FromRgb(0xE6, 0xB9, 0x3E)), //黃色 2
            "EmergencyStop" => new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B)), //紅色 3
            _ => new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)), //灰色 
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

