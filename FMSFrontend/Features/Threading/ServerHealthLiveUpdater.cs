using FMSFrontend.Features.Services;
using FMSFrontend.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FMSFrontend.Features.Threading
{
    public sealed class ServerHealthLiveUpdater : IDisposable
    {
        private readonly IServerHealthService _svc;
        private readonly GlobalProperties _global;
        private readonly DispatcherTimer _timer;

        // 你可以把週期調整成 2~5 秒
        public ServerHealthLiveUpdater(IServerHealthService svc, GlobalProperties globalProperties)
        {
            _svc = svc;
            _global = globalProperties;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _timer.Tick += async (_, __) => await PollAsync();
        }

        private async Task PollAsync()
        {
            // 開路時也可以探測，但要節流
            if (!_global.CanProbeHealth(openStateMinIntervalSeconds: 10, normalMinIntervalSeconds: 3))
                return;

            var sw = Stopwatch.StartNew();
            bool ok;
            try { ok = await _svc.CheckHealthAsync(); }
            catch { ok = false; }
            finally { sw.Stop(); }

            _global.LastLatencyMs = sw.ElapsedMilliseconds;
            _global.LastCheckedAt = DateTime.Now;

            if (ok) _global.RecordSuccess();
            else _global.RecordFailure();
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();
        public void Dispose() => _timer.Stop();
    }
}
