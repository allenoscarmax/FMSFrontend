using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Helpers;
using FMSFrontend.Interfaces;
using FMSFrontend.Services;




// 👇 依你的實際命名空間調整
using FMSFrontend.ViewModels;
using FMSFrontend.ViewModels.Production;
using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FMSFrontend.ViewModels.Windows
{
    public partial class ShowMachineWindowViewModel : ObservableObject
    {
        // === Services ===
        public readonly IWindowService _windowService;
        private readonly IWorksheetsService _worksheetsService;
        private readonly IMachinesService _machinesService;
        private readonly IWorkpieceService _workpieceService;
        private readonly IAuthorizationService _authorizationService;

        [ObservableProperty] private string deviceName = string.Empty;
        [ObservableProperty] private MachineInfo info = new();
        public ObservableCollection<WorkOrderRow> WorkOrders { get; } = new();
        public ShowMachineWindowViewModel(
        IWindowService windowService,
        IWorksheetsService worksheetsService,
        IMachinesService machinesService,
        IAuthorizationService authorizationService,
        IWorkpieceService workpieceService)
        {
            _windowService = windowService;
            _worksheetsService = worksheetsService;
            _machinesService = machinesService;
            _authorizationService = authorizationService;
            _workpieceService = workpieceService;
            SelectedFilterIndex = 0;
            ApplyDateFilter(SelectedFilterIndex);
        }
        /// <summary>
        /// 由 WindowService / 呼叫端注入哪一台機台的卡片。
        /// </summary>
        public void Initialize(MachineCardViewModel m)
        {
            DeviceName = m.MachineName
                ?? LanguageManager.GetString("ShowMachineWindowViewModel_DeviceName_Default", "設備名稱");
            Info.MachineName = m.MachineName ?? "";
            Info.Manufacturer = m.Manufacturer ?? "";
            Info.MachineId = m.machineId ?? "";
            Info.MainProgramName = m.MainProgramName;
            Info.MachineState = m.Status;
            Info.CycleTime = m.CycleTime;
            Info.ElectrodeName = m.ElectrodeName;
            Info.WorkSheetName = m.onDeckWorksheetSerial;
            Info.WorkPieceName = m.WorkpieceName;

            // 如果你之後要在這裡撈工單清單，也可以用 _worksheetsService / _machinesService
            // 去填 WorkOrders
        }
        // 日期篩選選項
        [ObservableProperty] private int selectedFilterIndex = -1;
        [ObservableProperty] private DateTime? fromDate = DateTime.Today;
        [ObservableProperty] private DateTime? toDate = DateTime.Today;
        [ObservableProperty] private bool isCustomDateMode;
        private bool _updatingDate;
        partial void OnSelectedFilterIndexChanged(int value) //選擇
        {
            IsCustomDateMode = value == 2;
            ApplyDateFilter(value); //設定日期
            _ = RefreshFetch();
        }
        private void ApplyDateFilter(int filterIndex)
        {
            _updatingDate = true;
            switch (filterIndex)
            {
                case 0:
                    FromDate = DateTime.Today; ToDate = DateTime.Today; break;
                case 1:
                    FromDate = DateTime.Today.AddDays(-6); ToDate = DateTime.Today; break;
                case 2:
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
                    _windowService.ShowMessage(LanguageManager.GetString("ShowMachineWindow_Message_DateRangeTooLong", "選擇的日期範圍不能超過一個月"));
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
                    _windowService.ShowMessage(LanguageManager.GetString("ShowMachineWindow_Message_DateRangeTooLong", "選擇的日期範圍不能超過一個月"));
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
        private async Task RefreshFetch()
        {
            try
            {
                if (ToDate == null || FromDate == null) return;
                List<WorksheetsTimelineDto>? WorksheetsTimelineDtos =
                    await _worksheetsService.GetWorksheetTimelineByDateTimeAsync(FromDate.Value, ToDate.Value.AddDays(1));
                
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
                                ElectrodeName = w.ElectrodeSerial // 代定義
                            });
                        }
                    }
                }
            }
            catch { }
        }
        //設定日期End
        [RelayCommand] 
        private async Task ForceElectrodeOut() 
        {
            try
            {
                if (!_authorizationService.RequireLoginAndWriteOperation(16))
                {
                    return;
                }
                List<MachinesDto> dtos = await _machinesService.GetAllMachinesAsync() ?? new();
                MachinesDto dto = dtos.FirstOrDefault(x => x.machineName == DeviceName)!;
                dto.onDeckElectrodeSerial = "";
                bool ok = await _machinesService.UpdateMachinesDataAsync(dto);
            }
            catch { }
        }

        [RelayCommand]
        private async Task ForceWorkOut()
        {
            try
            {
                if (!_authorizationService.RequireLoginAndWriteOperation(17))
                {
                    return;
                }
                List<MachinesDto> dtos = await _machinesService.GetAllMachinesAsync() ?? new();
                MachinesDto dto = dtos.FirstOrDefault(x => x.machineName == DeviceName)!;
                dto.onDeckWorkpieceSerial = "";
                bool ok = await _machinesService.UpdateMachinesDataAsync(dto);
            }
            catch { }
        }
        [RelayCommand]
        private async Task ClearWorkSheet()
        {
            try
            {
                if (!_authorizationService.RequireLoginAndWriteOperation(39))
                {
                    return;
                }
                List<MachinesDto> dtos = await _machinesService.GetAllMachinesAsync() ?? new();
                MachinesDto dto = dtos.FirstOrDefault(x => x.machineName == DeviceName)!;
                dto.onDeckWorksheetSerial = "";
                bool ok = await _machinesService.UpdateMachinesDataAsync(dto);
            }
            catch { }
        }

        [RelayCommand] private void CloseWindow(Window? w) => w?.Close();
    }

    public partial class MachineInfo : ObservableObject
    {
        [ObservableProperty] private string machineName = "EDM";
        [ObservableProperty] private string manufacturer = "";
        [ObservableProperty] private string machineId = "";
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
        public string ElectrodeName { get; set; } = "";
    }
}
