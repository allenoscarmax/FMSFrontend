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
    public class RobotLiveUpdater : IDisposable
    {
        private readonly IRobotService _svc;
        private readonly RobotStore _store;
        private readonly DispatcherTimer _timer;

        public RobotLiveUpdater(IRobotService svc, RobotStore store)
        {
            _svc = svc;
            _store = store;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _timer.Tick += async (_, __) => await UpdateRobotStatusAsync();
        }
        private async Task UpdateRobotStatusAsync()
        {
            try
            {
                // === 第一段：更新 ASRS 控制參數 ===
                var asrsDto = await _svc.GetAsrsParameterAsync();
                if (asrsDto != null)
                    _store.ApplyAsrsDto(asrsDto);

                // === 第二段：更新 Robot DB 基本資料 ===
                var robotDtos = await _svc.DB_GetAllRobotsAsync();
                if (robotDtos != null && robotDtos.Count > 0)
                {
                    // 假設只取第一台，或你可依 robotCode / productionLine 篩選
                    var robotDto = robotDtos.FirstOrDefault();
                    if (robotDto != null)
                        _store.ApplyRobotDto(robotDto);
                }
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
