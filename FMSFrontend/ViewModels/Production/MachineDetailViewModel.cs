using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views.Windows;
using System.Collections.ObjectModel;
using System.Windows;

namespace FMSFrontend.ViewModels.Production
{
    public partial class MachineDetailViewModel : ObservableObject
    {
        private readonly ProductionLinesViewModel _parent;

        public MachineDetailViewModel(ProductionLinesViewModel parent)
        {
            _parent = parent;
            foreach (var card in MachineDetails)
            {
                card.OpenWorkpieceInfo = (wp, tl) => _parent._windowService.ShowMaterialInformation(wp, tl);
                card.OpenElectrodeInfo = (el, tl) => _parent._windowService.ShowMaterialInformation(el, tl);
            }
        }

        [RelayCommand]
        private void BackToOverview()
        {
            // 假設你要展開到特定 StorageId 的 DetailControl
            _parent.ShowMachineOverview();
        }
        [RelayCommand]
        private void OpenMachineWindow(object? machine)   // machine 建議是 MachineCardViewModel
        {
            var vm = new ShowMachineWindowViewModel(machine);
            var win = new ShowMachineWindow { DataContext = vm };

            var owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            if (owner != null) win.Owner = owner;

            win.ShowDialog();
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
