using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
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

        //private CancellationTokenSource? _currentUpdateCts; // 取消目前更新的 CancellationTokenSource
        private bool _isUpdating; // 用於避免重入的旗標

        public RFIDBindLiveUpdater(IRfidService svc, RFIDBindStore store)
        {
            _svc = svc;
            _store = store;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
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
            //try
            //{
                if (ReadTagFlag)
                {
                    var ParasDto = await _svc.GetRFIDParasAsync();
                    if (ParasDto != null)
                        _store.ApplyParasDto(ParasDto);

                    var TagDto = await _svc.Read_Tag_IDAsync(0, 2);
                    if (TagDto != null)
                        _store.ApplyTagDto(TagDto);
                }
                else 
                {
                    var LogDto = await _svc.GetAllRFIDWriteLogAsync();
                    if (LogDto != null)
                        _store.ApplyRFIDBindPageDto(LogDto);
                }
                return true;
            //}
            //catch //(Exception ex)
            //{
                // TODO: 可加 log
                // ex.Message 或紀錄至 LogService
                //return false;
            //}
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
