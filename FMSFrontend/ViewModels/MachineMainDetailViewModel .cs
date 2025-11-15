using CommunityToolkit.Mvvm.ComponentModel;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Services.FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using MahApps.Metro.Controls;
using System.Collections.ObjectModel;

namespace FMSFrontend.ViewModels
{
    public partial class MachineMainDetailViewModel : ObservableObject
    {
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

        [ObservableProperty]
        private ObservableCollection<TabItemModel> tabs;
        [ObservableProperty]
        private ObservableCollection<TabItemModel> tabsPosition;
        [ObservableProperty]
        private ObservableCollection<TabItemModel> tabsParameter;
        [ObservableProperty]
        private ObservableCollection<TabItemModel> tabsWorkOrder;

        [ObservableProperty]
        private int selectedTabIndex;
        [ObservableProperty]
        private int selectedTabIndexPosition;
        [ObservableProperty]
        private int selectedTabIndexParameter;
        [ObservableProperty]
        private int selectedTabIndexWorkOrder;

        [ObservableProperty]
        private MachineOverviewCard selectedMachine;

        public MachineOverviewCard Machine { get; }

        public string MachineImagePath => Machine.MachineImagePath;

        public string MachineName => Machine.MachineName;
        [ObservableProperty]
        private int machineInfoTabControlSelectedIndex;

        [ObservableProperty]
        private MachineDisplayData displayData;

        public List<string> FilterOptions { get; } = new() { "今天", "過去3天", "本月", "自訂" };

        private string _selectedFilterOption = "今天";
        public string SelectedFilterOption
        {
            get => _selectedFilterOption;
            set
            {
                SetProperty(ref _selectedFilterOption, value);
                ApplyDateFilter();
            }
        }

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                SetProperty(ref _selectedDate, value);
                if (SelectedFilterOption == "自訂")
                    ApplyDateFilter();
            }
        }

        private void ApplyDateFilter()
        {
            DateTime fromDate = DateTime.Today;

            switch (SelectedFilterOption)
            {
                case "今天":
                    fromDate = DateTime.Today;
                    break;
                case "過去3天":
                    fromDate = DateTime.Today.AddDays(-2);
                    break;
                case "本月":
                    fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    break;
                case "自訂":
                    fromDate = SelectedDate;
                    break;
            }

            // 依照 fromDate 篩選資料
           // FilterWorkOrderList(fromDate);
        }


        // ✅ 實際使用的建構式
        public MachineMainDetailViewModel(MachineOverviewCard machine)
        {
            Machine = machine;

            SelectedMachine = machine; //Allen 加入

            Tabs = new ObservableCollection<TabItemModel>
            {
                new TabItemModel { Header = "1", TagColor = "#2779A7" },
                new TabItemModel { Header = "2", TagColor = "#2779A7" }
            };
            TabsPosition = new ObservableCollection<TabItemModel>
            {
                new TabItemModel { Header = "絕對", TagColor = "#2779A7" },
                new TabItemModel { Header = "機械", TagColor = "#2779A7" }
            };
            tabsParameter = new ObservableCollection<TabItemModel>
            {
                new TabItemModel { Header = "1", TagColor = "#2779A7" },
                new TabItemModel { Header = "2", TagColor = "#2779A7" }
            };
            tabsWorkOrder = new ObservableCollection<TabItemModel>
            {
                new TabItemModel { Header = "工單資訊", TagColor = "#2779A7" },
                new TabItemModel { Header = "工單Timeline", TagColor = "#2779A7" }
            };

            // 模擬資料（你也可以接實際資料）
            DisplayData = new MachineDisplayData
            {
                MachineNumber = "1",
                MachineStatus = "Disconnection",
                UsingElectrode = "E-03",
                MachiningCode = "EDM101",
                MachiningWorkingTime = "02:35:20",
                MachiningWorkingPercentage = "45%",
                CurrentWorksheet = "WS20250717",
                MachiningTool = "T-01",

                MachineTemperature = "38°C",
                SpindleRPM = "1200 RPM",
                OilLevelStatus = "正常",
                CoolantLevel = "75%",

                PositionID = "1",
                ABS_X = "1886.6",
                ABS_Y = "186.6",
                ABS_Z = "176.6",
                ABS_A = "183.0",

                ABS_B = "18.69",
                ABS_C = "186",
                MCH_X = "6886",
                MCH_Y = "1874.6",
                MCH_Z = "1456.6",

                Speed = "3000",
                Servo = "5",
                Gap = "0.25",
                OB = "0.03",
                E_SPD = "100",
                Pol = "POS",
                Pulse = "50",

                E_Cod = "A2",
                T_ON = "25ms",
                T_OFF = "5ms",
                LV = "120V",
                HV = "210V",
                JT = "12",
                JD = "3"
            };



            selectedTabIndex = 0;  selectedTabIndexPosition = 0; selectedTabIndexParameter = 0; selectedTabIndexWorkOrder = 1;
        }

        // ✅ 設計模式用的無參數建構式
        public MachineMainDetailViewModel() : this(new MachineOverviewCard
        {
            MachineName = "設計模式 EDM-XX",
            Status = "Running",
            Type = MachineType.EDM
        })
        {
        }

        public class TabItemModel
        {
            public override string ToString() => Header;
            public string Header { get; set; } = "";
            public string TagColor { get; set; } = ""; // 對應 TabItem 的 Tag 屬性
        }

        public partial class MachineDisplayData : ObservableObject
        {
            //機台資訊
            [ObservableProperty] private string machineNumber = "";
            [ObservableProperty] private string machineStatus = "";
            [ObservableProperty] private string usingElectrode = "";
            [ObservableProperty] private string machiningCode = "";
            [ObservableProperty] private string machiningWorkingTime = "";
            [ObservableProperty] private string machiningWorkingPercentage = "";
            [ObservableProperty] private string currentWorksheet = "";
            [ObservableProperty] private string machiningTool = "";

            [ObservableProperty] private string machineTemperature = "";
            [ObservableProperty] private string spindleRPM = "";
            [ObservableProperty] private string oilLevelStatus = "";
            [ObservableProperty] private string coolantLevel = "";

            //座標
            [ObservableProperty] private string positionID = "";
            [ObservableProperty] private string aBS_X = "";
            [ObservableProperty] private string aBS_Y = "";
            [ObservableProperty] private string aBS_Z = "";
            [ObservableProperty] private string aBS_C = "";
            [ObservableProperty] private string aBS_A = "";
            [ObservableProperty] private string aBS_B = "";

            [ObservableProperty] private string mCH_X = "";
            [ObservableProperty] private string mCH_Y = "";
            [ObservableProperty] private string mCH_Z = "";

            // 加工參數
            [ObservableProperty] private string speed = "";
            [ObservableProperty] private string servo = "";
            [ObservableProperty] private string gap = "";
            [ObservableProperty] private string oB = "";
            [ObservableProperty] private string e_SPD = "";
            [ObservableProperty] private string pol = "";
            [ObservableProperty] private string pulse = "";

            [ObservableProperty] private string e_Cod = "";
            [ObservableProperty] private string t_ON = "";
            [ObservableProperty] private string t_OFF = "";
            [ObservableProperty] private string lV = "";
            [ObservableProperty] private string hV = "";
            [ObservableProperty] private string jT = "";
            [ObservableProperty] private string jD = "";

        }

    }
}
