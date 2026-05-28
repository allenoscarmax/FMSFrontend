using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Models;
using IniFile;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Threading;

namespace FMSFrontend.Features.Threading
{
    public class RFIDBindLiveUpdater : IDisposable, INotifyPropertyChanged
    {
        private readonly IRfidService _svc;
        private readonly IElectrodeService _svc_Electrode;
        private readonly IWorkpieceService _svc_Workpiece;
        private readonly IPlcService _plcService;

        private readonly RFIDBindStore _store;
        private readonly DispatcherTimer _timer;
        private readonly SemaphoreSlim _rfidResetLock = new SemaphoreSlim(1, 1);
        private Task? _rfidResetTask;
        
        public bool ReadTagFlag { get; set; } = false;
        public bool ReadMaterInfoFlag { get; set; } = false; //讀取工件資訊
        private bool _isResetting;
        public bool IsResetting
        {
            get => _isResetting;
            private set
            {
                if (_isResetting == value) return;
                _isResetting = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsResetting)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;


        public bool IsElectrode = false; //標籤狀態
     //   public int ElectrodeTagNumber = 2; //Tag讀頭 鋐興:1 佑義:2
        public int ElectrodeTagNumber = 0; //測試
        public int WorkpieceTagNumber = 1; //Tag讀頭 鋐興:1 佑義:3
        //private CancellationTokenSource? _currentUpdateCts; // 取消目前更新的 CancellationTokenSource
        private bool _isUpdating; // 用於避免重入的旗標
        public RFIDBindLiveUpdater(IRfidService svc, IElectrodeService electrodeService, IWorkpieceService workpieceService, IPlcService plcService, RFIDBindStore store)
        {
            _svc = svc;
            _svc_Electrode = electrodeService;
            _svc_Workpiece = workpieceService;
            _plcService = plcService;
            _store = store;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            try
            {
                INIFile ini = new INIFile(AppDomain.CurrentDomain.BaseDirectory + "Basesitting.ini");
                //  ElectrodeTagNumber = Convert.ToInt16(ini.Read("Prarm", "ElectrodeTagNumber"));
                //  WorkpieceTagNumber = Convert.ToInt16(ini.Read("Prarm", "WorkpieceTagNumber"));
            }
            catch { }
            _timer.Tick += async (_, __) =>
            {
                // 避免重入
                if (_isUpdating) return;
                _isUpdating = true;
                try
                {
                    await UpdateStatusAsync();
                }
                finally
                {
                    _isUpdating = false;
                }
            };
        }
        public async Task<bool> UpdateStatusAsync()
        {
            try
            {
                if (ReadMaterInfoFlag)
                {
                    string? TagDto;
                    TagDto = await _svc.Read_Tag_IDAsync(0, ElectrodeTagNumber);
                    //TagDto = "1";
                    if (!string.IsNullOrEmpty(TagDto))
                    {
                        var eleDtos = await _svc_Electrode.DB_GetElectrodesByTagSerialAsync(TagDto);
                        if (eleDtos != null)
                        {
                            var eleDto = eleDtos.FirstOrDefault();
                            if (eleDto != null)
                            {
                                _store.ApplyEleDto(eleDto);
                            }
                        }
                    }
                    TagDto = await _svc.Read_Tag_IDAsync(0, WorkpieceTagNumber);
                    //TagDto = "10";
                    if (!string.IsNullOrEmpty(TagDto))
                    {
                        var WpDto = await _svc_Workpiece.GetWorkpieceByTagSerialAsync(TagDto);
                        if (WpDto != null)
                        {
                            _store.ApplyWpDto(WpDto);
                        }
                    }
                }

                if (ReadTagFlag)
                {
                    var ParasDto = await _svc.GetRFIDParasAsync();
                    if (ParasDto != null)
                    {
                        if (IsElectrode)
                            _store.ApplyParasDto(ParasDto, ElectrodeTagNumber);
                        else
                            _store.ApplyParasDto(ParasDto, WorkpieceTagNumber);
                    }
                    string? TagDto;
                    if (IsElectrode)
                    {
                        TagDto = await _svc.Read_Tag_IDAsync(0, ElectrodeTagNumber);
                    }
                    else
                    {
                        TagDto = await _svc.Read_Tag_IDAsync(0, WorkpieceTagNumber);
                    }
                    _store.ApplyTagDto(TagDto);
                }
                else
                {
                    var LogDto = await _svc.GetAllRFIDWriteLogAsync();
                    if (LogDto != null)
                        _store.ApplyRFIDBindPageDto(LogDto);
                }
                return true;
            }
            catch //(Exception ex)
            {
                return false;
                // TODO: 可加 log
                // ex.Message 或紀錄至 LogService
                //return false;
            }
        }

        public void Start()
        {
            _ = UpdateStatusAsync();
            _timer.Start();
        }
        
        public void Stop() => _timer.Stop();
        
        public async Task RfidResetAsync()
        {
            // 嘗試立即取得鎖，如果無法取得則表示有重置正在執行
            if (!await _rfidResetLock.WaitAsync(0))
            {
                // 無法立即取得鎖，直接返回，不等待
                return;
            }

            try
            {
                // 如果已經有重置任務在執行且未完成，直接返回
                if (_rfidResetTask != null && !_rfidResetTask.IsCompleted)
                {
                    return;
                }

                // 設定重置狀態為 true
                IsResetting = true;

                // 建立並追蹤重置任務
                _rfidResetTask = Task.Run(async () =>
                {
                    try
                    {
                        // 從新連線
                        bool ok = await _plcService.BalluffPowerAsync(true);
                        await Task.Delay(1000);
                        ok = await _plcService.BalluffPowerAsync(false);
                        await Task.Delay(1000);
                        ok = await _svc.RFID_to_disconnect(0);
                        await Task.Delay(300);
                        ok = await _svc.RFID_to_connect(0);
                    }
                    catch (Exception ex)
                    {
                    }
                });

                await _rfidResetTask;
            }
            finally
            {
                // 恢復狀態
                IsResetting = false;
                _rfidResetLock.Release();
            }
        }
        
        public void Dispose()
        {
            _timer.Stop();
            _rfidResetLock?.Dispose();
        }
    }

}
