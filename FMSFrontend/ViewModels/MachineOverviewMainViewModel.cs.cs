using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Controls;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Services.FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Models;
using FMSFrontend.ViewModels.Production;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace FMSFrontend.ViewModels
{
    public partial class MachineOverviewMainViewModel : ObservableObject
    {
        // === Services ===
        private readonly IMachinesService _MachinesService;
        public readonly IWorksheetsService _WorksheetsService;

        // === Singleton ===
        private readonly MachineStore _machineStore;
        public ObservableCollection<MachineModel> Machines => _machineStore.Machines;
        
        // ==LiveUpdater===
        public MachineLiveUpdater _machineLiveUpdater;
        DispatcherTimer _timer;
        
        // 全部卡片來源（不變）
        private readonly ObservableCollection<MachineOverviewCard> _allMachines = new();
        [ObservableProperty] private MachineOverviewCard? selectedMachine;
        
        // UI 綁定的卡片清單（會變）
        public ObservableCollection<MachineOverviewCard> FilteredMachines { get; } = new();// UI 綁定的卡片清單（會變）

        [ObservableProperty] private int selectedTabIndex = 0; // 預設選 EDM

        [ObservableProperty] private double cardOpacity = 1.0; // 卡片透明度（用於淡入效果）

        // 讓 Content 能通知 UI 更新
        [ObservableProperty] private MachineMainDetailControl? currentMachineDetailContent;
        
        public MachineOverviewMainViewModel(
            IMachinesService machinesService,
            IWorksheetsService worksheetsService,
            MachineStore machineStore,
            MachineLiveUpdater machineLiveUpdater
            )
        {
            _MachinesService = machinesService;
            _WorksheetsService = worksheetsService;
            _machineStore = machineStore;
            _machineLiveUpdater = machineLiveUpdater;
            /*
            _allMachines = new ObservableCollection<MachineOverviewCard>
        {
            new() { MachineName = "EDM-01", Status = "Stay", Type = MachineType.EDM },
            new() { MachineName = "EDM-02", Status = "Stay", Type = MachineType.EDM },
            new() { MachineName = "EDM-03", Status = "Stay", Type = MachineType.EDM }
        };
            SelectedMachine = _allMachines.First(); // 預設第一台
            // 預設先顯示 MachineMainDetailControl
            
            // 預設先選 EDM
            SelectedTabIndex = 0;
            */
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _timer.Tick += (_, __) => RefreshFromStore();
            _timer.Start();
            _ = _machineLiveUpdater.UpdateStatusAsync();
            RefreshFromStore();
           // ApplyFilterByTab();
        }
                        public void OnPageActivated()
        {
            _machineLiveUpdater.Start();
        }
        public void OnPageDeactivated()
        {
            _machineLiveUpdater.Stop();
            _timer.Stop();
            _timer = null!;
        }
        void RefreshFromStore()
        {
            if (Machines != null)
            {
                for (int i = 0; i < Machines.Count; i++)
                {
                    if (_allMachines.Count == i) _allMachines.Add(new MachineOverviewCard());
                    _allMachines[i].MachineName = Machines[i].MachineName;
                    _allMachines[i].Type = MapToMachineType(Machines[i].Type);
                    _allMachines[i].Status = Machines[i].Status;
                }
                while (_allMachines.Count > Machines.Count)
                {
                    _allMachines.RemoveAt(_allMachines.Count - 1);
                }
            }
            if (SelectedMachine == null && _allMachines.Count > 0)
            {
                SelectedTabIndex = 0;
                SelectedMachine = _allMachines[0];
                _machineLiveUpdater.SelectName = SelectedMachine.MachineName;
            }
            if (CurrentMachineDetailContent == null && SelectedMachine != null)
            {
                CurrentMachineDetailContent =
                    new MachineMainDetailControl(SelectedMachine, _WorksheetsService, _machineStore);
            }
            int j = 0;
            for (int i = 0; i < _allMachines.Count; i++)
            {
                if (SelectedTabIndex == 0 ||
                   (SelectedTabIndex == 1 && _allMachines[i].Type == MachineType.EDM))
                {
                    if (FilteredMachines.Count == j)
                        FilteredMachines.Add(new MachineOverviewCard());
                    FilteredMachines[j].MachineName = _allMachines[i].MachineName;
                    FilteredMachines[j].Status = _allMachines[i].Status;
                    FilteredMachines[j].Type = _allMachines[i].Type;
                    j++;
                }
            }
            while (FilteredMachines.Count > j)
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

        [RelayCommand]
        private void SelectMachine(MachineOverviewCard? card)
        {
            // 重新建立右側詳情區，ViewModel 會依此卡片更新顯示資料
            if (card is null) return;
            SelectedMachine = card;
            _machineLiveUpdater.SelectName = SelectedMachine.MachineName;
            CurrentMachineDetailContent = new MachineMainDetailControl(card, _WorksheetsService, _machineStore);
        }

        partial void OnSelectedTabIndexChanged(int value)
        {
            RefreshFromStore();
            //ApplyFilterByTab();
        }

        private async void ApplyFilterByTab()
        {
            CardOpacity = 0;    // 淡出
            await Task.Delay(100); // 等動畫時間
            FilteredMachines.Clear();
            MachineType? targetType = SelectedTabIndex switch
            {
                0 => null,
                1 => MachineType.EDM,
                2 => MachineType.CNC,         // "??"：顯示所有
                3 => MachineType.ROBOT,
                _ => null
            };

            foreach (var m in _allMachines)
            {
                if (targetType == null || m.Type == targetType)
                    FilteredMachines.Add(m);
            }
            CardOpacity = 0.3;
            await Task.Delay(50);
            CardOpacity = 0.5;
            await Task.Delay(50);
            CardOpacity = 0.7;
            await Task.Delay(50);
            CardOpacity = 1;
        }
    }



    public partial class MachineOverviewCard : ObservableObject
    {
        [ObservableProperty]
        private string machineName = "EDM-XX";

        [ObservableProperty]
        private string status = "idle"; // idle / running / warning / error / disabled
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

        public string MachineImagePath =>
       Type switch
       {
           MachineType.EDM => "pack://application:,,,/FMSFrontend;component/Image/MachineIcons/EDM.png",
           MachineType.CNC => "pack://application:,,,/FMSFrontend;component/Image/MachineIcons/CNC.png",
           MachineType.ZNC => "pack://application:,,,/FMSFrontend;component/Image/MachineIcons/ZNC.png",
           MachineType.ROBOT => "pack://application:,,,/FMSFrontend;component/Image/MachineIcons/Robot.png",
           _ => "pack://application:,,,/FMSFrontend;component/Image/MachineIcons/RobotOff.png"
       };


        // ✅ 提供圖片轉換器綁定使用（EDM → "EDM.png"）
        public string MachineTypeName => Type.ToString();

        // ❓這個卡片本身不包含按鈕，因此以下 Command 不是必需，但若你之後要點卡片顯示詳細，可以保留
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
        ROBOT = 2,
        ZNC  = 3,
        // 其他機型可擴充
    }
}
