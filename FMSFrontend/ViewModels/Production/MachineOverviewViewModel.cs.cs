using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Standard;
using FMSFrontend.Interfaces;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views.Windows;
using System.Collections.ObjectModel;
using System.Windows;
using static FMSFrontend.ViewModels.ElectrodeDetailViewModel;

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

            // 把卡片的回呼接到最上層的 WindowService：直接用「新資訊視窗」API
            foreach (var card in Machines)
            {
                card.OpenWorkpieceInfo = (wp, tl) => _parent._windowService.ShowMaterialInformation(wp, tl);
                card.OpenElectrodeInfo = (el, tl) => _parent._windowService.ShowMaterialInformation(el, tl);
            }
        }
        [RelayCommand]
        private void ToggleExpand()
        {
            // 假設你要展開到特定 StorageId 的 DetailControl
            _parent.ShowMachineDetail();
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

        // 由外層注入：用 Model + Timeline 直接開視窗
        public Action<WorkpieceModel, IEnumerable<TimelineItemModel>>? OpenWorkpieceInfo { get; set; }
        public Action<ElectrodeModel, IEnumerable<TimelineItemModel>>? OpenElectrodeInfo { get; set; }


        [RelayCommand]
        private void ShowWorkpieceDetail()
        {
            // 若暫時沒有實際資料，給一筆假資料即可開窗
            var wp = new WorkpieceModel { No = "W-TEST-001", Name = "示範工件" };
            OpenWorkpieceInfo?.Invoke(wp, Array.Empty<TimelineItemModel>());
        }

        [RelayCommand]
        private void ShowElectrodeDetail()
        {
            var elec = new ElectrodeModel { No = "E-TEST-001", Name = "示範電極" };
            OpenElectrodeInfo?.Invoke(elec, Array.Empty<TimelineItemModel>());
        }


    }
    public enum MachineType
    {
        EDM = 0,
        ZNC = 1,
        CNC = 2
    }

}
