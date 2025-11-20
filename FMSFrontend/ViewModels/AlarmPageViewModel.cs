using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Features.Dtos.Database;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Interfaces;
using FMSFrontend.Models;
using FMSFrontend.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Windows.Data;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace FMSFrontend.ViewModels
{
    public enum AlarmSeverity { None, Hint, Alarm }
    public partial class AlarmPageViewModel : ObservableObject
    {
        private readonly IWindowService _windowService;
        private readonly IAlarmService _alarmService;
        public AlarmStore AlarmStore { get; }
        public AlarmGroupModel AlarmGroup => AlarmStore.AlarmGroup;

        // —— 新增：MainWindow 要綁的三個摘要屬性 ——
        [ObservableProperty] private AlarmSeverity summarySeverity = AlarmSeverity.None;
        [ObservableProperty] private string summaryMessage = "目前無提示";
        [ObservableProperty] private int unreadCount = 0;


        // ===== Tabs =====
        [ObservableProperty]
        private int selectedTabIndexParameter = 0; // 預設選「目前警報」

        // ===== 日期篩選 =====
        public ObservableCollection<string> DateFilterOptions { get; } =
            new() { "今天", "前7天", "自訂" };

        [ObservableProperty] private string? selectedFilterOption = "今天";
        [ObservableProperty] private DateTime? fromDate = DateTime.Today;  // DatePicker 友善
        [ObservableProperty] private DateTime? toDate = DateTime.Today;
        [ObservableProperty] private bool isCustomDateMode;

        private DateTime? _lastValidFromDate = DateTime.Today;
        private DateTime? _lastValidToDate = DateTime.Today;

        // ===== 資料來源 =====
        public ObservableCollection<AlarmItem> CurrentAlarms { get; } = new();
        public ObservableCollection<AlarmItem> HistoryAlarms { get; } = new();

        // 仍保留 ICollectionView 供匯出計數用（會跟日期篩選同步）
        private readonly ICollectionView _historyView;

        [ObservableProperty] private AlarmItem? selectedHistoryAlarm;

        // ======================= Pagination =======================
        public ObservableCollection<AlarmItem> PagedHistoryAlarms { get; } = new();

        [ObservableProperty] private int pageSize = 12;   // 每頁筆數（可依 UI 高度調）
        [ObservableProperty] private int currentPage = 1;
        [ObservableProperty] private int totalPages = 1;

        public bool CanGoPrev => CurrentPage > 1;
        public bool CanGoNext => CurrentPage < TotalPages;

        [RelayCommand] private void GoFirstPage() => LoadPage(1);
        [RelayCommand] private void GoPrevPage() => LoadPage(CurrentPage - 1);
        [RelayCommand] private void GoNextPage() => LoadPage(CurrentPage + 1);
        [RelayCommand] private void GoLastPage() => LoadPage(TotalPages);

        partial void OnPageSizeChanged(int value) => ApplyFilter();
        partial void OnCurrentPageChanged(int value)
        {
            OnPropertyChanged(nameof(CanGoPrev));
            OnPropertyChanged(nameof(CanGoNext));
        }

        // ===================== End Pagination =====================

        public AlarmPageViewModel(IWindowService windowService, IAlarmService alarmService, AlarmStore alarmStore)
        {
            _windowService = windowService;
            _alarmService = alarmService;
            AlarmStore = alarmStore;


            AlarmStore.AlarmGroup.PropertyChanged += (_, __) => RefreshFromStore();

            selectedFilterOption = DateFilterOptions.FirstOrDefault(); // 預設第一個

            // ---- Demo：目前警報 ----
            //CurrentAlarms.Add(new AlarmItem { Time = DateTime.Parse("2025/06/23 16:19:19"), Code = "HINT_007", Message = "!", Level = "HINT", Source = "EDM02S" });
            // ---- Demo：歷史警報 ----
            //HistoryAlarms.Add(new AlarmItem { Time = DateTime.Today.AddHours(-5), Code = "ALARM_003", Level = "ALARM", Message = "Robot Losts Connection.", Source = "EDM02S" });

            // ICollectionView：配合同步的日期過濾
            _historyView = CollectionViewSource.GetDefaultView(HistoryAlarms);
            _historyView.Filter = FilterByDate;

            // 來源集合變動時重算分頁
            HistoryAlarms.CollectionChanged += (_, __) => ApplyFilter();

            // 預設區間 + 初次過濾/分頁
            ApplyDatePreset();
            ApplyFilter();


            // Summary 來源：以「目前警報」(CurrentAlarms) 為準
            CurrentAlarms.CollectionChanged += (_, __) => RecomputeSummary();
            RecomputeSummary(); // 初次算一次

        }
        public void OnPageActivated() // 開啟警報視窗
        {
            AlarmGroup.IsAlarmWindowsOpen = true;
        }
        public void OnPageDeactivated() // 關閉警報視窗
        {
            AlarmGroup.IsAlarmWindowsOpen = false;
        }
        void RefreshFromStore() // 從 AlarmStore 更新目前警報
        {
            
            if (SelectedTabIndexParameter == 0)
            {
                for (int i = 0; i < AlarmGroup.AlarmModels.Count; i++)
                {
                    if (CurrentAlarms.Count == i) CurrentAlarms.Add(new AlarmItem());
                    CurrentAlarms[i].Time = AlarmGroup.AlarmModels[i].TimeStamp;
                    CurrentAlarms[i].Code = AlarmGroup.AlarmModels[i].ErrorCode;
                    CurrentAlarms[i].Message = AlarmGroup.AlarmModels[i].MessageCn;
                    CurrentAlarms[i].Level = GetLevel(AlarmGroup.AlarmModels[i].ErrorCode);
                    CurrentAlarms[i].Source = "";
                }
                while (CurrentAlarms.Count > AlarmGroup.AlarmModels.Count)
                {
                    CurrentAlarms.RemoveAt(CurrentAlarms.Count - 1);
                }
            }
        }
        string GetLevel(string ErrorCode) // 取得警報等級
        {
            if (ErrorCode.IndexOf("ALARM") != 0)
                return "ALARM";
            else if (ErrorCode.IndexOf("HINT") != 0)
                return "HINT";
            else
                return "INFO";
        }
        public async Task RefreshFromDate() // 從日期篩選更新歷史警報
        {
            try
            {
                if (FromDate == null || ToDate == null) return;
                
              List<ErrorMessageLogDto> dtos = await _alarmService.GetErrorMessageLogByDateTimeAsync(FromDate.Value, ToDate.Value.AddDays(1))
                 ?? new List<ErrorMessageLogDto>();
              HistoryAlarms.Clear();
              foreach (var dto in dtos)
              {
                  HistoryAlarms.Add(new AlarmItem
                  {
                      Time = dto.TimeStamp,
                      Code = dto.ErrorCode,
                      Message = dto.MessageCn,
                      Level = GetLevel(dto.ErrorCode),
                      Source = ""
                  });
              }
              
            }
            catch { }
        }

        // ====== 事件：選單/日期變更 ======
        partial void OnSelectedFilterOptionChanged(string? value)
        {
            IsCustomDateMode = value == "自訂";
            ApplyDatePreset();
            ApplyFilter();
            _ = RefreshFromDate();
        }

        partial void OnFromDateChanged(DateTime? value)
        {
            if (value == null || ToDate == null) { _lastValidFromDate = value; return; }

            if (value > ToDate)
            {
                _windowService.ShowMessage("開始日期不能大於結束日期");
                FromDate = _lastValidFromDate;
                return;
            }
            if ((ToDate - value)?.TotalDays > 31)
            {
                _windowService.ShowMessage("選擇的日期範圍不能超過一個月");
                FromDate = _lastValidFromDate;
                return;
            }

            _lastValidFromDate = value;
            ApplyFilter();
           _ = RefreshFromDate();
        }

        partial void OnToDateChanged(DateTime? value)
        {
            if (value == null || FromDate == null) { _lastValidToDate = value; return; }

            if (value < FromDate)
            {
                _windowService.ShowMessage("結束日期不能小於開始日期");
                ToDate = _lastValidToDate;
                return;
            }
            if ((value - FromDate)?.TotalDays > 31)
            {
                _windowService.ShowMessage("選擇的日期範圍不能超過一個月");
                ToDate = _lastValidToDate;
                return;
            }

            _lastValidToDate = value;
            ApplyFilter();
            _ = RefreshFromDate();
        }

        // ====== Commands（仍可用） ======
        [RelayCommand]
        private void ExportAlarms()
        {
            // 以 _historyView 的目前過濾結果計數
            var count = _historyView.Cast<AlarmItem>().Count();
            _windowService.ShowMessage($"已匯出 {count} 筆歷史警報（示意）");
        }

        [RelayCommand]
        private void RemoveSelectedAlarms()
        {
            if (SelectedHistoryAlarm is null)
            {
                _windowService.ShowMessage("請先選擇要移除的項目");
                return;
            }
            HistoryAlarms.Remove(SelectedHistoryAlarm);
            SelectedHistoryAlarm = null;
            ApplyFilter();
        }

        // ====== Helpers ======
        private void ApplyDatePreset()
        {
            switch (SelectedFilterOption)
            {
                case "今天":
                    FromDate = DateTime.Today; ToDate = DateTime.Today; break;
                case "前7天":
                    FromDate = DateTime.Today.AddDays(-6); ToDate = DateTime.Today; break;
                case "自訂":
                default: break; // 保留使用者輸入
            }
            _lastValidFromDate = FromDate;
            _lastValidToDate = ToDate;
        }

        // ICollectionView 的 Filter（供 _historyView 使用）
        private bool FilterByDate(object obj) => obj is AlarmItem a && InRange(a);

        // 給 ApplyFilter() 直接 LINQ 過濾使用
        private bool InRange(AlarmItem a)
        {
            if (FromDate is null && ToDate is null) return true;
            var from = (FromDate ?? DateTime.MinValue).Date;
            var to = (ToDate ?? DateTime.MaxValue).Date.AddDays(1).AddTicks(-1);
            return a.Time >= from && a.Time <= to;
        }

        // 核心：依日期過濾 + 排序 + 分頁；也會刷新 _historyView 讓 Export 計數正確
        private void ApplyFilter()
        {
            // 先 refresh 供 _historyView 內部使用（Export 會用）
            _historyView?.Refresh();

            var filtered = HistoryAlarms
                .Where(InRange)
                .OrderByDescending(a => a.Time)
                .ToList();

            TotalPages = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)PageSize));
            CurrentPage = Math.Max(1, Math.Min(CurrentPage, TotalPages));

            PagedHistoryAlarms.Clear();
            foreach (var a in filtered.Skip((CurrentPage - 1) * PageSize).Take(PageSize))
                PagedHistoryAlarms.Add(a);

            OnPropertyChanged(nameof(CanGoPrev));
            OnPropertyChanged(nameof(CanGoNext));

        }

        private void LoadPage(int page)
        {
            if (TotalPages <= 0) TotalPages = 1;
            page = Math.Max(1, Math.Min(TotalPages, page));
            if (page == CurrentPage) return;
            CurrentPage = page;
            ApplyFilter();
        }

        // —— 依目前警報重算摘要：最高等級 + 最新訊息 + 未讀數 —— 
        private void RecomputeSummary()
        {
            if (CurrentAlarms.Count == 0)
            {
                SummarySeverity = AlarmSeverity.None;
                SummaryMessage = "目前無提示";
                UnreadCount = 0;
                return;
            }

            // 最高等級：只要有 ALARM 就算 ALARM；否則有 HINT 算 HINT；否則 None
            var hasAlarm = CurrentAlarms.Any(a => string.Equals(a.Level, "ALARM", StringComparison.OrdinalIgnoreCase));
            var hasHint = CurrentAlarms.Any(a => string.Equals(a.Level, "HINT", StringComparison.OrdinalIgnoreCase));

            SummarySeverity = hasAlarm ? AlarmSeverity.Alarm
                             : hasHint ? AlarmSeverity.Hint
                                        : AlarmSeverity.None;

            // 最新訊息（依時間最大）
            var latest = CurrentAlarms.OrderByDescending(a => a.Time).First();
            SummaryMessage = $"{latest.Source}：{latest.Message}";

            // 未讀（簡單處理：用目前數量；之後你可換成 IsRead 計算）
            UnreadCount = CurrentAlarms.Count;
        }
    }

    public class AlarmItem
    {
        public DateTime Time { get; set; }
        public string Code { get; set; } = "";
        public string Message { get; set; } = "";
        public string Level { get; set; } = "";   // "ALARM" / "HINT" / "INFO"
        public string Source { get; set; } = "";
    }
}
