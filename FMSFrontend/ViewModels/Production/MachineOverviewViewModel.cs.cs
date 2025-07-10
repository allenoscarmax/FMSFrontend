using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace FMSFrontend.ViewModels.Production
{
    public partial class MachineOverviewViewModel : ObservableObject
    {
        private readonly ProductionLinesViewModel _parent;
        public ObservableCollection<MachineCardViewModel> Machines { get; } = new()
        {
            new MachineCardViewModel
            {
                MachineName = "EDM-01",
                Status = "Running",
                Type = MachineType.EDM,
                Restriction = true
                
            },
            new MachineCardViewModel
            {
                MachineName = "ZNC-02",
                Status = "Stay",
                Type = MachineType.ZNC,
                Restriction = true
            },
            new MachineCardViewModel
            {
                MachineName = "CNC-03",
                Status = "Alarm",
                Type = MachineType.CNC
            },
            new MachineCardViewModel
            {
                MachineName = "EDM-04",
                Status = "Stay",
                Type = MachineType.EDM
            },
            new MachineCardViewModel
            {
                MachineName = "CNC-05",
                Status = "Disconnection",
                Type = MachineType.CNC
            }
        };
        public MachineOverviewViewModel(ProductionLinesViewModel parent)
        {
            _parent = parent;
            
        }
        [RelayCommand]
        private void ToggleExpand()
        {
            // 假設你要展開到特定 StorageId 的 DetailControl
            _parent.ShowMachineDetail();
        }
    }

    public partial class MachineCardViewModel : ObservableObject
    {
        [ObservableProperty]
        private string machineName = "EDM-XX";

        [ObservableProperty]
        private string status = "idle"; // 可為 idle / running / warning / error / disabled

        [ObservableProperty]
        private MachineType type = MachineType.EDM;

        [ObservableProperty]
        private bool restriction;

        // ✅ 圖片綁定使用的字串（自動從 enum 轉成檔名）
        public string MachineTypeName => Type.ToString();

        [RelayCommand]
        private void ShowWorkpieceDetail()
        {
            System.Diagnostics.Debug.WriteLine($"【{MachineName}】工件詳情");
        }

        [RelayCommand]
        private void ShowElectrodeDetail()
        {
            System.Diagnostics.Debug.WriteLine($"【{MachineName}】電極詳情");
        }


    }
    public enum MachineType
    {
        EDM = 0,
        ZNC = 1,
        CNC = 2
    }

}
