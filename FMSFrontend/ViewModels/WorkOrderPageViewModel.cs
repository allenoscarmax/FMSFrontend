using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging; 
using　CommunityToolkit.Mvvm.Messaging.Messages;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Dtos.Apps;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Services.FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Interfaces;
using FMSFrontend.Models;
using FMSFrontend.Services;
using FMSFrontend.Views;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace FMSFrontend.ViewModels
{
    public partial class WorkOrderPageViewModel : ObservableObject
    {
        private readonly IWindowService _WindowService;
        private readonly IWorkpieceService _WorkpieceService;
        private readonly IProbeService _ProbeService;
        private readonly IElectrodeService _ElectrodeService;
        private readonly IWorksheetsService _WorksheetsService;
        private readonly IAuthorizationService _auth;
        private readonly IWorksheetAppService _worksheetAppService;
        private readonly RobotStore _robotStore;

        private CancellationTokenSource? _currentUpdateCts; // 取消目前更新的 CancellationTokenSource

        private Robot _robot => _robotStore.Robot;

        [ObservableProperty]
        private int edmSelectedTab;

        [ObservableProperty]
        private WorkOrderData? selectedEDMQueueItem;

        partial void OnEdmSelectedTabChanged(int value)
        {
            // 非 Processing(2) 時，清空選取，避免殘留造成誤判
            if (value != 2)
                SelectedEDMQueueItem = null;

            ReviseCommand.NotifyCanExecuteChanged();
        }
        partial void OnSelectedEDMQueueItemChanged(WorkOrderData? value)
        {
        //    System.Diagnostics.Debug.WriteLine($"SelectedEDMQueueItemChanged: {(value == null ? "null" : value.WorksheetNumber)}");
            ReviseCommand.NotifyCanExecuteChanged();
        }
        private bool CanRevise()
    => EDMSelectedTab == 2 && SelectedEDMQueueItem != null;

        [ObservableProperty]
        private int selectedTabIndexParameter;
        partial void OnSelectedTabIndexParameterChanged(int value)
        {
            _ = FetchAndBindByStatusAsync();
        }

        [ObservableProperty] private int eDMSelectedTab; // 決定 EDM 子頁籤

        partial void OnEDMSelectedTabChanged(int value)
        {
           
            _ = FetchAndBindByStatusAsync();
        }
        public List<string> DateFilterOptions { get; set; } = new() { "今天", "過去7天", "自訂" };
        [ObservableProperty]
        private string selectedFilterOption = "今天";

        private DateTime? lastValidFromDate = DateTime.Today;
        private DateTime? lastValidToDate = DateTime.Today;

        partial void OnSelectedFilterOptionChanged(string value)
        {
            OnPropertyChanged(nameof(IsCustomDateMode));
            ApplyDateFilter();
            // 切換日期篩選選項後立即重新抓資料
            _ = FetchAndBindByStatusAsync();
        }
        [ObservableProperty]
        private DateTime? fromDate = DateTime.Today;

        partial void OnFromDateChanged(DateTime? value)
        {
            if (value == null || ToDate == null)
            {
                lastValidFromDate = value;
                return;
            }

            if (value > ToDate)
            {
                _WindowService.ShowMessage("開始日期不能大於結束日期");
                FromDate = lastValidFromDate;
                return;
            }

            if ((ToDate - value)?.TotalDays > 31)
            {
                _WindowService.ShowMessage("選擇的日期範圍不能超過一個月");
                FromDate = lastValidFromDate;
                return;
            }

            lastValidFromDate = value;

            // 自訂日期變更後立即重新抓資料
            if (IsCustomDateMode)
                _ = FetchAndBindByStatusAsync();
        }

        [ObservableProperty]
        private DateTime? toDate = DateTime.Today;
        public void OnPageActivated()
        {
            var status = MapWorkStatusForParameterTab(SelectedTabIndexParameter);

            _ = FetchAndBindByStatusAsync();
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
                ShowWarning("結束日期不能小於開始日期");
                ToDate = lastValidToDate;
                return;
            }

            if ((value - FromDate)?.TotalDays > 31)
            {
                ShowWarning("選擇的日期範圍不能超過一個月");
                ToDate = lastValidToDate;
                return;
            }
            lastValidToDate = value;

            // 自訂日期變更後立即重新抓資料
            if (IsCustomDateMode)
                _ = FetchAndBindByStatusAsync();
        }
        public bool IsCustomDateMode => SelectedFilterOption == "自訂";
        public ObservableCollection<WorkOrderData> WorkOrderList { get; set; } = new();
        public ObservableCollection<WorkOrderData> FailureWorkOrders { get; set; } = new();
        public ObservableCollection<WorkOrderData> FilteredEDMList { get; } = new();
        private void ApplyDateFilter()
        {
            switch (SelectedFilterOption)
            {
                case "今天":
                    ToDate = DateTime.Today;
                    FromDate = DateTime.Today;
                    break;
                case "過去7天":
                    ToDate = DateTime.Today;
                    FromDate = DateTime.Today.AddDays(-6); // 包含今天一共7天

                    break;
                case "自訂":
                default:
                    break;
            }
            // 非自訂範圍（今天/過去7天）更新後也要重新抓資料
            if (!IsCustomDateMode)
                _ = FetchAndBindByStatusAsync();
        }
        public ICommand DeleteCommand { get; }

        public WorkOrderPageViewModel(IWindowService windowService,
            IElectrodeService electrodeService,
            IWorkpieceService workpieceService,
            IProbeService probeService,
            IStorageService storageService,
            IMachinesService machinesService,
            IWorksheetsService worksheetsService,
            IAuthorizationService auth,
            IWorksheetAppService worksheetAppService,
            RobotStore robotStore,
            MachineLiveUpdater machineLiveUpdater)
        {
            _WindowService = windowService;
            _WorkpieceService = workpieceService;
            _ProbeService = probeService;
            _ElectrodeService = electrodeService;
            _WorksheetsService = worksheetsService;
            _worksheetAppService = worksheetAppService;
            _auth = auth;
            _robotStore = robotStore;

            // 使用非同步方法刪除：保留 RelayCommand，但在內部啟動 async Task
            DeleteCommand = new RelayCommand<WorkOrderData>(item =>
                {
                    if (item != null)
                    {
                        _ = DeleteWorkOrderAsync(item);
                        
                    }
                });
            //_ = FetchAndBindByStatusAsync();
        }

        private void ShowWarning(string message)
        {
            FMSFrontend.Extensions.DialogMessageWindow dd = new Extensions.DialogMessageWindow(message);
            dd.Show();
        }
        [RelayCommand]
        private void OpenUploadSheet()
        {
            if (!_auth.RequireLogin())
                return;

            _WindowService.ShowUploadSheetWindow();

            _ = FetchAndBindByStatusAsync();
        }

        [RelayCommand(CanExecute = nameof(CanRevise))]
        private async Task Revise()
        {
            if (!_auth.RequireLogin()) return;

            if (_robot.DispatchEnabled || _robot.IsStarted)
            {
                _WindowService.ShowMessage("請先取消機器人啟動狀態與關閉派工功能後，才能進行流程修正。");
                return;
            }
            if (SelectedEDMQueueItem is null)
            {
                _WindowService.ShowMessage("請先選擇一筆工單。");
                return;
            }

            var action = _WindowService.ShowReviseWindow();    

            if (action is null or ReviseProcessAction.Cancel) return;

            var item = SelectedEDMQueueItem;
            if (item is null) return; // 防競態

            ReviseWorksheetResultDto result;
            try
            {
                result = await _worksheetAppService.Revise(new ReviseWorksheetRequestDto
                {
                    worksheetNumber = item.WorksheetNumber,
                    action = (Features.Dtos.Apps.ReviseProcessAction)action,
                    setupUser = "admin",
                });
            }
            catch (Exception ex)
            {
                _WindowService.ShowMessage("後端連線失敗：" + ex.Message);
                return;
            }
            if (!result.success)
            {
                _WindowService.ShowMessage(result.message);
                return;
            }

            _WindowService.ShowMessage(result.message);
            _WindowService.ShowMessage("請記得手動將電極從機台上移除並重新開啟遠端模式");
        }

        // 最小改動：呼叫後端 API 並綁定到對應的 UI 集合（使用 CancellationToken）
        List<WorksheetsDto> MappedWorksheets(List<WorksheetIncludeTimelineDto> dtos) 
        {
            var result = new List<WorksheetsDto>();
            if (dtos == null || dtos.Count == 0)
                return result;

            foreach (var dto in dtos)
            {
                // 僅映射已完成的工單
                if (!string.Equals(dto.status, "Completed", StringComparison.OrdinalIgnoreCase))
                    continue;

                result.Add(new WorksheetsDto
                {
                    _id = dto._id,
                    worksheetNumber = dto.workSheetNumber,
                    workpieceName = dto.workpieceName,
                    workStatus = dto.status,
                    targetEDM = dto.TargetEDM,
                    // 已完成的工單通常不再有進度，安全預設
                    processStep = 0,
                    totalProcessStep = 0,
                    coordinate = string.Empty,
                    setupUser = string.Empty
                });
            }

            return result;
        }
        private async Task FetchAndBindByStatusAsync()
        {
            try
            {
                // 支援取消
                _currentUpdateCts?.Cancel();
                _currentUpdateCts?.Dispose();
                _currentUpdateCts = new CancellationTokenSource();
                _currentUpdateCts.CancelAfter(TimeSpan.FromMilliseconds(1000));
                var ct = _currentUpdateCts.Token;
                var status = "";
                if (SelectedTabIndexParameter == 0) status = "New";
                else if (SelectedTabIndexParameter == 1) status = MapWorkStatusForEdmTab(EDMSelectedTab);
                else if (SelectedTabIndexParameter == 2) status = "Failure";
                else return;
                List<WorksheetsDto> ws = new();

                if (status == "Completed")
                {
                    if (FromDate != null && ToDate != null)
                    {
                        List<WorksheetIncludeTimelineDto> dtos = await _WorksheetsService.GetWorkSheetsIncludeTimelineByDateTimeAsync(FromDate.Value, ToDate.Value, ct) ?? new();
                        ws = MappedWorksheets(dtos);
                    }
                }
                else
                    ws = await _WorksheetsService.GetWorkSheetByWorkStatusAsync(status, CancellationToken.None) ?? new List<WorksheetsDto>();
                WorkOrderList.Clear();
                FilteredEDMList.Clear();
                FailureWorkOrders.Clear();
                foreach (var w in ws)
                {
                    var workOrder = MapToWorkOrderData(w); // 建立工單資料
                    var es = await _ElectrodeService.GetElectrodeByWorksheetNumberAsync(w.worksheetNumber ?? string.Empty, CancellationToken.None) // 取得該工單的電極清單
                             ?? new List<ElectrodeDto>();
                    foreach (var e in es)
                    {
                        // DTO 裡 edM_offsetPGM 為字串，需轉成整數
                        int offsetVal = 0;
                        if (!string.IsNullOrWhiteSpace(e.edM_offsetPGM))
                            int.TryParse(e.edM_offsetPGM, out offsetVal);

                        // NeedEDM 判斷：有程式且尚未完成/驗證
                        bool needEDM = !string.IsNullOrWhiteSpace(e.edmpgm) &&
                                       !string.Equals(e.state, "Completed", StringComparison.OrdinalIgnoreCase) &&
                                       !string.Equals(e.state, "Verified", StringComparison.OrdinalIgnoreCase);

                        workOrder.EDMDetails.Add(new EDMDetail
                        {
                            ElectrodeName = e.electrodeName ?? "",
                            LabelSerial = e.tagSerial ?? "",
                            Status = e.state ?? "",
                            NeedEDM = true, //代定義
                            EDMProgram = e.edmpgm ?? "",
                            Offset = e.offsetStatus ?? 0,
                            IsShare = e.shared,
                            ShareElectrode = e.shareLink ?? ""
                        });
                    }
                    if (SelectedTabIndexParameter == 0) WorkOrderList.Add(workOrder);
                    else if (SelectedTabIndexParameter == 1) FilteredEDMList.Add(workOrder);
                    else if (SelectedTabIndexParameter == 2) FailureWorkOrders.Add(workOrder);
                }
                return;
            }
            catch { }
        }

        // 新增：處理刪除工單的非同步方法（包含 UI 確認、API 呼叫與錯誤處理）
        private async Task DeleteWorkOrderAsync(WorkOrderData item)
        {
            if (!_auth.RequireLogin())
                return;
            try
            {
                // 要求使用者確認
                bool yes = _WindowService.ShowYesNoDialog($"確定要刪除工單 {item.WorksheetNumber} 嗎？");
                if (!yes) return;
                if (string.IsNullOrWhiteSpace(item.Id))
                {
                    _WindowService.ShowMessage("找不到工單 ID，無法刪除。");
                    return;
                }
                bool ok = false;
                // 刪除相關電極資料
                var eleDtos = await _ElectrodeService.GetElectrodeByWorksheetNumberAsync(item.WorksheetNumber, CancellationToken.None);
                if (eleDtos != null)
                {
                    foreach (var ele in eleDtos)
                    {
                        if (!string.IsNullOrWhiteSpace(ele._id))
                        {
                            ok = await _ElectrodeService.DB_DeleteElectrodeDataByIdAsync(ele._id);
                            if (!ok)
                            {
                                _WindowService.ShowMessage($"刪除工單中的電極失敗：");
                                return;
                            }

                        }
                    }
                }

                //刪除相關工件資料
                var wpDtos = await _WorkpieceService.GetWorkpieceByWorksheetNumberAsync(item.WorksheetNumber, CancellationToken.None);
                if (wpDtos != null)
                {
                    ok = await _WorkpieceService.DeleteWorkpieceDataByIdAsync(wpDtos._id);
                    if (!ok)
                    {
                        _WindowService.ShowMessage($"刪除工單中的工件失敗：");
                        return;
                    }
                }

                // 呼叫後端 API 刪除工單
                ok = await _WorksheetsService.DeleteWorkSheetDataByIdAsync(item.Id);
                if (ok)
                {
                    WorkOrderList.Remove(item);
                    _WindowService.ShowMessage("已成功刪除工單。");   // 若 API 呼叫成功，從 UI 清單移除
                }
                else
                {
                    _WindowService.ShowMessage($"刪除工單失敗：");
                    return;
                }
            }
            catch (Exception ex)
            {
                _WindowService.ShowMessage($"刪除工單失敗：{ex.Message}"); // 顯示錯誤資訊但不要讓應用程式崩潰
            }
            _ = FetchAndBindByStatusAsync();
        }

        // ← 新增：分頁索引對應到後端 WorkStatus（請依實際需求調整）
        private static string MapWorkStatusForParameterTab(int index) => index switch
        {
            0 => "Working",
            1 => "Verified",
            2 => "Completed",
            _ => "Working"
        };

        private static string MapWorkStatusForEdmTab(int index) => index switch
        {
            0 => "Waiting",
            1 => "Queue",
            2 => "Processing",
            3 => "Completed",
            _ => ""
        };

        // ← 新增：將 Workpiece 轉為畫面使用的 WorkOrderData
        private static WorkOrderData MapToWorkOrderData(WorksheetsDto ws)
        {
            return new WorkOrderData
            {
                Id = ws._id ?? string.Empty,
                WorksheetNumber = ws.worksheetNumber ?? string.Empty,
                WorkpieceName = ws.workpieceName ?? string.Empty,
                Status = ws.workStatus ?? string.Empty,
                StatusColor = ToStatusBrush(ws.workStatus),
                TargetEDM = ws.targetEDM ?? string.Empty,
                Coordinate = ws.coordinate ?? string.Empty,
                SetupUser = ws.setupUser ?? string.Empty,
                ProcessStep = ws.processStep ?? 0,
                TotalProcessStep = ws.totalProcessStep ?? 0,
                EDMDetails = new ObservableCollection<EDMDetail>() // Worksheets 不含電極明細，先回傳空集合
            };
        }

        private static SolidColorBrush ToStatusBrush(string? status)
        {
            return status switch
            {
                "Queue" => new SolidColorBrush(Color.FromRgb(0xE6, 0xB9, 0x3E)), //黃色
                "Processing" => new SolidColorBrush(Color.FromRgb(0x56, 0xC0, 0x6C)), //綠色
                "Failure" => new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B)), //紅色
                "Completed" => new SolidColorBrush(Color.FromRgb(0x2F, 0x64, 0xCF)), //藍色
                _ => Brushes.Gray
            };
        }

        // 新增於類別內（private helper）
        private void ScheduleFetchForWorkOrder()
        {
            _ = FetchAndBindByStatusAsync();
        }
    }

    public class WorkOrderData : INotifyPropertyChanged
    {
        // 新增 Id 屬性以對應後端 MongoDB _id
        public string Id { get; set; } = string.Empty;

        public string WorksheetNumber { get; set; } = ""; // 工單編號
        public string WorkpieceName { get; set; } = ""; //工件名稱
        public string Status { get; set; } = ""; //工單狀態
        public Brush StatusColor { get; set; } = Brushes.Transparent;//工單燈號
        public string TargetEDM { get; set; } = "";//目標EDM
        public string Coordinate { get; set; } = "";//座標
        public string SetupUser { get; set; } = "";//設定者
        public int ProcessStep { get; set; } = 1; //目前步數
        public int TotalProcessStep { get; set; } = 3; //總步數


        public double EDMStageProgress => TotalProcessStep == 0 ? 0 : (100.0 * ProcessStep / TotalProcessStep);  // 進度條百分比（回傳 double）
        public string EDMStageDisplay => $"EDM加工階段：{ProcessStep} / {TotalProcessStep}"; // 顯示文字，如 "EDM加工階段：1 / 3"
        public ObservableCollection<EDMDetail> EDMDetails { get; set; } = new ObservableCollection<EDMDetail>();

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class EDMDetail 
    {
        public string ElectrodeName { get; set; } = "";
        public string LabelSerial { get; set; } = "";
        public string Status { get; set; } = "";
        public bool NeedEDM { get; set; }
        public string EDMProgram { get; set; } = "";
        public int Offset { get; set; }
        public bool IsShare { get; set; } = false; //總步數
        public string ShareElectrode { get; set; } = ""; //總步數

    }
}
