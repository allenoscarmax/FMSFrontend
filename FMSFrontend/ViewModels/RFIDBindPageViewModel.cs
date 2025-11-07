using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using FMSFrontend.Extensions;
using FMSFrontend.Interfaces;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Models;

namespace FMSFrontend.ViewModels
{
    public partial class RFIDBindPageViewModel : ObservableObject
    {
        //RFID
        private readonly IRfidService _RfidService;
        public RFIDBindStore RfidBindStore { get; }
        public RFIDBindModel rFIDBindmodel  => RfidBindStore.RfidBind;
        public RFIDBindLiveUpdater _rfidUpdater;

        //public ObservableCollection<BurnRecord> BurnHistoryList { get; set; } = new (); //移至rFIDBindmodel 中更新 
        public List<string> DateFilterOptions { get; set; } = new() { "今天", "過去7天", "自訂" };
        
        private DateTime? lastValidFromDate = DateTime.Today;
        private DateTime? lastValidToDate = DateTime.Today;

        private readonly IWindowService _windowService;
        private readonly IHttpService _httpService;

        // 用於管理 Fetch 呼叫的 CancellationTokenSource，避免重複堆疊執行
        private CancellationTokenSource? _fetchCts;

        public bool IsCustomDateMode => SelectedFilterOption == "自訂";
       
        [ObservableProperty] private string selectedFilterOption = "今天";
        partial void OnSelectedFilterOptionChanged(string value)
        {
            OnPropertyChanged(nameof(IsCustomDateMode));
            ApplyDateFilter();

            // 當改變篩選模式時重新抓取並套用新的日期範圍
            RefreshFetch();
        }
        [ObservableProperty] private int selectedFilterIndex = 0;

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

        public RFIDBindPageViewModel(IWindowService windowService, IHttpService httpService,
            IRfidService iRFIDMgmtModuleService, RFIDBindStore rfidBindStore, RFIDBindLiveUpdater rfidUpdater)
        {
            _windowService = windowService;
            _httpService = httpService;

            _RfidService = iRFIDMgmtModuleService;
             RfidBindStore = rfidBindStore;
            _rfidUpdater = rfidUpdater;
            _rfidUpdater.ReadTagFlag = true;

            // 初始化 SelectedFilterIndex 根據 SelectedFilterOption
            SelectedFilterIndex = DateFilterOptions.IndexOf(SelectedFilterOption);
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
                _rfidUpdater.ReadTagFlag = false;
                RefreshFetch();   // 關閉視窗後重新抓取並綁定
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

                bool ok = await _RfidService.DeleteAllRFIDWriteLogDataAsync();
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

        // 當頁面載入時啟動
        public void OnPageActivated()
        {
            // _rfidUpdater.Start();
            RefreshFetch(); // 重新抓資料
        }

        // 當頁面卸載時停止
        public void OnPageDeactivated()
        {
          _rfidUpdater.Stop();
        }

        private void RefreshFetch()
        {
            try
            {
                _fetchCts?.Cancel();
                _fetchCts?.Dispose();
            }   
            catch { }
            _fetchCts = new CancellationTokenSource();
            _ = FetchAndBindByStatusAsync(_fetchCts.Token);
        }

        private async Task FetchAndBindByStatusAsync(CancellationToken ct)
        {
            rFIDBindmodel.from = FromDate; // 更新模型的日期範圍
            rFIDBindmodel.to = ToDate; // 更新模型的日期範圍
            _rfidUpdater.ReadTagFlag = false;
            _ = await _rfidUpdater.UpdateRFIDBindPageStatusAsync();
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
