using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FMSFrontend.Features.Threading
{
    public class AMRLiveUpdater : IDisposable
    {
        private readonly IAMRService _amrService;
        private readonly AMRStore _amrStore;
        private readonly DispatcherTimer _timer;
        private bool _isUpdating;

        public AMRLiveUpdater(IAMRService amrService, AMRStore amrStore)
        {
            _amrService = amrService;
            _amrStore = amrStore;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += async (_, __) =>
            {
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

        public async Task UpdateStatusAsync()
        {
            try
            {
                var amrDto = await _amrService.GetAgvParaAsync();
                if (amrDto != null)
                    _amrStore.ApplyAmrDto(amrDto);
            }
            catch
            {
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
