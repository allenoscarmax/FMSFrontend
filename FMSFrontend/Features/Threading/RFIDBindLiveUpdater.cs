using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using IniFile;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FMSFrontend.Features.Threading
{
    public class RFIDBindLiveUpdater : IDisposable
    {
        private readonly IRfidService _svc;
        private readonly RFIDBindStore _store;
        private readonly DispatcherTimer _timer;
        public bool ReadTagFlag { get; set; } = false;
        public bool IsElectrode = false; //標籤狀態
        public int ElectrodeTagNumber = 2;
        public int WorkpieceTagNumber = 1;
        //private CancellationTokenSource? _currentUpdateCts; // 取消目前更新的 CancellationTokenSource
        private bool _isUpdating; // 用於避免重入的旗標
        public RFIDBindLiveUpdater(IRfidService svc, RFIDBindStore store)
        {
            _svc = svc;
            _store = store;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            try
            {
                INIFile ini = new INIFile(AppDomain.CurrentDomain.BaseDirectory + "Basesitting.ini");
                ElectrodeTagNumber = Convert.ToInt16(ini.Read("Prarm", "ElectrodeTagNumber"));
                WorkpieceTagNumber = Convert.ToInt16(ini.Read("Prarm", "WorkpieceTagNumber"));
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
        public void Dispose() => _timer.Stop();
    }

}
