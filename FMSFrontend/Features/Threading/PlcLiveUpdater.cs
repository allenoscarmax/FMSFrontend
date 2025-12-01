using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FMSFrontend.Features.Threading
{
    public class PlcLiveUpdater : IDisposable
    {
        private readonly IPlcService _svc;
        private readonly PlcStore _store;
        private readonly DispatcherTimer _timer;

        //private CancellationTokenSource? _currentUpdateCts; // 取消目前更新的 CancellationTokenSource
        private bool _isUpdating; // 用於避免重入的旗標

        public PlcLiveUpdater(IPlcService svc, PlcStore store)
        {
            _svc = svc;
            _store = store;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.6) };
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
        private async Task UpdateStatusAsync()
        {
            try
            {
                // === 第一段：更新 倉儲門 控制參數 ===
                var Dto = await _svc.GetALLMagazineParaAsync();
                if (Dto != null)
                    _store.ApplyMagazineParaDto(Dto);
            }
            catch //(Exception ex)
            {
                // TODO: 可加 log
                // ex.Message 或紀錄至 LogService
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
