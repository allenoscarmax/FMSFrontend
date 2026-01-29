using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using FMSFrontend.Extensions;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Interfaces;
using FMSFrontend.Models;
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


namespace FMSFrontend.ViewModels
{
    public partial class RFIDBindPageViewModel : ObservableObject
    {
        // === Services ===
        private readonly IWindowService _windowService;
        private readonly IHttpService _httpService;
        private readonly IRfidService _rfidService;
        private readonly IPlcService _plcService;
        private readonly IAuthorizationService _auth;
        private CancellationTokenSource? _currentUpdateCts; // 取消目前更新的 CancellationTokenSource
        // === Singleton ===
        public RFIDBindStore RfidBindStore { get; }
        public RFIDBindModel rFIDBindmodel => RfidBindStore.RfidBind;

        // === LiveUpdater ===
        public RFIDBindLiveUpdater _rfidUpdater;
        public RFIDBindPageViewModel(IWindowService windowService, IHttpService httpService,
        IRfidService iRFIDMgmtModuleService, IPlcService plcService, IAuthorizationService auth,
        RFIDBindStore rfidBindStore,
        RFIDBindLiveUpdater rfidUpdater)
        {
            _windowService = windowService;
            _httpService = httpService;
            _plcService = plcService;
            _rfidService = iRFIDMgmtModuleService;
            RfidBindStore = rfidBindStore;
            _rfidUpdater = rfidUpdater;
            _rfidUpdater.ReadTagFlag = true;
            _auth = auth;
        }

        // ====== 日期篩選 ======
        public ObservableCollection<string> DateFilterOptions { get; } = new() { "今天", "前7天", "自訂" };
        [ObservableProperty] private string selectedFilterOption = "今天";
        [ObservableProperty] private DateTime? fromDate = DateTime.Today;
        [ObservableProperty] private DateTime? toDate = DateTime.Today;
        [ObservableProperty] private bool isCustomDateMode;

        private bool _updatingDate;
        partial void OnSelectedFilterOptionChanged(string value)
        {
            IsCustomDateMode = value == "自訂";
            ApplyDateFilter(); //設定日期
            RefreshFetch();
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
                    _windowService.ShowMessage("選擇的日期範圍不能超過一個月");
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
            RefreshFetch();
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
                    _windowService.ShowMessage("選擇的日期範圍不能超過一個月");
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
            RefreshFetch();
        }

        [RelayCommand]
        private void ReadMaterialInformation()
        {
            if (!_auth.RequireLogin()) return;

            _rfidUpdater.ReadTagFlag = true;
            _rfidUpdater.ReadMaterInfoFlag = true;
            _rfidUpdater.Start();
            rFIDBindmodel.ReadElectrodeFlag = false;
            rFIDBindmodel.ReadWorkpieceFlag = false;
            // 先讓使用者選「電極 / 工件 / 探針」
            if (!_windowService.ShowMaterialTypeSelectWindow(out MaterialKind kind))
                return;
            switch (kind)
            {
                case MaterialKind.Electrode:
                    if (rFIDBindmodel.ReadElectrodeFlag)
                        _windowService.ShowMaterialInformation(rFIDBindmodel.electrode, rFIDBindmodel.Timeline);
                    else
                        ShowWarning("未讀取到電極資料");
                    break;
                case MaterialKind.Workpiece:
                    if (rFIDBindmodel.ReadWorkpieceFlag)
                        _windowService.ShowMaterialInformation(rFIDBindmodel.workpiece, rFIDBindmodel.Timeline);
                    else
                        ShowWarning("未讀取到工件資料");
                    break;
                default:
                    return;
            }
            // 視窗關閉後執行原本的後續流程\
            _rfidUpdater.Stop();
            _rfidUpdater.ReadTagFlag = false;
            _rfidUpdater.ReadMaterInfoFlag = false;
        }

        [RelayCommand]
        private void OpenMaterialTypeSelect()
        {
            if (!_auth.RequireLogin())
                return;

            // 先讓使用者選「電極 / 工件 / 探針」
            if (!_windowService.ShowMaterialTypeSelectWindow(out MaterialKind kind))
                return;

            // 交給 WindowService 照 kind 開對應的視窗
            switch (kind)
            {
                case MaterialKind.Electrode:
                    _windowService.ShowMaterialPairWindow(isElectrode: true);
                    break;

                case MaterialKind.Workpiece:
                    _windowService.ShowMaterialPairWindow(isElectrode: false);
                    break;

                case MaterialKind.Probe:
                    _windowService.ShowProbePairWindow();   // 下面一起補這個
                    break;

                default:
                    return;
            }

            // 視窗關閉後執行原本的後續流程
            _rfidUpdater.ReadTagFlag = false;
            RefreshFetch();   // 關閉視窗後重新抓取並綁定
        }
        [RelayCommand]
        private async Task BalluffReset()
        {
            if (!_auth.RequireLoginAndWriteOperation(30)) return;

            try
            {
                bool ok = await _plcService.BalluffPowerAsync(true);
                await Task.Delay(1000);
                ok = await _plcService.BalluffPowerAsync(false);
                await Task.Delay(1000);
                ok = await _rfidService.RFID_to_disconnect(0);
                await Task.Delay(300);
                ok = await _rfidService.RFID_to_connect(0);

                _windowService.ShowMessage("OK");
            }
            catch { }
        }

        [RelayCommand]
        private async Task Clear()
        {
            try
            {
                // 先顯示確認對話
                bool confirm = _windowService.ShowYesNoDialog("確定要清除所有燒錄歷史紀錄嗎？此動作無法復原。");
                if (!confirm) return;

                bool ok = await _rfidService.DeleteAllRFIDWriteLogDataAsync();
                if (ok)
                {
                    // _windowService.ShowMessage("清除成功");
                    rFIDBindmodel.BurnHistoryList.Clear(); // 清除本地列表
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
            _ = RefreshFetchAsync();
        }

        private async Task RefreshFetchAsync()
        {
            try
            {
                _currentUpdateCts?.Cancel();
                _currentUpdateCts?.Dispose();
            }
            catch { }
            _currentUpdateCts = new CancellationTokenSource();
            rFIDBindmodel.from = FromDate; // 更新模型的日期範圍
            rFIDBindmodel.to = ToDate; // 更新模型的日期範圍
            _rfidUpdater.ReadTagFlag = false;
            bool ok = await _rfidUpdater.UpdateStatusAsync();
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
