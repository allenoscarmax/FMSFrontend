using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Standard;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Services.FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Interfaces;
using FMSFrontend.Models;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Factory;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views.Windows;
using OSCARMAXFMS_V3.DBmodels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using static FMSFrontend.ViewModels.ElectrodeDetailViewModel;
using static FMSFrontend.ViewModels.ProductionLinesViewModel;
namespace FMSFrontend.ViewModels.Production
{
    public partial class MachineOverviewViewModel : ObservableObject
    {
        private readonly ProductionLinesViewModel _parent;
        private readonly IHttpService _httpService;

        //== Services ==
        private readonly IStorageService _StorageService;
        private readonly IElectrodeService _ElectrodeService;
        private readonly IWorkpieceService _WorkpieceService;
        private readonly IProbeService _ProbeService;
        private readonly IMachinesService _MachinesService;
        //== Store ==
        private readonly MachineStore _machineStore;
        public ObservableCollection<MachineModel> Machines => _machineStore.Machines;
        DispatcherTimer _timer;
        //== LiveUpdater ==
        public MachineLiveUpdater _machineLiveUpdater;
        public ObservableCollection<MachineCardViewModel> MachineCardVm { get; } = new();
        public MachineOverviewViewModel(ProductionLinesViewModel parent, IHttpService httpService,
             IElectrodeService electrodeService, IWorkpieceService workpieceService, IProbeService probeService,
             IStorageService storageService, IMachinesService machinesService,
             MachineLiveUpdater machineLiveUpdater, MachineStore machineStore)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));

            _StorageService = storageService;
            _ElectrodeService = electrodeService;
            _WorkpieceService = workpieceService;
            _ProbeService = probeService;
            _MachinesService = machinesService;

            _machineLiveUpdater = machineLiveUpdater;
            _machineStore = machineStore;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _timer.Tick += (_, __) => BuildCardsFromMachines();
            _timer.Start();
        }
        public void OnPageActivated()
        {
        }
        public void OnPageDeactivated()
        {
            _timer.Stop();
            _timer = null!;
        }
        int updateCnt = 0;
        private void BuildCardsFromMachines()
        {
            void build()
            {
                if (Machines.Count == 0) return; 
                if (updateCnt > Machines.Count ) updateCnt = 0;
                if (MachineCardVm.Count == updateCnt)
                {
                    MachineCardVm.Add(new MachineCardViewModel());
                    MachineCardVm[updateCnt].OpenWorkpieceInfo = (wp, tl) => _parent._windowService.ShowMaterialInformation(wp, tl,
                    _httpService, _ElectrodeService, _WorkpieceService, _ProbeService, _StorageService);
                    MachineCardVm[updateCnt].OpenElectrodeInfo = (el, tl) => _parent._windowService.ShowMaterialInformation(el, tl,
                        _httpService, _ElectrodeService, _WorkpieceService, _ProbeService, _StorageService);
                }
                MachineCardVm[updateCnt].MachineName = Machines[updateCnt].MachineName;
                MachineCardVm[updateCnt].Type = MapToMachineType(Machines[updateCnt].Type);
                MachineCardVm[updateCnt].Status = Machines[updateCnt].Status;

                while (MachineCardVm.Count > Machines.Count)
                {
                    MachineCardVm.RemoveAt(MachineCardVm.Count - 1);
                }
                updateCnt = (updateCnt + 1) % Machines.Count;
            }
            var disp = Application.Current?.Dispatcher;
            if (disp != null && !disp.CheckAccess()) disp.Invoke(build);
            else build();
        }

        private static MachineCardViewModel MapMachinesDto(MachineModel m)
        {
            return new MachineCardViewModel
            {
                MachineName = m.MachineName,
                Status = m.Status,
                Type = MapToMachineType(m.Type),
                Restriction = m.Restriction
            };
        }
        private static MachineType MapToMachineType(string s)
        {
            return s switch
            {
                "EDM" => MachineType.EDM,
                "CNC" => MachineType.CNC,
                "ZNC" => MachineType.ZNC,
                _ => MachineType.EDM,
            };
        }
        [RelayCommand]
        private void ToggleExpand()
        {
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
