using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using FMSFrontend.Extensions;
using FMSFrontend.Interfaces;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using OSCARMAXFMS_V3.DBmodels;
using FMSFrontend.Views.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static FMSFrontend.ViewModels.MainWindowViewModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FMSFrontend.ViewModels
{
    public partial class RFIDBindPageViewModel : ObservableObject
    {
        public ObservableCollection<BurnRecord> BurnHistoryList { get; set; } = new ();
        public List<string> DateFilterOptions { get; set; } = new() { "今天", "過去7天", "自訂" };
        [ObservableProperty]
        private string selectedFilterOption = "今天";
        [ObservableProperty]
        private int selectedFilterIndex = 0;

        private DateTime? lastValidFromDate = DateTime.Today;
        private DateTime? lastValidToDate = DateTime.Today;

        private readonly IWindowService _windowService;
        private readonly IHttpService _httpService;

        // 用於管理 Fetch 呼叫的 CancellationTokenSource，避免重複堆疊執行
        private CancellationTokenSource? _fetchCts;

        public bool IsCustomDateMode => SelectedFilterOption == "自訂";
        partial void OnSelectedFilterOptionChanged(string value)
        {
            OnPropertyChanged(nameof(IsCustomDateMode));
            ApplyDateFilter();
            // 當改變篩選模式時重新抓取並套用新的日期範圍
            RefreshFetch();
        }

        partial void OnSelectedFilterIndexChanged(int value)
        {
            // 當以 index 選擇時，轉成對應的選項文字，讓現有的文字處理流程負責套用與抓取
            if (value >= 0 && value < DateFilterOptions.Count)
            {
                SelectedFilterOption = DateFilterOptions[value];
            }
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
            // 若為自訂模式且日期變更，重新抓取
            if (IsCustomDateMode) RefreshFetch();
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
            // 若為自訂模式且日期變更，重新抓取
            if (IsCustomDateMode) RefreshFetch();
        }

        public RFIDBindPageViewModel(IWindowService windowService, IHttpService httpService)
        {
            _windowService = windowService;
            _httpService = httpService;

            // 初始化 SelectedFilterIndex 根據 SelectedFilterOption
            SelectedFilterIndex = DateFilterOptions.IndexOf(SelectedFilterOption);

            // 進入時自動刷新（第一次載入）
            RefreshFetch();

            // 訂閱 MainWindowViewModel 的頁面刷新訊息：當切換到 RFIDBind 時重新抓取
            WeakReferenceMessenger.Default.Register<ValueChangedMessage<string>>(this, (r, message) =>
            {
                if (string.Equals(message.Value, "RFIDBind", StringComparison.Ordinal))
                {
                    RefreshFetch();
                }
            });
        }

        [RelayCommand]
        private void OpenMaterialTypeSelect()
        {
            if (_windowService.ShowMaterialTypeSelectWindow(out MaterialKind kind))
            {
                Window? window = kind switch
                {
                    MaterialKind.Electrode => new MaterialPairWindow(isElectrode: true),
                    MaterialKind.Workpiece => new MaterialPairWindow(isElectrode: false),
                    MaterialKind.Probe => new ProbePairWindow(),   // 新增的探針視窗
                    _ => null
                };

                window?.ShowDialog();


                // 關閉視窗後重新抓取並綁定
                RefreshFetch();
            }
        }

        [RelayCommand]
        private async Task Clear()
        {
            try
            {
                // 先顯示確認對話
                bool confirm = _windowService.ShowYesNoDialog("確定要清除所有燒錄歷史紀錄嗎？此動作無法復原。");
                if (!confirm) return;

                bool ok = await _httpService.SendPutAsync("RFIDMgmtModule/DB_DeleteAllRFIDWriteLogData", new { });
                if (ok)
                {
                    _windowService.ShowMessage("清除成功");
                    RefreshFetch(); // 重新抓資料
                }
                else
                {
                    _windowService.ShowMessage("清除失敗，請稍後重試");
                }
            }
            catch (Exception ex)
            {
                _windowService.ShowMessage($"清除時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 取消之前的 fetch（如有），建立新的 CancellationTokenSource，並啟動 FetchAndBindByStatusAsync。
        /// </summary>
        private void RefreshFetch()
        {
            try
            {
                _fetchCts?.Cancel();
                _fetchCts?.Dispose();
            }
            catch { /* 忽略 Dispose 錯誤 */ }

            _fetchCts = new CancellationTokenSource();
            // 不等待，背景執行；FetchAndBindByStatusAsync 內部支援 CancellationToken
            _ = FetchAndBindByStatusAsync(_fetchCts.Token);
        }

        private async Task FetchAndBindByStatusAsync(CancellationToken ct)
        {
            BurnHistoryList.Clear();
            JsonElement? json = await _httpService.GetJsonAsync<JsonElement>("RFIDMgmtModule/DB_GetAllRFIDWriteLog", ct);
            ct.ThrowIfCancellationRequested();

            List<RFIDWriteLog> rFIDWriteLog = (json.HasValue && json.Value.ValueKind != JsonValueKind.Undefined) ?
                 JsonSerializer.Deserialize<List<RFIDWriteLog>>(json.Value.GetRawText()) ?? new List<RFIDWriteLog>() :
                 new List<RFIDWriteLog>();

            BurnHistoryList.Clear();

            // 應用日期過濾：若 FromDate/ToDate 設定則以其範圍過濾（包含整天）
            DateTime from = FromDate?.Date ?? DateTime.MinValue;
            DateTime to = ToDate?.Date.AddDays(1).AddTicks(-1) ?? DateTime.MaxValue; // inclusive end of day

            foreach (var w in rFIDWriteLog)
            {
                if (w.timeStamp < from || w.timeStamp > to) continue;
                BurnHistoryList.Add(MapToBurnRecordData(w));
            }
        }

        private static BurnRecord MapToBurnRecordData(RFIDWriteLog ws)
        {
            if (ws == null) return new BurnRecord();
            return new BurnRecord
            {
                Time = ws.timeStamp,
                MaterialType = string.IsNullOrWhiteSpace(ws.type) ? (ws.objName ?? string.Empty) : ws.type,
                SerialNo = ws.srialNo ?? string.Empty,
                TagSerial = ws.tagSerial ?? string.Empty
            };
        }

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

        private void ShowWarning(string message)
        {
            FMSFrontend.Extensions.DialogMessageWindow dd = new Extensions.DialogMessageWindow(message);
            dd.Show();
        }

        public class BurnRecord
        {
            public DateTime Time { get; set; }
            public string MaterialType { get; set; } = "";
            public string SerialNo { get; set; } = "";
            public string TagSerial { get; set; } = "";
        }
    }

}
