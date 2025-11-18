using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Services.FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Models;
using FMSFrontend.Views.Windows;
using OSCARMAXFMS_V3.DBmodels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;
using FMSFrontend.Features.Mappings;
namespace FMSFrontend.Features.Threading
{
    public class CommandScheduleLiveUpdater : IDisposable
    {
        private readonly ICommandScheduleService _svc;

        private readonly CommandScheduleStore _store;
        private readonly DispatcherTimer _timer;

        public string SelectName =""; // 選擇的機台編號

        //private CancellationTokenSource? _currentUpdateCts; // 取消目前更新的 CancellationTokenSource
        private bool _isUpdating; // 用於避免重入的旗標

        public CommandScheduleLiveUpdater(ICommandScheduleService CommandSchedulesService, CommandScheduleStore store)
        {
            _svc = CommandSchedulesService;
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
            // 取得派工資料
            var cmdDtos = await _svc.GetAllCommandScheduleAsync() ?? new List<CommandStructDto>();
            _store.ApplyCommandScheduleDto(cmdDtos);
            return true;
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
