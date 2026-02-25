using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Helpers;
using FMSFrontend.Interfaces;
using FMSFrontend.Models;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Factory;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views;
using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using System.Reflection.PortableExecutable;
using System.Runtime.Intrinsics.Arm;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
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
        private readonly IAuthorizationService _authorizationService;
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
        public string SelectMachineName = "";

        [RelayCommand]
        private async Task RestrictionClick() //禁用事件
        {
            try
            {

                var edm = AllMachines.FirstOrDefault(m => m.MachineName == Machine.MachineName);
                if (edm != null)
                {
                    bool canctrl = !edm.OscarEdm.CanControl;
                    if (!_authorizationService.RequireLoginAndWriteOperation(18, " " + edm.MachineName + ": " + canctrl))
                        return;
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
                {
                    if (!_authorizationService.RequireLoginAndWriteOperation(19, ": " + edm.MachineName))
                        return;
                    await _machinesService.ResetDispatchErrorMessageAsync(edm.MachineNumber - 1);
                }
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

        // ===== 日期篩選 =====
        public ObservableCollection<string> DateFilterOptions { get; set; } = new() { "今天", "前7天", "自訂" };
        [ObservableProperty] private string selectedFilterOption = "今天";
        [ObservableProperty] private DateTime? fromDate = DateTime.Today;
        [ObservableProperty] private DateTime? toDate = DateTime.Today;
        [ObservableProperty] private bool isCustomDateMode;

        private bool _updatingDate;
        partial void OnSelectedFilterOptionChanged(string value)
        {
            IsCustomDateMode = value == "自訂";
            ApplyDateFilter();
            _ = RefreshFetch();
        }
        private void ApplyDateFilter()
        {
            _updatingDate = true;
            switch (SelectedFilterOption)
            {
                case "今天":
                    FromDate = DateTime.Today; ToDate = DateTime.Today; break;
                case "前7天":
                    FromDate = DateTime.Today.AddDays(-6); ToDate = DateTime.Today; break;
                case "自訂":
                    FromDate = DateTime.Today.AddMonths(-1); ToDate = DateTime.Today; break;
                default: break; // 保留使用者輸入
            }
            _updatingDate = false;
        }
        partial void OnFromDateChanged(DateTime? value)
        {
            if (_updatingDate || value == null || ToDate == null) return;
            _updatingDate = true;
            try
            {
                var today = DateTime.Today;
                var from = value.Value.Date;
                var to = ToDate.Value.Date;
                //日期邏輯判斷
                if (from > today) from = today;         // 封頂今天
                if (to > today) to = today;             // 封頂今天
                if (from > to) to = from.AddMonths(1);  // 如果開始日大於結束日，調整結束日為開始日加一個月
                if (to > from.AddMonths(1))             // 一個月範圍限制
                {
                    _windowService.ShowMessage(LanguageManager.GetString("MachineMainDetail_Message_DateRangeTooLong", "選擇的日期範圍不能超過一個月"));
                    to = from.AddMonths(1);
                }
                if (to > today) to = today; // 避免被 AddMonths 推到未來
                FromDate = from;
                ToDate = to;
            }
            finally
            {
                _updatingDate = false;
            }
            _ = RefreshFetch(); // 若為自訂模式且日期變更，重新抓取
        }
        partial void OnToDateChanged(DateTime? value)
        {
            if (_updatingDate || value == null || FromDate == null) return;
            _updatingDate = true;
            try
            {
                var today = DateTime.Today;
                var to = value.Value.Date;
                var from = FromDate.Value.Date;
                if (to > today) to = today; // 封頂今天（結束日不能超過今天）
                if (to < from) from = to.AddMonths(-1); // 如果結束日小於開始日，調整開始日為結束日減一個月
                if (from < to.AddMonths(-1)) // 一個月範圍限制
                {
                    _windowService.ShowMessage(LanguageManager.GetString("MachineMainDetail_Message_DateRangeTooLong", "選擇的日期範圍不能超過一個月"));
                    from = to.AddMonths(-1);
                }
                if (from > today) from = today; // 避免 from 被推到未來（理論上不會，但保險）
                FromDate = from;
                ToDate = to;
            }
            finally
            {
                _updatingDate = false;
            }
            _ = RefreshFetch();
        }
        //設定日期End
        private async Task RefreshFetch()
        {

            try
            {
                if (FromDate != null && ToDate != null)
                {
                    List<WorksheetsTimelineDto>? WorksheetsTimelineDtos =
                        await _worksheetService.GetWorksheetTimelineByDateTimeAsync(FromDate.Value, ToDate.Value.AddDays(1));
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
                                    EoectrodeName = w.ElectrodeSerial // 代定義
                                });

                            }
                        }
                    }
                }
                else
                {
                    _windowService.ShowMessage(LanguageManager.GetString("MachineMainDetail_Message_DateFormatError", "日期格式錯誤"));
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
            _authorizationService = parent._authorizationService;

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
                new TabItemModel { Header = LanguageManager.GetString("MachineMainDetail_Tab_Absolute", "絕對"), TagColor = "#2779A7" },
                new TabItemModel { Header = LanguageManager.GetString("MachineMainDetail_Tab_Machine", "機械"), TagColor = "#2779A7" }
            };
            tabsParameter = new ObservableCollection<TabItemModel>
            {
                new TabItemModel { Header = "1", TagColor = "#2779A7" },
                new TabItemModel { Header = "2", TagColor = "#2779A7" }
            };
            tabsWorkOrder = new ObservableCollection<TabItemModel>
            {
                new TabItemModel { Header = LanguageManager.GetString("MachineMainDetail_Tab_WorkOrderInfo", "工單資訊"), TagColor = "#2779A7" },
                new TabItemModel { Header = LanguageManager.GetString("MachineMainDetail_Tab_WorkOrderTimeline", "工單Timeline"), TagColor = "#2779A7" }
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
            if (MachineName.Contains("EDM"))
            {
                // 從MachinesT取得對應Machine名稱一樣的的機台資料顯示在DisplayData上
                var edm = AllMachines.FirstOrDefault(m => m.MachineName == Machine.MachineName);
                if (edm == null || edm.OscarEdm == null) return;
                MachineNumber = int.TryParse(edm.OscarEdm.MachineNumber, out var num) ? num : -1;
                if (canctrlDelay > 0) canctrlDelay--;
                if (canctrlDelay == 0) DisplayData.CanControl = edm.OscarEdm.CanControl;

                // 機台資訊
                DisplayData.MachineInfos[0].Name = LanguageManager.GetString("MachineMainDetail_Info_MachineNumber", "• 機台型號：");
                DisplayData.MachineInfos[0].Value = edm.OscarEdm.MachineNumber;
                DisplayData.MachineInfos[1].Name = LanguageManager.GetString("MachineMainDetail_Info_MachineStatus", "• 機台狀態：");
                DisplayData.MachineInfos[1].Value = edm.OscarEdm.MachineStatus;
                DisplayData.MachineInfos[2].Name = LanguageManager.GetString("MachineMainDetail_Info_UsingElectrode", "• 使用電極：");
                DisplayData.MachineInfos[2].Value = edm.OscarEdm.UsingElectrode;
                DisplayData.MachineInfos[3].Name = LanguageManager.GetString("MachineMainDetail_Info_MachiningCode", "• 加工程式：");
                DisplayData.MachineInfos[3].Value = edm.OscarEdm.MachiningCode;
                DisplayData.MachineInfos[4].Name = LanguageManager.GetString("MachineMainDetail_Info_CycleTime", "• 加工時間：");
                DisplayData.MachineInfos[4].Value = edm.OscarEdm.CycleTime;
                DisplayData.MachineInfos[5].Name = LanguageManager.GetString("MachineMainDetail_Info_MachiningProgress", "• 加工進度：");
                DisplayData.MachineInfos[5].Value = edm.OscarEdm.MachiningWorkingPercentage;
                DisplayData.MachineInfos[6].Name = LanguageManager.GetString("MachineMainDetail_Info_CurrentWorksheet", "• 目前工單：");
                DisplayData.MachineInfos[6].Value = edm.OscarEdm.CurrentWorksheet;
                DisplayData.MachineInfos[7].Name = LanguageManager.GetString("MachineMainDetail_Info_MachiningTool", "• 刀具號碼：");
                DisplayData.MachineInfos[7].Value = edm.OscarEdm.MachiningTool;

                DisplayData.MachineInfos[8].Name = LanguageManager.GetString("MachineMainDetail_Info_MachineTemperature", "• 機台溫度 : ");
                DisplayData.MachineInfos[8].Value = edm.OscarEdm.MachineTemperature;
                DisplayData.MachineInfos[9].Name = LanguageManager.GetString("MachineMainDetail_Info_SpindleRpm", "• 主軸轉速 : ");
                DisplayData.MachineInfos[9].Value = edm.OscarEdm.SpindleRPM;
                DisplayData.MachineInfos[10].Name = LanguageManager.GetString("MachineMainDetail_Info_OilLevel", "• 油位狀態 : ");
                DisplayData.MachineInfos[10].Value = edm.OscarEdm.OilLevelStatus;
                DisplayData.MachineInfos[11].Name = LanguageManager.GetString("MachineMainDetail_Info_CoolantLevel", "• 冷卻液量 : ");
                DisplayData.MachineInfos[11].Value = edm.OscarEdm.CoolantLevel;
                for (int i = 12; i < 16; i++)
                {
                    DisplayData.MachineInfos[i].Name = "";
                    DisplayData.MachineInfos[i].Value = "";
                }

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
                DisplayData.MCH_A = "";
                DisplayData.MCH_B = "";
                DisplayData.MCH_C = "";

                // 加工參數
                DisplayData.ProcessingParam[0].Name = LanguageManager.GetString("MachineMainDetail_Param_Ton", "TON(us):");
                DisplayData.ProcessingParam[0].Value = edm.OscarEdm.T_ON;
                DisplayData.ProcessingParam[1].Name = LanguageManager.GetString("MachineMainDetail_Param_Toff", "TOFF(us):");
                DisplayData.ProcessingParam[1].Value = edm.OscarEdm.T_OFF;
                DisplayData.ProcessingParam[2].Name = LanguageManager.GetString("MachineMainDetail_Param_Current", "I(A):");
                DisplayData.ProcessingParam[2].Value = edm.OscarEdm.E_SPD; //??
                DisplayData.ProcessingParam[3].Name = LanguageManager.GetString("MachineMainDetail_Param_Polarity", "Pol:");
                DisplayData.ProcessingParam[3].Value = edm.OscarEdm.Pol;
                DisplayData.ProcessingParam[4].Name = LanguageManager.GetString("MachineMainDetail_Param_Hv", "Hv:");
                DisplayData.ProcessingParam[4].Value = edm.OscarEdm.HV;
                DisplayData.ProcessingParam[5].Name = LanguageManager.GetString("MachineMainDetail_Param_Gap", "Gap(V):");
                DisplayData.ProcessingParam[5].Value = edm.OscarEdm.Gap;
                DisplayData.ProcessingParam[6].Name = LanguageManager.GetString("MachineMainDetail_Param_Speed", "Speed:");
                DisplayData.ProcessingParam[6].Value = edm.OscarEdm.Speed;
                DisplayData.ProcessingParam[7].Name = LanguageManager.GetString("MachineMainDetail_Param_Pulse", "Pulse");
                DisplayData.ProcessingParam[7].Value = edm.OscarEdm.Pulse;
                DisplayData.ProcessingParam[8].Name = LanguageManager.GetString("MachineMainDetail_Param_Servo", "Servo(%):");
                DisplayData.ProcessingParam[8].Value = edm.OscarEdm.Servo;
                DisplayData.ProcessingParam[9].Name = LanguageManager.GetString("MachineMainDetail_Param_Jd", "JD(mm):");
                DisplayData.ProcessingParam[9].Value = edm.OscarEdm.JD;
                DisplayData.ProcessingParam[10].Name = LanguageManager.GetString("MachineMainDetail_Param_Ob", "OB:");
                DisplayData.ProcessingParam[10].Value = edm.OscarEdm.OB;
                DisplayData.ProcessingParam[11].Name = LanguageManager.GetString("MachineMainDetail_Param_Jt", "JT(s):");
                DisplayData.ProcessingParam[11].Value = edm.OscarEdm.JT;
                for (int i = 12; i < 16; i++)
                {
                    DisplayData.ProcessingParam[i].Name = "";
                    DisplayData.ProcessingParam[i].Value = "";
                }
            }
            else if (MachineName.Contains("UH500"))
            {
                // 從MachinesT取得對應Machine名稱一樣的的機台資料顯示在DisplayData上
                var machine = AllMachines.FirstOrDefault(m => m.MachineName == Machine.MachineName);
                if (machine == null || machine.SunmillSiemensCNC == null) return;
                var cnc = machine.SunmillSiemensCNC;

                MachineNumber = int.TryParse(cnc.MachineNumber, out var num) ? num : -1;
                if (canctrlDelay > 0) canctrlDelay--;
                if (canctrlDelay == 0) DisplayData.CanControl = cnc.CanControl;
                // 機台資訊
                DisplayData.MachineInfos[0].Name = LanguageManager.GetString("MachineMainDetail_Info_MachineNumber", "• 機台型號：");
                DisplayData.MachineInfos[0].Value = cnc.MachineNumber;
                DisplayData.MachineInfos[1].Name = LanguageManager.GetString("MachineMainDetail_Info_MachineStatus", "• 機台狀態：");
                DisplayData.MachineInfos[1].Value = cnc.MachineStatus;
                DisplayData.MachineInfos[2].Name = LanguageManager.GetString("MachineMainDetail_Info_UsingTool", "• 使用刀具：");
                DisplayData.MachineInfos[2].Value = cnc.ToolName;
                DisplayData.MachineInfos[3].Name = LanguageManager.GetString("MachineMainDetail_Info_MachiningCode", "• 加工程式：");
                DisplayData.MachineInfos[3].Value = cnc.MachiningCode;
                DisplayData.MachineInfos[4].Name = LanguageManager.GetString("MachineMainDetail_Info_CycleTime", "• 加工時間：");
                DisplayData.MachineInfos[4].Value = cnc.CycleTime;
                DisplayData.MachineInfos[5].Name = LanguageManager.GetString("MachineMainDetail_Info_MachiningProgress", "• 加工進度：");
                DisplayData.MachineInfos[5].Value = cnc.MachiningWorkingPercentage;
                DisplayData.MachineInfos[6].Name = LanguageManager.GetString("MachineMainDetail_Info_CurrentWorksheet", "• 目前工單：");
                DisplayData.MachineInfos[6].Value = cnc.CurrentWorksheet;
                DisplayData.MachineInfos[7].Name = LanguageManager.GetString("MachineMainDetail_Info_MachiningTool", "• 刀具號碼：");
                DisplayData.MachineInfos[7].Value = cnc.MachiningTool;
                for (int i = 8; i < 16; i++)
                {
                    DisplayData.MachineInfos[i].Name = "";
                    DisplayData.MachineInfos[i].Value = "";
                }
                // 座標
                DisplayData.PositionID = cnc.PositionID;
                DisplayData.ABS_X = cnc.ABS_X;
                DisplayData.ABS_Y = cnc.ABS_Y;
                DisplayData.ABS_Z = cnc.ABS_Z;
                DisplayData.ABS_A = cnc.ABS_A;
                DisplayData.ABS_B = cnc.ABS_B;
                DisplayData.ABS_C = cnc.ABS_C;

                DisplayData.MCH_X = cnc.MCH_X;
                DisplayData.MCH_Y = cnc.MCH_Y;
                DisplayData.MCH_Z = cnc.MCH_Z;
                DisplayData.MCH_A = cnc.MCH_A;
                DisplayData.MCH_B = cnc.MCH_B;
                DisplayData.MCH_C = cnc.MCH_C;

                // 加工參數
                DisplayData.ProcessingParam[0].Name = LanguageManager.GetString("MachineMainDetail_Param_FeedRate", "進給速度");
                DisplayData.ProcessingParam[0].Value = cnc.FeedRate;
                DisplayData.ProcessingParam[1].Name = LanguageManager.GetString("MachineMainDetail_Param_SpindleSpeed", "主軸轉速");
                DisplayData.ProcessingParam[1].Value = cnc.SpindleSpeed;
                for (int i = 2; i < 16; i++)
                {
                    DisplayData.ProcessingParam[i].Name = "";
                    DisplayData.ProcessingParam[i].Value = "";
                }
            }
        }

        public class TabItemModel
        {
            public override string ToString() => Header;
            public string Header { get; set; } = "";
            public string TagColor { get; set; } = ""; // 對應 TabItem 的 Tag 屬性
        }

        public partial class MachineDisplayData : ObservableObject
        {
            public MachineDisplayData()
            {
                for (int i = 0; i < 16; i++)
                {
                    ProcessingParam.Add(new ProcessingParam());
                    MachineInfos.Add(new MachineInfo());
                } 
            }
            //機台禁用
            [ObservableProperty] private bool canControl = false;
            //機台資訊
            [ObservableProperty] private string machineNumber = "";  //機台號碼
            [ObservableProperty] private string machineStatus = "";  //機台狀態
            [ObservableProperty] private string usingElectrode = ""; //使用電極(刀具)
            [ObservableProperty] private string machiningCode = "";  //加工程式
            [ObservableProperty] private string machiningWorkingTime = ""; //加工持續時間
            [ObservableProperty] private string machiningWorkingPercentage = ""; //加工進度
            [ObservableProperty] private string currentWorksheet = ""; //工單編號
            [ObservableProperty] private string machiningTool = ""; //使用刀具

            [ObservableProperty] private string machineTemperature = ""; //機台溫度
            [ObservableProperty] private string spindleRPM = ""; //主軸轉速
            [ObservableProperty] private string oilLevelStatus = ""; //油位狀態
            [ObservableProperty] private string coolantLevel = ""; //冷卻液位
                                                                   //機台資訊
            public ObservableCollection<MachineInfo> MachineInfos { get; set; } = new ObservableCollection<MachineInfo>();

            // 加工參數
            public ObservableCollection<ProcessingParam> ProcessingParam { get; set; } = new ObservableCollection<ProcessingParam>();

            //座標
            [ObservableProperty] private string positionID = ""; //坐標系
            [ObservableProperty] private string aBS_X = "";
            [ObservableProperty] private string aBS_Y = "";
            [ObservableProperty] private string aBS_Z = "";
            [ObservableProperty] private string aBS_W = "";
            [ObservableProperty] private string aBS_C = "";
            [ObservableProperty] private string aBS_A = "";
            [ObservableProperty] private string aBS_B = "";

            [ObservableProperty] private string mCH_X = "";
            [ObservableProperty] private string mCH_Y = "";
            [ObservableProperty] private string mCH_Z = "";
            [ObservableProperty] private string mCH_W = "";
            [ObservableProperty] private string mCH_C = "";
            [ObservableProperty] private string mCH_A = "";
            [ObservableProperty] private string mCH_B = "";
        }
        public partial class MachineInfo : ObservableObject
        {
            [ObservableProperty] private string name = "";
            [ObservableProperty] private string value = "";
        }
        public partial class ProcessingParam : ObservableObject
        {
            [ObservableProperty] private string name = "";
            [ObservableProperty] private string value = "";
        }
        public class WorkOrderRow
        {
            public string CreatTime { get; set; } = ""; //代定義
            public string WorksheetNumber { get; set; } = "";
            public string WorkStatus { get; set; } = "";
            public string EoectrodeName { get; set; } = "";
        }
    }
}
