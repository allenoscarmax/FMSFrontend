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
using IniFile;
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
        int StationCount = 0; //工作站數量
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

            INIFile ini = new INIFile(AppDomain.CurrentDomain.BaseDirectory + "Basesitting.ini");
            try
            {
                StationCount = Convert.ToInt16(ini.Read("Prarm", "StationCount"));
            }
            catch { }
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
                    var existingCard = AllMachines.FirstOrDefault(c => c.MachineName == machine.MachineName && c.Type != MachineType.STATION);
                    if (existingCard != null) //更新機台卡片
                    {
                        existingCard.Status = machine.Status;
                        existingCard.Type = MapToMachineType(machine.Type);
                    }
                    else//新增機台卡片
                    {
                        var card = new MachineOverviewCard
                        {
                            MachineName = machine.MachineName,
                            Type = MapToMachineType(machine.Type),
                            Status = machine.Status
                        };
                        AllMachines.Add(card);
                    }
                }
                //移除不存在的機台卡片
                foreach (var machine in AllMachines)
                {
                    if (machine.Type == MachineType.STATION) continue;
                    if (!Machines.Any(m => m.MachineName == machine.MachineName))
                    {
                        AllMachines.Remove(machine);
                        break; //跳出迴圈避免修改集合時發生錯誤
                    }
                }
            }

            if (AllMachines != null && AllMachines.Count > 0 && StationCount != 0)
            {
                // Station card (single instance, update if exists)
                var stationCard = AllMachines.FirstOrDefault(c => c.Type == MachineType.STATION);
                if (stationCard == null)
                {
                    stationCard = new MachineOverviewCard
                    {
                        MachineName = "工作站",
                        Type = MachineType.STATION,
                        Status = Station != null ? "Running" : ""
                    };
                    AllMachines.Add(stationCard);
                }
                else
                {
                    stationCard.Status = Station != null ? "Running" : "";
                }
            }
            FilteredMachinesInit(); //初始化計數
            //將AllMachines依照類型排序
            if (SelectedTabIndex == 0 || SelectedTabIndex == 1) //EDM or All
            {
                UpdataFilteredMachines(MachineType.EDM);
            }
            if (SelectedTabIndex == 0 || SelectedTabIndex == 2) //STATION or All
            {
                UpdataFilteredMachines(MachineType.STATION);
            }
            RemoveFilteredMachines();
           
            if (SelectedMachine == null && FilteredMachines.Count > 0)
            {
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
        private static async ValueTask CloseUserControlAsync(UserControl? control, CancellationToken ct = default)
        {
            if (control is null) return;

            if (control is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else if (control is IDisposable disposable)
            {
                disposable.Dispose();
            }

            control.Visibility = System.Windows.Visibility.Collapsed;
        }

        [RelayCommand]
        private async Task SelectMachine(MachineOverviewCard? card)
        {
            if (card is null) return;
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
            if (card.Type == MachineType.EDM)
                CurrentMachineDetailContent = new MachineMainDetailControl(card, this);
            else
                CurrentMachineDetailContent = new MachineStationControl(card, this);
        }

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

