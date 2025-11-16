using CommunityToolkit.Mvvm.ComponentModel;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Services.FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Models;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views;
using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using System.Reflection.PortableExecutable;
using System.Security.Policy;
using System.Windows.Threading;

namespace FMSFrontend.ViewModels
{
    public partial class MachineMainDetailViewModel : ObservableObject
    {
        // === Services ===
        private readonly IWorksheetsService _WorksheetService;
        // === Singleton ===
        private readonly MachineStore _machineStore;
        public ObservableCollection<MachineModel> MachinesT => _machineStore.Machines;

       
        DispatcherTimer _timer;
        public ObservableCollection<WorkOrderRow> WorkOrders { get; } = new();

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

        //[ObservableProperty] private MachineOverviewCard selectedMachine;


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
            //FilterWorkOrderList(fromDate);
        }
        private async Task RefreshFetch()
        {
            //try { 
            List<WorksheetsTimelineDto>? WorksheetsTimelineDtos =
                await _WorksheetService.GetWorksheetTimelineByDateTimeAsync(DateTime.Today, _selectedDate);
            if (WorksheetsTimelineDtos != null)
            {
                WorkOrders.Clear();
                foreach (var w in WorksheetsTimelineDtos)
                {
                    if (w.EDMnumber == Machine.MachineName )
                    {
                        WorkOrders.Add(new WorkOrderRow
                        {
                            CreatTime = w.TimeStampe.ToLongDateString() ?? "",
                            WorksheetNumber = w.WorkSheetSerial ?? "",
                            WorkStatus = w.WorkCommand,
                            WorkpieceName = "" // 代定義
                        });
                    }
                }
            }
            //} Catch{}
        }

        // ✅ 實際使用的建構式
        public MachineMainDetailViewModel(MachineOverviewCard machine,
            IWorksheetsService worksheetsService,
            MachineStore machineStore)
        {
            Machine = machine;
            //SelectedMachine = machine; //Allen 加入
            _machineStore = machineStore;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _timer.Tick += (_, __) => RefreshFromStore();
            _timer.Start();

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

            DisplayData = new MachineDisplayData();

            selectedTabIndex = 0; selectedTabIndexPosition = 0; selectedTabIndexParameter = 0; selectedTabIndexWorkOrder = 1;
        }

        private void RefreshFromStore()
        {
            if (Machine == null || DisplayData == null) return;
            // 從MachinesT取得對應Machine名稱一樣的的機台資料顯示在DisplayData上
            var edm = MachinesT.FirstOrDefault(m => m.MachineName == Machine.MachineName);
            if (edm == null || edm.OscarEdm == null) return;

            // 機台資訊
            DisplayData.MachineNumber = edm.OscarEdm.MachineNumber;
            DisplayData.MachineStatus = edm.OscarEdm.MachineStatus;
            DisplayData.UsingElectrode = edm.OscarEdm.UsingElectrode;
            DisplayData.MachiningCode = edm.OscarEdm.MachiningCode;
            DisplayData.MachiningWorkingTime = edm.OscarEdm.MachiningWorkingTime;
            DisplayData.MachiningWorkingPercentage = edm.OscarEdm.MachiningWorkingPercentage;
            DisplayData.CurrentWorksheet = edm.OscarEdm.CurrentWorksheet;
            DisplayData.MachiningTool = edm.OscarEdm.MachiningTool;

            // 機台狀態
            DisplayData.MachineTemperature = edm.OscarEdm.MachineTemperature;
            DisplayData.SpindleRPM = edm.OscarEdm.SpindleRPM;
            DisplayData.OilLevelStatus = edm.OscarEdm.OilLevelStatus;
            DisplayData.CoolantLevel = edm.OscarEdm.CoolantLevel;

            // 座標
            DisplayData.PositionID = edm.OscarEdm.PositionID;
            DisplayData.ABS_X = edm.OscarEdm.ABS_X;
            DisplayData.ABS_Y = edm.OscarEdm.ABS_Y;
            DisplayData.ABS_Z = edm.OscarEdm.ABS_Z;
            DisplayData.ABS_A = edm.OscarEdm.ABS_A;
            DisplayData.ABS_B = edm.OscarEdm.ABS_B;
            DisplayData.ABS_C = edm.OscarEdm.ABS_C;

            DisplayData.MCH_X = edm.OscarEdm.MCH_X;
            DisplayData.MCH_Y = edm.OscarEdm.MCH_Y;
            DisplayData.MCH_Z = edm.OscarEdm.MCH_Z;

            // 加工參數
            DisplayData.Speed = edm.OscarEdm.Speed;
            DisplayData.Servo = edm.OscarEdm.Servo;
            DisplayData.Gap = edm.OscarEdm.Gap;
            DisplayData.OB = edm.OscarEdm.OB;
            DisplayData.E_SPD = edm.OscarEdm.E_SPD;
            DisplayData.Pol = edm.OscarEdm.Pol;
            DisplayData.Pulse = edm.OscarEdm.Pulse;

            DisplayData.E_Cod = edm.OscarEdm.E_Code;
            DisplayData.T_ON = edm.OscarEdm.T_ON;
            DisplayData.T_OFF = edm.OscarEdm.T_OFF;
            DisplayData.LV = edm.OscarEdm.LV;
            DisplayData.HV = edm.OscarEdm.HV;
            DisplayData.JT = edm.OscarEdm.JT;
            DisplayData.JD = edm.OscarEdm.JD;
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
        public class WorkOrderRow
        {
            public string CreatTime { get; set; } = ""; //代定義
            public string WorksheetNumber { get; set; } = "";
            public string WorkStatus { get; set; } = "";
            public string WorkpieceName { get; set; } = "";
        }
    }
}
