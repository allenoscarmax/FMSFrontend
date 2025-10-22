using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Interfaces;
using FMSFrontend.Services;
using FMSFrontend.Views;
using OSCARMAXFMS_V3.DBmodels; // ← 新增：Workpiece 模型
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

namespace FMSFrontend.ViewModels
{
    public partial class WorkOrderPageViewModel : ObservableObject
    {

        [ObservableProperty]
        private int selectedTabIndexParameter;
        partial void OnSelectedTabIndexParameterChanged(int value)
        {
            _paramTabCts?.Cancel();
            _paramTabCts = new CancellationTokenSource();

            var status = MapWorkStatusForParameterTab(value);
            _ = FetchAndBindByStatusAsync(status, _paramTabCts.Token);
        }

        [ObservableProperty]
        private int eDMSelectedTab; // 決定 EDM 子頁籤
        partial void OnEDMSelectedTabChanged(int value)
        {
            _edmTabCts?.Cancel();
            _edmTabCts = new CancellationTokenSource();

            var status = MapWorkStatusForEdmTab(value);
            _ = FetchAndBindByStatusAsync(status, _edmTabCts.Token);
        }
        public List<string> DateFilterOptions { get; set; } = new() { "今天", "過去7天", "自訂" };
        [ObservableProperty]
        private string selectedFilterOption = "今天";

        private DateTime? lastValidFromDate = DateTime.Today;
        private DateTime? lastValidToDate = DateTime.Today;

        private readonly IWindowService _windowService;
        private readonly IHttpService _httpService; // ← 新增：呼叫 API

        // ← 新增：切換標籤時取消前一個請求
        private CancellationTokenSource? _paramTabCts;
        private CancellationTokenSource? _edmTabCts;

        partial void OnSelectedFilterOptionChanged(string value)
        {
            OnPropertyChanged(nameof(IsCustomDateMode));
            ApplyDateFilter();
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
        }

        [ObservableProperty]
        private DateTime? toDate = DateTime.Today;

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
        }
        public bool IsCustomDateMode => SelectedFilterOption == "自訂";

        public ObservableCollection<WorkOrderData> WorkOrderList { get; set; }
        public ObservableCollection<WorkOrderData> FailureWorkOrders { get; set; } = new();

