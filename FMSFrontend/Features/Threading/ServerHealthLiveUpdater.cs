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
        private readonly GlobalProperties _globalProperties;
        private readonly DispatcherTimer _timer;

        // 你可以把週期調整成 2~5 秒
        public ServerHealthLiveUpdater(IServerHealthService svc, GlobalProperties globalProperties)
        {
            _svc = svc;
            _globalProperties = globalProperties;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _timer.Tick += async (_, __) => await PollAsync();
        }

        private async Task PollAsync()
        {
            var sw = Stopwatch.StartNew();
            bool ok = false;

            try
            {
                ok = await _svc.CheckHealthAsync();
            }
            catch
            {
                ok = false;
            }
            finally
            {
                sw.Stop();
            }

            // DispatcherTimer 已在 UI 執行緒，不需要再做 Dispatcher.Invoke
            _globalProperties.IsServerAlive = ok;
            _globalProperties.LastLatencyMs = sw.ElapsedMilliseconds;
            _globalProperties.LastCheckedAt = DateTime.Now;
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();
        public void Dispose() => _timer.Stop();
    }
}
