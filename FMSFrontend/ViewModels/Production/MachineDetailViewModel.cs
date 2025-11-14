using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Services.FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Models;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views.Windows;
//using OSCARMAXFMS_V3.DBmodels;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace FMSFrontend.ViewModels.Production
{
    public partial class MachineDetailViewModel : ObservableObject
    {
        public ProductionLinesViewModel _parent;
        private readonly IHttpService _httpService;

        // === Services ===
        private readonly IStorageService _StorageService;
        private readonly IElectrodeService _ElectrodeService;
        private readonly IWorkpieceService _WorkpieceService;
        private readonly IProbeService _ProbeService;
        private readonly IMachinesService _MachinesService;

        //== Store ==
        private readonly MachineStore _machineStore;
        public ObservableCollection<MachineModel> Machines => _machineStore.Machines;
        //== LiveUpdater ==
        public MachineLiveUpdater _machineLiveUpdater;
        public ObservableCollection<MachineCardViewModel> MachineCardVm { get; } = new();

        // Make the collection settable so we can replace it in one UI operation to avoid per-item layout churn
        private ObservableCollection<MachineCardViewModel> _machineDetails = new();
        public ObservableCollection<MachineCardViewModel> MachineDetails
        {
            get => _machineDetails;
            private set => SetProperty(ref _machineDetails, value);
        }
        DispatcherTimer _timer;
        public MachineDetailViewModel(ProductionLinesViewModel parent, IHttpService httpService,
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

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
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
                for (int i = 0; i < Machines.Count; i++)
                {
                    updateCnt = i;
                    if (_machineDetails.Count == updateCnt) //當沒有對應的 CardVm 時，新增一個
                    {
                        _machineDetails.Add(new MachineCardViewModel(_parent));
                    }
                    // 更新 CardVm 的內容
                    _machineDetails[updateCnt].MachineName = Machines[updateCnt].MachineName;
                    _machineDetails[updateCnt].Type = MapToMachineType(Machines[updateCnt].Type);
                    _machineDetails[updateCnt].Status = Machines[updateCnt].Status;
                    _machineDetails[updateCnt].Restriction = Machines[updateCnt].Restriction;
                    _machineDetails[updateCnt].onDeckElectrodeSerial = Machines[updateCnt].onDeckElectrodeSerial;
                    _machineDetails[updateCnt].onDeckWorkpieceSerial = Machines[updateCnt].onDeckWorkpieceSerial;
                    _machineDetails[updateCnt].onDeckWorksheetSerial = Machines[updateCnt].onDeckWorksheetSerial;
                }
                while (_machineDetails.Count > Machines.Count) //刪除多餘的 CardVm
                {
                    _machineDetails.RemoveAt(_machineDetails.Count - 1);
                }
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
        private void BackToOverview()
        {
            _parent.ShowMachineOverview();
        }

        [RelayCommand]
        private void OpenMachineWindow(object? machine)   // machine 建議是 MachineCardViewModel
        {
            _parent.OpenMachineWindow(machine);
        }
    }
}
