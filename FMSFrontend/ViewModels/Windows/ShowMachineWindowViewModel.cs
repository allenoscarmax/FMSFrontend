using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Interfaces;



// 👇 依你的實際命名空間調整
using FMSFrontend.ViewModels;
using FMSFrontend.ViewModels.Production;
using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FMSFrontend.ViewModels.Windows
{
    public partial class ShowMachineWindowViewModel : ObservableObject
    {
        public ProductionLinesViewModel _parent;

        // === Services ===
        public readonly IWindowService _windowService;
        private readonly IWorksheetsService _worksheetsService;

        [ObservableProperty] private string deviceName = "設備名稱";
        [ObservableProperty] private MachineInfo info = new();
        public ObservableCollection<WorkOrderRow> WorkOrders { get; } = new();
        public ShowMachineWindowViewModel(object? machineCard, ProductionLinesViewModel parent)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));

            _windowService = parent._windowService;
            _worksheetsService = parent._worksheetsService;

            if (machineCard is MachineCardViewModel m)
            {
                DeviceName = m.MachineName ?? "設備名稱";
                Info.MachineTypeName = m.MachineTypeName;       // 給 Converter 用（EDM/CNC/ZNC…）

                Info.EquipmentName = m.MachineName ?? "";       // 0.設備名稱
                Info.EquipmentType = m.Type.ToString();         // 1.設備類型
                Info.ModelNo = m.MachineName ?? "";             // 2.設備型號
                Info.MainProgramName = m.MainProgramName;       // 3.當前程式
                Info.MachineState = m.Status;                   // 4.機台狀態
                Info.CycleTime = m.CycleTime;                   // 5.狀態持續時間
                Info.ElectrodeName = m.ElectrodeName;           // 6.電極名稱
                Info.WorkSheetName = m.onDeckWorksheetSerial;   // 7.工單名稱
                Info.WorkPieceName = m.WorkpieceName;           // 8.電極名稱
            }
            else
            {
                // 若沒帶入卡片 VM，保留預設
                DeviceName = "設備名稱";
            }

        }
        // 日期篩選選項
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
            if (IsCustomDateMode) _= RefreshFetch(); // 若為自訂模式且日期變更，重新抓取
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
            if (IsCustomDateMode) _= RefreshFetch();
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
        private async Task RefreshFetch()
        {
            try {
                List<WorksheetsTimelineDto>? WorksheetsTimelineDtos =
                    await _worksheetsService.GetWorksheetTimelineByDateTimeAsync(DateTime.Today, DateTime.Today);
                if (WorksheetsTimelineDtos != null)
                {
                    WorkOrders.Clear();
                    foreach (var w in WorksheetsTimelineDtos)
                    {
                        if (w.EDMnumber == DeviceName)
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
            catch{ }
        }
        //設定日期End
        [RelayCommand] private void ForceElectrodeOut() { /* TODO */ }
        [RelayCommand] private void ForceWorkOut() { /* TODO */ }
        [RelayCommand] private void CloseWindow(Window? w) => w?.Close();
    }

    public partial class MachineInfo : ObservableObject
    {
        [ObservableProperty] private string machineTypeName = "EDM";
        [ObservableProperty] private string equipmentName = "";
        [ObservableProperty] private string equipmentType = "";
        [ObservableProperty] private string modelNo = "";
        [ObservableProperty] private string mainProgramName = "";
        [ObservableProperty] private string machineState = "";
        [ObservableProperty] private string cycleTime = "";
        [ObservableProperty] private string electrodeName = "";
        [ObservableProperty] private string workSheetName = "";
        [ObservableProperty] private string workPieceName = "";
    }

    public class WorkOrderRow
    {
        public string CreatTime { get; set; } = ""; //代定義
        public string WorksheetNumber { get; set; } = "";
        public string WorkStatus { get; set; } = "";
        public string WorkpieceName { get; set; } = "";
    }
}
