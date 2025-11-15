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

namespace FMSFrontend.ViewModels
{
    public partial class MachineOverviewMainViewModel : ObservableObject
    {
        /*
        // === Services ===
        
        private readonly IStorageService _StorageService;
        private readonly IElectrodeService _ElectrodeService;
        private readonly IWorkpieceService _WorkpieceService;
        private readonly IProbeService _ProbeService;
        private readonly IMachinesService _MachinesService;
        public readonly IWorksheetsService _worksheetsService;

        // === Singleton ===
        private readonly MachineStore _machineStore;

        // ==LiveUpdater===
        public MachineLiveUpdater _machineLiveUpdater;
        */
        
        // 全部卡片來源（不變）
        private readonly ObservableCollection<MachineOverviewCard> _allMachines;
        [ObservableProperty]
        private MachineOverviewCard? selectedMachine;

        // UI 綁定的卡片清單（會變）
        public ObservableCollection<MachineOverviewCard> FilteredMachines { get; } = new();

        [ObservableProperty]
        private int selectedTabIndex;
        [ObservableProperty]
        private double cardOpacity = 1.0;

        // 讓 Content 能通知 UI 更新
        [ObservableProperty]
        private object currentMachineDetailContent;

        public MachineOverviewMainViewModel()
        {
            _allMachines = new ObservableCollection<MachineOverviewCard>
        {
            new() { MachineName = "EDM-01", Status = "Stay", Type = MachineType.EDM },
            new() { MachineName = "EDM-02", Status = "Stay", Type = MachineType.EDM },
            new() { MachineName = "EDM-03", Status = "Stay", Type = MachineType.EDM }
        };
            SelectedMachine = _allMachines.First(); // 預設第一台
            // 預設先顯示 MachineMainDetailControl
            CurrentMachineDetailContent = new MachineMainDetailControl(SelectedMachine);
        

            SelectedTabIndex = 0;


            // 預設先選 EDM
            SelectedTabIndex = 0;
            ApplyFilterByTab();
        }

        [RelayCommand]
        private void SelectMachine(MachineOverviewCard? card)
        {
            if (card is null) return;

            SelectedMachine = card;
            // 重新建立右側詳情區，ViewModel 會依此卡片更新顯示資料
            CurrentMachineDetailContent = new MachineMainDetailControl(card);
        }

        partial void OnSelectedTabIndexChanged(int value)
        {
            ApplyFilterByTab();
        }

        private async void ApplyFilterByTab()
        {
            // 淡出
            CardOpacity = 0;

            // 等動畫時間
            await Task.Delay(100);
            

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
