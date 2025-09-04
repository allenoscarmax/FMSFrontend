using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;
using Microsoft.Win32;

namespace FMSFrontend.ViewModels
{
    public partial class OperationHistoryViewModel : ObservableObject
    {
        // 供 UI 綁定的快速範圍
        public List<string> QuickRanges { get; set; } = new() { "今天", "過去7天", "自訂" };

        [ObservableProperty]
        private string selectedQuickRange = "今天";

        // 供工具列 DatePicker 綁定
        [ObservableProperty]
        private DateTime? startDate = DateTime.Today;

        [ObservableProperty]
        private DateTime? endDate = DateTime.Today;

        // DataGrid 綁這個（已過濾）
        public ICollectionView OperationRecords { get; set; }

        // 內部完整資料集
        private readonly ObservableCollection<OperationRecord> _allRecords = new();

        private readonly IWindowService _windowService;

        private DateTime? _lastValidStart = DateTime.Today;
        private DateTime? _lastValidEnd = DateTime.Today;

        public OperationHistoryViewModel(IWindowService windowService)
        {
            _windowService = windowService;

            // 假資料（你可改成實際資料來源）
            SeedSample();

            // 準備 View + 篩選器
            OperationRecords = CollectionViewSource.GetDefaultView(_allRecords);
            OperationRecords.Filter = FilterByDate;

            // 依預設「今天」套一次
            ApplyQuickRange();
        }

        #region 快速範圍/日期變更

        partial void OnSelectedQuickRangeChanged(string value)
        {
            ApplyQuickRange();
            RefreshFilter();
        }

        partial void OnStartDateChanged(DateTime? value)
        {
            // 自訂模式才允許手調日期；快速範圍仍可顯示但不調整
            if (SelectedQuickRange != "自訂")
                return;

            if (!ValidateRange(value, EndDate, out string? msg))
            {
                _windowService.ShowMessage(msg ?? "日期區間不合法");
                StartDate = _lastValidStart;
                return;
            }

            _lastValidStart = value;
            RefreshFilter();
        }

        partial void OnEndDateChanged(DateTime? value)
        {
            if (SelectedQuickRange != "自訂")
                return;

            if (!ValidateRange(StartDate, value, out string? msg))
            {
                _windowService.ShowMessage(msg ?? "日期區間不合法");
                EndDate = _lastValidEnd;
                return;
            }

            _lastValidEnd = value;
            RefreshFilter();
        }

        private void ApplyQuickRange()
        {
            switch (SelectedQuickRange)
            {
                case "今天":
                    StartDate = DateTime.Today;
                    EndDate = DateTime.Today;
                    break;
                case "過去7天":
                    EndDate = DateTime.Today;
                    StartDate = DateTime.Today.AddDays(-6); // 含今天共7天
                    break;
                case "自訂":
                    // 保留使用者目前的 Start/End，不強制改動
                    break;
            }
            _lastValidStart = StartDate;
            _lastValidEnd = EndDate;
        }

        private static bool ValidateRange(DateTime? start, DateTime? end, out string? message)
        {
            message = null;
            if (start is null || end is null) return true;

            if (end < start)
            {
                message = "結束日期不能小於開始日期";
                return false;
            }

            if ((end.Value - start.Value).TotalDays > 31)
            {
                message = "選擇的日期範圍不能超過一個月";
                return false;
            }

            return true;
        }

        private void RefreshFilter() => OperationRecords?.Refresh();

        private bool FilterByDate(object obj)
        {
            if (obj is not OperationRecord r) return false;

            // 沒選日期就不篩
            if (StartDate is null || EndDate is null) return true;

            var d = r.Time.Date;
            return d >= StartDate.Value.Date && d <= EndDate.Value.Date;
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void Clear()
        {
            SelectedQuickRange = "今天";
            ApplyQuickRange();
            RefreshFilter();
        }

        [RelayCommand]
        private void Export()
        {
            // 匯出目前「篩選後」的資料
            var rows = OperationRecords.Cast<OperationRecord>().ToList();
            if (rows.Count == 0)
            {
                _windowService.ShowMessage("目前沒有可匯出的資料。");
                return;
            }

            var dlg = new SaveFileDialog
            {
                Title = "匯出操作紀錄",
                Filter = "CSV 檔 (*.csv)|*.csv",
                FileName = $"OperationHistory_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    using var sw = new StreamWriter(dlg.FileName, false, System.Text.Encoding.UTF8);
                    // 標題列
                    sw.WriteLine("時間,使用者,訊息");

                    foreach (var r in rows)
                    {
                        // CSV 簡單轉義（用雙引號包起來）
                        string t = r.Time.ToString("yyyy/M/d HH:mm:ss", CultureInfo.InvariantCulture);
                        string u = CsvEscape(r.User);
                        string m = CsvEscape(r.Message);
                        sw.WriteLine($"\"{t}\",\"{u}\",\"{m}\"");
                    }

                    _windowService.ShowMessage("匯出完成。");
                }
                catch (Exception ex)
                {
                    _windowService.ShowMessage($"匯出失敗：{ex.Message}");
                }
            }
        }

        private static string CsvEscape(string? s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.Replace("\"", "\"\"");
        }

        #endregion

        #region Sample Data

        private void SeedSample()
        {
            // 這裡放幾筆示意資料（你可替換為實際 Query/Service 讀取）
            _allRecords.Add(new OperationRecord
            {
                Time = new DateTime(2025, 9, 2, 15, 23, 0),
                User = "Admin",
                Message = "登入成功"
            });
            _allRecords.Add(new OperationRecord
            {
                Time = new DateTime(2025, 9, 3, 10, 0, 0),
                User = "None",
                Message = "系統開啟"
            });
            _allRecords.Add(new OperationRecord
            {
                Time = new DateTime(2025, 9, 3, 11, 30, 0),
                User = "None",
                Message = "系統關閉"
            });

            // 也可依你實際畫面重複幾筆
            _allRecords.Add(new OperationRecord { Time = new DateTime(2025, 9, 3, 10, 0, 0), User = "None", Message = "系統開啟" });
            _allRecords.Add(new OperationRecord { Time = new DateTime(2025, 9, 3, 11, 30, 0), User = "None", Message = "系統關閉" });
            _allRecords.Add(new OperationRecord { Time = new DateTime(2025, 9, 3, 10, 0, 0), User = "None", Message = "系統開啟" });
            _allRecords.Add(new OperationRecord { Time = new DateTime(2025, 9, 3, 11, 30, 0), User = "None", Message = "系統關閉" });
        }

        #endregion

        #region Model

        public class OperationRecord
        {
            public DateTime Time { get; set; }
            public string User { get; set; } = "";
            public string Message { get; set; } = "";

            // 若你 XAML 綁的是 TimeText，可保留這個
            public string TimeText => Time.ToString("yyyy/M/d HH:mm:ss", CultureInfo.InvariantCulture);
        }

        #endregion
    }
}