        // 最少新增：EDM 子頁簽綁定需要的集合（XAML 中綁定了 FilteredEDMList）
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
        }
        public ICommand DeleteCommand { get; }

        public WorkOrderPageViewModel(IWindowService windowService, IHttpService httpService) // ← 調整建構子
        {
            _windowService = windowService;
            _httpService = httpService;
            

            WorkOrderList = new ObservableCollection<WorkOrderData>
            {
                // 可保留初始化範例資料；切換分頁後會被 API 結果覆蓋
                new WorkOrderData
                {
                    WorksheetNumber = "20250722001",
                    WorkpieceName = "24-018-003",
                    Status = "NEW",
                    StatusColor = Brushes.Gold,
                    
                    TargetEDM = "EDM1",
                    Coordinate = "G01",
                    SetupUser = "admin",
                    
                    EDMDetails =new ObservableCollection<EDMDetail>
                    {
                        new EDMDetail { ElectrodeName = "E01", LabelSerial = "A123", Status = "待機", NeedEDM = true, EDMProgram = "P1", Offset = 1 },
                        new EDMDetail { ElectrodeName = "E02", LabelSerial = "A124", Status = "加工中", NeedEDM = false, EDMProgram = "P2", Offset = 2 },
                    }
                }
            };

            // 使用非同步方法刪除：保留 RelayCommand，但在內部啟動 async Task
            DeleteCommand = new RelayCommand<WorkOrderData>(item =>
            {
                if (item != null)
                {
                    _ = DeleteWorkOrderAsync(item);
                }
            });
        }

        private void ShowWarning(string message)
        {
            FMSFrontend.Extensions.DialogMessageWindow dd = new Extensions.DialogMessageWindow(message);
            dd.Show();
        }
        [RelayCommand]
        private void OpenUploadSheet()
        {
            _windowService.ShowUploadSheetWindow();
        }
        // 最小改動：呼叫後端 API 並綁定到對應的 UI 集合（使用 CancellationToken）
        private async Task FetchAndBindByStatusAsync(string workStatus, CancellationToken ct)
        {
           // try
           // {
                // 支援取消
                JsonElement? json = await _httpService.GetJsonAsync<JsonElement>("Worksheet/DB_GetAllWorkSheet", ct);
                ct.ThrowIfCancellationRequested();

                List<Worksheets> worksheets = json.HasValue
                    ? JsonSerializer.Deserialize<List<Worksheets>>(json.Value.GetRawText()) ?? new List<Worksheets>()
                    : new List<Worksheets>();

                var mapped = worksheets.Select(MapToWorkOrderData).ToList();

                // 只更新目前畫面所綁定的集合（避免不必要變動）
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (SelectedTabIndexParameter == 1)
                    {
                        // EDM 子頁簽
                        FilteredEDMList.Clear();
                        foreach (var w in mapped.Where(x => string.Equals(x.Status, workStatus, StringComparison.OrdinalIgnoreCase)))
                            FilteredEDMList.Add(w);
                        return;
                    }

                    if (SelectedTabIndexParameter == 2 || string.Equals(workStatus, "Error", StringComparison.OrdinalIgnoreCase) || string.Equals(workStatus, "Failed", StringComparison.OrdinalIgnoreCase))
                    {
                        FailureWorkOrders.Clear();
                        foreach (var w in mapped.Where(x => string.Equals(x.Status, workStatus, StringComparison.OrdinalIgnoreCase)))
                            FailureWorkOrders.Add(w);
                        return;
                    }

                    // 預設更新主頁（New/參數分頁）
                    WorkOrderList.Clear();
                    foreach (var w in mapped.Where(x => string.Equals(x.Status, workStatus, StringComparison.OrdinalIgnoreCase)))
                        WorkOrderList.Add(w);
                });
           // }
           // catch (OperationCanceledException)
           // {
                // 被取消，靜默忽略
           // }
           // catch (Exception ex)
           // {
           //     Application.Current.Dispatcher.Invoke(() =>
           //     {
           //         _windowService.ShowMessage($"讀取工單資料失敗：{ex.Message}");
           //     });
          //  }
        }

        // 新增：處理刪除工單的非同步方法（包含 UI 確認、API 呼叫與錯誤處理）
        private async Task DeleteWorkOrderAsync(WorkOrderData item)
        {
            try
            {
                // 要求使用者確認
                bool yes = _windowService.ShowYesNoDialog($"確定要刪除工單 {item.WorksheetNumber} 嗎？");
                if (!yes) return;

                if (string.IsNullOrWhiteSpace(item.Id))
                {
                    _windowService.ShowMessage("找不到工單 ID，無法刪除。");
                    return;
                }

                string route = $"Worksheet/DB_DeleteWorkSheetDataById/{item.Id}";
                await _httpService.SendPutAsync(route,"");

                // 若 API 呼叫成功，從 UI 清單移除
                WorkOrderList.Remove(item);
                _windowService.ShowMessage("已成功刪除工單。");
            }
            catch (Exception ex)
            {
                // 顯示錯誤資訊但不要讓應用程式崩潰
                _windowService.ShowMessage($"刪除工單失敗：{ex.Message}");
            }
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
            0 => "Reserved",
            1 => "Working",
            2 => "Error",
            _ => "Working"
        };

        // ← 新增：將 Workpiece 轉為畫面使用的 WorkOrderData
        private static WorkOrderData MapToWorkOrderData(Worksheets ws)
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

        private static Brush ToStatusBrush(string? status)
        {
            return status switch
            {
                "Working" => Brushes.Orange,
                "Verified" => Brushes.MediumPurple,
                "Completed" => Brushes.DeepSkyBlue,
                "Reserved" => Brushes.SteelBlue,
                "Error" => Brushes.IndianRed,
                "Empty" => Brushes.Gray,
                _ => Brushes.Gray
            };
        }
    }

    public class WorkOrderData : INotifyPropertyChanged
    {
        // 新增 Id 屬性以對應後端 MongoDB _id
        public string Id { get; set; } = string.Empty;

        public string WorksheetNumber { get; set; } // 工單編號
        public string WorkpieceName { get; set; } //工件名稱
        public string Status { get; set; } //工單狀態
        public Brush StatusColor { get; set; } //工單燈號
        public string TargetEDM { get; set; } //目標EDM
        public string Coordinate { get; set; } //座標
        public string SetupUser { get; set; } //設定者
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class EDMDetail 
    {
        public string ElectrodeName { get; set; }
        public string LabelSerial { get; set; }
        public string Status { get; set; }
        public bool NeedEDM { get; set; }
        public string EDMProgram { get; set; }
        public int Offset { get; set; }

    }
}
