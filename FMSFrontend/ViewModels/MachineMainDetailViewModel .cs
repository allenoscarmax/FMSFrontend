using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Services.FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Interfaces;
using FMSFrontend.Models;
using FMSFrontend.ViewModels.Factory;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views;
using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using System.Reflection.PortableExecutable;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FMSFrontend.ViewModels
{
    public partial class MachineMainDetailViewModel : ObservableObject
    {
        //private readonly MachineOverviewMainViewModel _parent;
        // === Services ===
        private readonly IWindowService _windowService;
        private readonly IWorksheetsService _worksheetService;
        private readonly IMachinesService _machinesService;

        // === Singleton ===
        private readonly MachineStore _machineStore;
        public ObservableCollection<MachineModel> AllMachines => _machineStore.Machines;
        DispatcherTimer _timer;

        //機台資訊
        [ObservableProperty] private ObservableCollection<TabItemModel> tabs;           // 機台資訊Tab
        [ObservableProperty] private ObservableCollection<TabItemModel> tabsPosition;   // 工作座標Tab
        [ObservableProperty] private ObservableCollection<TabItemModel> tabsParameter;  // 加工參數Tab
        [ObservableProperty] private ObservableCollection<TabItemModel> tabsWorkOrder;  // 工單資訊Tab
        [ObservableProperty] private int selectedTabIndex;                              // 機台資訊 Tab選擇
        [ObservableProperty] private int selectedTabIndexPosition;                      // 工作座標 Tab選擇
        [ObservableProperty] private int selectedTabIndexParameter;                     // 加工參數 Tab選擇
        [ObservableProperty] private int selectedTabIndexWorkOrder;                     // 工單資訊 Tab選擇
        [ObservableProperty] private MachineDisplayData displayData; //顯示機台詳細資訊

        public int MachineNumber = 0;
        public string  SelectMachineName = "";

        [RelayCommand]
        private async Task RestrictionClick() //禁用事件
        {
            try
            {
                var edm = AllMachines.FirstOrDefault(m => m.MachineName == Machine.MachineName);
                if (edm != null)
                {
                    bool canctrl = !edm.OscarEdm.CanControl;
                    await _machinesService.SetMachineCanControlAsync(edm.MachineNumber - 1, canctrl);
                    canctrlDelay = 5;
                }
            }
            catch { }
        }
        [RelayCommand]
        private async Task ResetClick() //重置事件
        {
            try
            {
                var edm = AllMachines.FirstOrDefault(m => m.MachineName == Machine.MachineName);
                if (edm != null)
                    await _machinesService.ResetDispatchErrorMessageAsync(edm.MachineNumber - 1);
            }
            catch { }
                
        }
        //[ObservableProperty] private MachineOverviewCard selectedMachine;
        [ObservableProperty] private int machineInfoTabControlSelectedIndex; //機台資訊TabControl選擇索引 
        public MachineOverviewCard Machine { get; } //由machineoverview傳入選中的卡片
        public string MachineImagePath => Machine.MachineImagePath; //機台圖片路徑
        public string MachineName => Machine.MachineName; //機台名稱

        

        //工單資訊
        public ObservableCollection<WorkOrderRow> WorkOrders { get; } = new();
        public List<string> DateFilterOptions { get; set; } = new() { "今天", "前7天", "自訂" };
        public bool IsCustomDateMode => SelectedFilterOption == "自訂";
        [ObservableProperty] private string selectedFilterOption = "今天";
        [ObservableProperty] private int selectedFilterIndex = 0;

        [ObservableProperty] private DateTime? fromDate = DateTime.Today;
        [ObservableProperty] private DateTime? toDate = DateTime.Today;

        private DateTime? lastValidFromDate = DateTime.Today;
        private DateTime? lastValidToDate = DateTime.Today;
        partial void OnSelectedFilterOptionChanged(string value)
        {
            OnPropertyChanged(nameof(IsCustomDateMode));
            ApplyDateFilter();
            _ = RefreshFetch();
        }
        partial void OnSelectedFilterIndexChanged(int value)
        {
            // 當以 index 選擇時，轉成對應的選項文字，讓現有的文字處理流程負責套用與抓取
            if (value >= 0 && value < DateFilterOptions.Count)
            {
                SelectedFilterOption = DateFilterOptions[value];
            }
        }
        partial void OnFromDateChanged(DateTime? value)
        {
            if (value == null || ToDate == null)
            {
                lastValidFromDate = value;
                return;
            }
            if (value > ToDate)
            {
                _windowService.ShowMessage("開始日期不能大於結束日期");
                FromDate = lastValidFromDate;
                return;
            }
            if ((ToDate - value)?.TotalDays > 31)
            {
                _windowService.ShowMessage("選擇的日期範圍不能超過一個月");
                FromDate = lastValidFromDate;
                return;
            }
            lastValidFromDate = value;
            if (IsCustomDateMode) _ = RefreshFetch(); // 若為自訂模式且日期變更，重新抓取
        }
        partial void OnToDateChanged(DateTime? value)
        {
            if (value == null || FromDate == null)
            {
                lastValidToDate = value;
                return;
            }

            if (value < FromDate)
            {
                _windowService.ShowMessage("結束日期不能小於開始日期");
                ToDate = lastValidToDate;
                return;
            }

            if ((value - FromDate)?.TotalDays > 31)
            {
                _windowService.ShowMessage("選擇的日期範圍不能超過一個月");
                ToDate = lastValidToDate;
                return;
            }

            lastValidToDate = value;
            // 若為自訂模式且日期變更，重新抓取
            if (IsCustomDateMode) _ = RefreshFetch();
        }

        private void ApplyDateFilter()
        {
            switch (SelectedFilterOption)
            {
                case "今天":
                    ToDate = DateTime.Today;
                    FromDate = DateTime.Today;
                    break;
                case "前7天":
                    ToDate = DateTime.Today;
                    FromDate = DateTime.Today.AddDays(-6); // 包含今天一共7天
                    break;
                case "自訂":
                default:
                    break;
            }
        }
        //設定日期End
        private async Task RefreshFetch()
        {
            try {
                if (FromDate != null && ToDate != null)
                {
                    List<WorksheetsTimelineDto>? WorksheetsTimelineDtos =
                        await _worksheetService.GetWorksheetTimelineByDateTimeAsync(FromDate.Value, ToDate.Value);
                    WorkOrders.Clear();
                    if (WorksheetsTimelineDtos != null)
                    {
                        foreach (var w in WorksheetsTimelineDtos)
                        {
                            if (w.EDMnumber == Machine.MachineName)
                            {
                                WorkOrders.Add(new WorkOrderRow
                                {
                                    CreatTime = w.TimeStampe.ToString("yyyy/MM/dd") ?? "",
                                    WorksheetNumber = w.WorkSheetSerial ?? "",
                                    WorkStatus = w.WorkCommand,
                                    WorkpieceName = "" // 代定義
                                });

                            }
                        }
                    }
                }
                else
                {
                    _windowService.ShowMessage("日期格式錯誤");
                }
            } 
            catch
            {
            }
        }

        // ✅ 實際使用的建構式
        public MachineMainDetailViewModel(MachineOverviewCard machine, MachineOverviewMainViewModel parent)
        {
            Machine = machine;
            _windowService = parent._windowService;
            _machineStore = parent._machineStore;
            _worksheetService = parent._worksheetsService;
            _machinesService = parent._machinesService;

           

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
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
        public void OnPageActivated()
        {
            
        }
        public void OnPageDeactivated()
        {
            _timer.Stop();
            _timer = null!;
        }
        bool test = false;
       public int canctrlDelay = 3;
        private void RefreshFromStore()
        {
            if (Machine == null || DisplayData == null) return;
            // 從MachinesT取得對應Machine名稱一樣的的機台資料顯示在DisplayData上
            var edm = AllMachines.FirstOrDefault(m => m.MachineName == Machine.MachineName);
            if (edm == null || edm.OscarEdm == null) return;
            MachineNumber = int.TryParse(edm.OscarEdm.MachineNumber, out var num) ? num : -1;
            test = !test;
            if(canctrlDelay>0) canctrlDelay--;
            if (canctrlDelay == 0) DisplayData.CanControl = edm.OscarEdm.CanControl;
            // 機台資訊
            DisplayData.MachineNumber = edm.OscarEdm.MachineNumber;
            DisplayData.MachineStatus = edm.OscarEdm.MachineStatus;
            DisplayData.UsingElectrode = edm.OscarEdm.UsingElectrode;
            DisplayData.MachiningCode = edm.OscarEdm.MachiningCode;
            DisplayData.MachiningWorkingTime = edm.OscarEdm.CycleTime;
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
            //機台禁用
            [ObservableProperty] private bool canControl = false; 
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
