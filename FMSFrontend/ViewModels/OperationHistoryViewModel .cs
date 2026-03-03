using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Features.Services;
using FMSFrontend.Helpers;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Interfaces;
using FMSFrontend.Models;
using FMSFrontend.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FMSFrontend.ViewModels
{
    public partial class OperationHistoryViewModel : ObservableObject
    {
        // DataGrid 綁這個（已過濾）
        public ICollectionView OperationRecords { get; set; }
        // 內部完整資料集
        private readonly ObservableCollection<OperationRecord> _allRecords = new();
        private readonly IWindowService _windowService;
        private readonly IOperationMessageLogService _operationService;
        public OperationHistoryViewModel(IWindowService windowService, IOperationMessageLogService operationService)
        {
            _windowService = windowService;
          //  LanguageManager.ApplySavedLanguage();

            OperationRecords = CollectionViewSource.GetDefaultView(_allRecords);
            OperationRecords.Filter = FilterByDate;
            _operationService = operationService;

            SelectedFilterIndex = 0;
            OnSelectedFilterIndexChanged(0);
        }
        private async Task Refresh() //讀取記錄檔案
        {
            try
            {
                if (FromDate is null || ToDate is null)
                    return;

                var from = FromDate.Value.Date;
                var to = ToDate.Value.Date.AddDays(1).AddTicks(-1); // 包含當天整日

                var dtos = await _operationService.GetOperationMessageLogByDateAsync(from, to);

                _allRecords.Clear();
                if (dtos != null)
                {
                    foreach (var dto in dtos)
                    {
                        _allRecords.Add(new OperationRecord
                        {
                            Time = dto.TimeStamp,
                            User = dto.SetupUser ?? string.Empty,
                            Message = dto.MessageCn ?? string.Empty
                        });
                    }
                }

                RefreshFilter();
            }
            catch (Exception ex)
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    LanguageManager.GetString("OperationHistory_Message_LoadFailed", "讀取操作紀錄失敗：{0}"),
                    ex.Message);
                _windowService.ShowMessage(message);
            }
        }
        #region 日期篩選
        // ===== 日期篩選 =====
        [ObservableProperty] private DateTime? fromDate = DateTime.Today;
        [ObservableProperty] private DateTime? toDate = DateTime.Today;
        [ObservableProperty] private bool isCustomDateMode;
        [ObservableProperty] private int selectedFilterIndex;

        private bool _updatingDate;
        partial void OnSelectedFilterIndexChanged(int value)
        {
            IsCustomDateMode = value == 2;
            ApplyDateFilter(value);
            _ = Refresh();
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
        partial void OnFromDateChanged(DateTime? value) //開始日期變更
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
                    _windowService.ShowMessage(LanguageManager.GetString("OperationHistory_Message_DateRangeTooLong", "選擇的日期範圍不能超過一個月"));
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
            _ = Refresh();
        }
        partial void OnToDateChanged(DateTime? value) //結束日期變更
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
                    _windowService.ShowMessage(LanguageManager.GetString("OperationHistory_Message_DateRangeTooLong", "選擇的日期範圍不能超過一個月"));
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
            _ = Refresh();
        }
        private void RefreshFilter() => OperationRecords?.Refresh();
        private bool FilterByDate(object obj)
        {
            if (obj is not OperationRecord r) return false;

            // 沒選日期就不篩
            if (FromDate is null || ToDate is null) return true;

            var d = r.Time.Date;
            return d >= FromDate.Value.Date && d <= ToDate.Value.Date;
        }
        #endregion

        #region Commands

        [RelayCommand]
        private void Clear()
        {
            bool confirm = _windowService.ShowYesNoDialog(LanguageManager.GetString("OperationHistory_Confirm_Clear", "確定要刪除?"));
            if (!confirm) return;
            _operationService.DeleteAllOperationMessageLogDataAsync();
            _ = Refresh();
        }

        [RelayCommand]
        private void Export()
        {
            // 匯出目前「篩選後」的資料
            var rows = OperationRecords.Cast<OperationRecord>().ToList();
            if (rows.Count == 0)
            {
                _windowService.ShowMessage(LanguageManager.GetString("OperationHistory_Message_NoExportData", "目前沒有可匯出的資料。"));
                return;
            }

            var dlg = new SaveFileDialog
            {
                Title = LanguageManager.GetString("OperationHistory_Dialog_ExportTitle", "匯出操作紀錄"),
                Filter = "CSV 檔 (*.csv)|*.csv",
                FileName = $"OperationHistory_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    using var sw = new StreamWriter(dlg.FileName, false, System.Text.Encoding.UTF8);
                    // 標題列
                    var header = string.Join(",",
                        LanguageManager.GetString("OperationHistory_Column_Time", "時間"),
                        LanguageManager.GetString("OperationHistory_Column_User", "使用者"),
                        LanguageManager.GetString("OperationHistory_Column_Message", "訊息"));
                    sw.WriteLine(header);

                    foreach (var r in rows)
                    {
                        // CSV 簡單轉義（用雙引號包起來）
                        string t = r.Time.ToString("yyyy/M/d HH:mm:ss", CultureInfo.InvariantCulture);
                        string u = CsvEscape(r.User);
                        string m = CsvEscape(r.Message);
                        sw.WriteLine($"\"{t}\",\"{u}\",\"{m}\"");
                    }

                    _windowService.ShowMessage(LanguageManager.GetString("OperationHistory_Message_ExportSuccess", "匯出完成。"));
                }
                catch (Exception ex)
                {
                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        LanguageManager.GetString("OperationHistory_Message_ExportFailed", "匯出失敗：{0}"),
                        ex.Message);
                    _windowService.ShowMessage(message);
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
