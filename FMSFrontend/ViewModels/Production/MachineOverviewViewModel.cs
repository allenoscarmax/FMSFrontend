using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Standard;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Interfaces;
using FMSFrontend.Models;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Factory;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.Intrinsics.Arm;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
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
                for (int i = 0; i < Machines.Count; i++)
                {
                    updateCnt = i;
                    if (MachineCardVm.Count == updateCnt)
                    {
                        // 使用帶 parent 的建構函式，確保指令可運作
                        MachineCardVm.Add(new MachineCardViewModel(_parent));
                    }
                    MachineCardVm[updateCnt].MachineName = Machines[updateCnt].MachineName;
                    MachineCardVm[updateCnt].machineModel = Machines[updateCnt].machineModel;
                    MachineCardVm[updateCnt].Manufacturer = Machines[updateCnt].Manufacturer;

                    MachineCardVm[updateCnt].Type = MapToMachineType(Machines[updateCnt].Type);
                    MachineCardVm[updateCnt].Status = Machines[updateCnt].Status;
                    MachineCardVm[updateCnt].onDeckElectrodeSerial = Machines[updateCnt].OnDeckElectrodeSerial;
                    MachineCardVm[updateCnt].onDeckWorkpieceSerial = Machines[updateCnt].OnDeckWorkpieceSerial;
                    MachineCardVm[updateCnt].onDeckWorksheetSerial = Machines[updateCnt].OnDeckWorksheetSerial;
                    MachineCardVm[updateCnt].ElectrodeName = Machines[updateCnt].ElectrodeShortName;
                    MachineCardVm[updateCnt].WorkpieceName = Machines[updateCnt].WorkpieceShortName;
                    MachineCardVm[updateCnt].MainProgramName = Machines[updateCnt].OscarEdm.MainProgramName;
                    MachineCardVm[updateCnt].CycleTime = Machines[updateCnt].OscarEdm.CycleTime;
                    if (Machines[updateCnt].MachineName.IndexOf("EDM") != -1)
                        MachineCardVm[updateCnt].Restriction = !Machines[updateCnt].OscarEdm.CanControl;
                    else if (Machines[updateCnt].MachineName.IndexOf("FanucCNC") != -1)
                        MachineCardVm[updateCnt].Restriction = !Machines[updateCnt].SunmillFanucCNC.CanControl;
                    else if (Machines[updateCnt].MachineName.IndexOf("SiemensCNC") != -1)
                        MachineCardVm[updateCnt].Restriction = !Machines[updateCnt].SunmillSiemensCNC.CanControl;
                    else if (Machines[updateCnt].MachineName.IndexOf("CMM") != -1)
                        MachineCardVm[updateCnt].Restriction = !Machines[updateCnt].MitutoyoCMM.CanControl;
                }
                while (MachineCardVm.Count > Machines.Count)
                {
                    MachineCardVm.RemoveAt(MachineCardVm.Count - 1);
                }
                /* // 測試用假資料
                if (MachineCardVm.Count == 3)
                {
                    MachineCardVm[0].MachineName = "1";
                    MachineCardVm[1].MachineName = "2";
                    MachineCardVm[2].MachineName = "3";

                    MachineCardVm[0].Status = "";
                    MachineCardVm[1].Status = "Stopping";
                    MachineCardVm[2].Status = "EmergencyStop";

                    MachineCardVm[0].Restriction = false;
                    MachineCardVm[1].Restriction = false;
                    MachineCardVm[2].Restriction = false;
                }
                //*/
                //MachineCardVm[0].Restriction = false;
                //MachineCardVm[1].Restriction = false;
                //MachineCardVm[2].Restriction = false;

            }
            var disp = Application.Current?.Dispatcher;
            if (disp != null && !disp.CheckAccess()) disp.Invoke(build);
            else build();
        }

        private static MachineCardViewModel MapMachinesDto(MachineModel m, ProductionLinesViewModel parent)
        {
            // 使用帶 parent 的建構函式以保留功能
            var vm = new MachineCardViewModel(parent)
            {
                MachineName = m.MachineName,
                Status = m.Status,
                Type = MapToMachineType(m.MachineName),
                Restriction = m.Restriction
            };
            return vm;
        }
        private static MachineType MapToMachineType(string? MachineName)
        {
            if (string.IsNullOrWhiteSpace(MachineName))
                return MachineType.EDM;

            if (string.Equals(MachineName, "EDM2", StringComparison.OrdinalIgnoreCase))
                return MachineType.ESD;

            if (MachineName.IndexOf("EDM", StringComparison.OrdinalIgnoreCase) != -1)
                return MachineType.EDM; // default value
            else if (MachineName.IndexOf("FanucCNC", StringComparison.OrdinalIgnoreCase) != -1)
                return MachineType.FanucCNC;
            else if (MachineName.IndexOf("SiemensCNC", StringComparison.OrdinalIgnoreCase) != -1)
                return MachineType.SiemensCNC;
            else if (MachineName.IndexOf("CMM", StringComparison.OrdinalIgnoreCase) != -1)
                return MachineType.CMM;
            else return MachineType.EDM;
        }
        [RelayCommand]
        private void ToggleExpand()
        {
            _parent.ShowMachineDetail();
        }
        [RelayCommand]
        private void OpenMachineWindow(object? machine)   // machine 建議是 MachineCardViewModel
        {
            if (machine is not MachineCardViewModel card)
                return;

            _parent.OpenMachineWindow(card);
        }
    }

    public partial class MachineCardViewModel : ObservableObject
    {
        private readonly ProductionLinesViewModel? _parent;
        public MachineCardViewModel() { }
        public MachineCardViewModel(ProductionLinesViewModel parent) { _parent = parent; }

        [ObservableProperty] private string machineName = ""; //設備名稱
        [ObservableProperty] private string manufacturer = ""; //製造商
        [ObservableProperty] private string status = "";  
        [ObservableProperty] private MachineType type = MachineType.EDM; //設備類型
        [ObservableProperty] private bool restriction;  //設備鎖定 保留
        //設備資訊
        [ObservableProperty] private string mainProgramName = "";
        [ObservableProperty] private string cycleTime = "";
        public string machineModel { get; set; } = "";// 機台型號

        public string onDeckElectrodeSerial { get; set; } = "";// 夾持中電極標籤序號（RFID）
        public string onDeckWorkpieceSerial { get; set; } = "";// 夾持中工件標籤序號（RFID）
        public string onDeckWorksheetSerial { get; set; } = "";// 當前工單號/序號  

        [ObservableProperty] private string electrodeName = "";
        [ObservableProperty] private string workpieceName = "";

        partial void OnElectrodeNameChanged(string value) => OnPropertyChanged(nameof(ElectrodeShortName));
        partial void OnWorkpieceNameChanged(string value) => OnPropertyChanged(nameof(WorkpieceShortName));

        // Short display versions used by the card UI (truncated with ellipsis)
        public string WorkpieceShortName
        {
            get
            {
                var name = WorkpieceName ?? string.Empty;
                if (string.IsNullOrEmpty(name)) return string.Empty;
                return name.Length > 10 ? name.Substring(0, 10) + "…" : name;
            }
        }

        public string ElectrodeShortName
        {
            get
            {
                var name = ElectrodeName ?? string.Empty;
                if (string.IsNullOrEmpty(name)) return string.Empty;
                return name.Length > 10 ? name.Substring(0, 10) + "…" : name;
            }
        }

        //由Status決定顏色
        public Brush StatusBrush => Status switch
        {
            "Running" => new SolidColorBrush(Color.FromRgb(0x56, 0xC0, 0x6C)), //綠色 1
            "Stopping" => new SolidColorBrush(Color.FromRgb(0xE6, 0xB9, 0x3E)), //黃色 2
            "EmergencyStop" => new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B)), //紅色 3
            _ => new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)), //灰色 
        };
        partial void OnStatusChanged(string value) => OnPropertyChanged(nameof(StatusBrush));
        public string MachineTypeName => Type.ToString(); // ✅ 圖片綁定使用的字串（自動從 enum 轉成檔名）


        [RelayCommand]
        private void ShowElectrodeDetail()
        {
            //onDeckElectrodeSerial = "3"; // for test
            if (_parent == null) return;
            _parent.OpenMaterialInformationBySerialAsync(onDeckElectrodeSerial, MaterialType.Electrode);
        }

        [RelayCommand]
        private void ShowWorkpieceDetail()
        {
            // onDeckWorkpieceSerial = "31"; // for test
            if (_parent == null) return;
            _parent.OpenMaterialInformationBySerialAsync(onDeckWorkpieceSerial, MaterialType.Workpiece);
        }
    }
    public enum MachineType
    {
        EDM = 0,
        ESD = 1,
        ZNC = 2,
        CNC = 3,
        SiemensCNC = 4,
        FanucCNC = 5,
        CMM = 6,
        Null = 99
    }

}
