using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Extensions;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Interfaces;
using FMSFrontend.Models;
using FMSFrontend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FMSFrontend.ViewModels.Windows
{
    public partial class ProbePairViewModel : ObservableObject
    {
        private readonly Window _window;
        private readonly IWindowService _windowService;
        private readonly IHttpService _httpService;
        // === Services ===
        private readonly IProbeService _ProbeService;
        private readonly IRfidService _RfidService;
        // === Singleton ===
        public RFIDBindStore RfidBindStore { get; } = new();
        public RFIDBindModel RfidBindmodel => RfidBindStore.RfidBind;
        // ==LiveUpdater===
        public RFIDBindLiveUpdater _rfidUpdater;

        public ProbePairViewModel(Window window, IWindowService windowService, IHttpService httpService,
            IElectrodeService electrodeService, IProbeService probeService, IRfidService rfidService,
            IWorkpieceService workpieceService, IWorksheetsService worksheetService,
            RFIDBindStore rFIDBindStore, RFIDBindLiveUpdater rFIDBindLiveUpdater)
        {
            _window = window;
            _windowService = windowService;
            _httpService = httpService;

            // === Services ===
            _ProbeService = probeService;
            _RfidService = rfidService;
            // === Singleton ===
            RfidBindStore = rFIDBindStore;
            // ==LiveUpdater===
            _rfidUpdater = rFIDBindLiveUpdater;

            // 監聽 TagSerial 變化以更新配對按鈕可用狀態
            RfidBindmodel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(RFIDBindModel.TagSerial))
                {
                    PairCommand.NotifyCanExecuteChanged();
                }
            };
        }
        // 使視窗可在 Loaded/Unloaded 中呼叫的公開方法
        public void OnPageActivated()
        {
            _rfidUpdater.Start();
            _rfidUpdater.ReadTagFlag = true;
            // 確保進入頁面時更新一次按鈕狀態
            PairCommand.NotifyCanExecuteChanged();
        }
        public void OnPageDeactivated()
        {
            _rfidUpdater.Stop();
            _rfidUpdater.ReadTagFlag = false;
        }

        // 僅在有 Tag 時可配對
        private bool CanPair() => !string.IsNullOrWhiteSpace(RfidBindmodel.TagSerial);

        // Pair
        [RelayCommand(CanExecute = nameof(CanPair))]
        private async Task Pair()
        {
            ProbeDto probe = new();
            var OK = false;
            try
            {
                List<ProbeDto> List = await _ProbeService.GetAllProbeAsync() ?? new List<ProbeDto>();
                if (List == null) return;
                probe = List.FirstOrDefault() ?? new ProbeDto();
            }
            catch { _windowService.ShowMessage("發生錯誤"); }
            try
            {
                probe.tagSerial = RfidBindmodel.TagSerial ?? "";
                OK = await _ProbeService.UpdateProbeDataAsync(probe);
                if (!OK)
                {
                    _windowService.ShowMessage("Probe上傳失敗");
                    return;
                }
            }
            catch { _windowService.ShowMessage("發生錯誤"); }
            try
            {
                var RFIDWriteLog = new RFIDWriteLogDto
                {
                    timeStamp = DateTime.Now,
                    type = "Probe",
                    tagSerial = RfidBindmodel.TagSerial ?? "",
                    srialNo = "",
                    objName = "",
                };
                OK = await _RfidService.InsertNewRFIDWriteLogDataAsync(RFIDWriteLog);
                if (!OK)
                {
                    _windowService.ShowMessage("RFIDWriteLog上傳失敗: ");
                    return;
                }
                _windowService.ShowMessage($"Probe上傳成功!");

                // 配對成功後離開
                _window?.Close();
            }
            catch { _windowService.ShowMessage("發生錯誤"); }
        }

        [RelayCommand]
        private void Back()
        {
            _window?.Close();

            //  var selectWindow = new MaterialTypeSelectWindow();
            if (_windowService.ShowMaterialTypeSelectWindow(out MaterialKind kind))
            {
                Window? window = kind switch
                {
                    MaterialKind.Electrode => new MaterialPairWindow(isElectrode: true),
                    MaterialKind.Workpiece => new MaterialPairWindow(isElectrode: false),
                    MaterialKind.Probe => new FMSFrontend.Views.Windows.ProbePairWindow(),   // 新增的探針視窗
                    _ => null
                };

                window?.ShowDialog();
            }
        }

    }
}
