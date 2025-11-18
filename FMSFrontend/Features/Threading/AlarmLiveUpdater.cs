using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Dtos.Database;
using FMSFrontend.Features.Mappings;
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
namespace FMSFrontend.Features.Threading
{
    public class AlarmLiveUpdater : IDisposable
    {
        private readonly IAlarmService _svc_Alarms;
   
        private readonly AlarmStore _store;
        private readonly DispatcherTimer _timer;

        public string SelectName =""; // 選擇的機台編號

        //private CancellationTokenSource? _currentUpdateCts; // 取消目前更新的 CancellationTokenSource
        private bool _isUpdating; // 用於避免重入的旗標

        public AlarmLiveUpdater(IAlarmService AlarmsService,
            AlarmStore store)
        {
            _svc_Alarms = AlarmsService;

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
            //Try
            //{
            //取得所有機器基本資料
            List<ErrorMessageLogDto>? dtos = await _svc_Alarms.GetCurrentErrorMessageLogAsync();

            _store.ApplyErrorMessageLogDto(dtos);
            return true;
            //}
            //catch //(Exception ex)
            //{
            //    return false;
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
