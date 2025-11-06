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
    public class RFIDBindLiveUpdater : IDisposable
    {
        private readonly IRFIDMgmtModuleService _svc;
        private readonly RFIDBindStore _store;
        private readonly DispatcherTimer _timer;

        public RFIDBindLiveUpdater(IRFIDMgmtModuleService svc, RFIDBindStore store)
        {
            _svc = svc;
            _store = store;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _timer.Tick += async (_, __) => await UpdateRFIDBindPageStatusAsync();
        }
        private async Task UpdateRFIDBindPageStatusAsync()
        {
            try
            {
                // === 第一段：更新 ASRS 控制參數 ===
                var Dto = await _svc.GetAllRFIDWriteLogAsync();
                if (Dto != null)
                    _store.ApplyRFIDBindPageDto(Dto);
            }
            catch //(Exception ex)
            {
                // TODO: 可加 log
                // ex.Message 或紀錄至 LogService
            }
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();
        public void Dispose() => _timer.Stop();
    }

}
