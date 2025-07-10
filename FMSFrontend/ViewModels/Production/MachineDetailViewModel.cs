using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace FMSFrontend.ViewModels.Production
{
    public partial class MachineDetailViewModel : ObservableObject
    {
        private readonly ProductionLinesViewModel _parent;

        public MachineDetailViewModel(ProductionLinesViewModel parent)
        {
            _parent = parent;
        }

        [RelayCommand]
        private void BackToOverview()
        {
            // 假設你要展開到特定 StorageId 的 DetailControl
            _parent.ShowMachineOverview();
        }

        public ObservableCollection<MachineCardViewModel> MachineDetails { get; } = new()
        {
            new MachineCardViewModel
            {
                MachineName = "EDM-01",
                Status = "Running",
                Type = MachineType.EDM,
                Restriction = false
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
                Type = MachineType.CNC,
                Restriction = false
            },
            new MachineCardViewModel
            {
                MachineName = "EDM-04",
                Status = "Disconnection",
                Type = MachineType.EDM,
                Restriction = true
            },
            new MachineCardViewModel
            {
                MachineName = "ZNC-05",
                Status = "Running",
                Type = MachineType.ZNC,
                Restriction = false
            }
        };
    }
}
