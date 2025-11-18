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
    public class SettingLiveUpdater : IDisposable
    {
        private readonly IDevicesService _svc_Device;
        private readonly IPlcService _svc_PLC;
        private readonly IAppointmentMaintenanceService _svc_IAppointment;

        private readonly SettingStore _store;
        private readonly DispatcherTimer _timer;

        public string SelectName =""; // 選擇的機台編號

        //private CancellationTokenSource? _currentUpdateCts; // 取消目前更新的 CancellationTokenSource
        private bool _isUpdating; // 用於避免重入的旗標

        public SettingLiveUpdater(
            IElectrodeService electrodeService,
            IWorkpieceService workpieceService,
            IProbeService probeService,
            SettingStore store)
        {
            _svc_Settings = SettingsService;
            _svc_electrode = electrodeService;
            _svc_Workpiece = workpieceService;
            _svc_Probe = probeService;

            _store = store;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += async (_, __) =>
            {
                // 避免重入
                if (_isUpdating) return;
                _isUpdating = true;
                try
                {
                }
                finally
                {
                    _isUpdating = false;
                }
            };
        }
        int Cnt = 0;
        public async Task<bool> UpdateDeviceAsync()
        {
            //try
            //{
            //取得所有機器基本資料
            List<SettingModel> Settings = new List<SettingModel>();
            List<Devices> dDtos = await _svc_Device.GetAllDevicesAsync()?? new List<DevicesDto>();

            for (int i = 0; i < dDtos.Count; i++)
            {
                SettingModel setting = new SettingModel();
                setting.DeviceID = dDtos[i].DeviceID;
                setting.DeviceName = dDtos[i].DeviceName;
                setting.IPAddress = dDtos[i].IPAddress;
                setting.Port = dDtos[i].Port;
                setting.DeviceType = dDtos[i].DeviceType;
                setting.Location = dDtos[i].Location;
                setting.MachineType = dDtos[i].MachineType;
                setting.IsActive = dDtos[i].IsActive;
                Settings.Add(setting);
            }
            return true;
            //}
            //catch //(Exception ex)
            //{
            //    return false;
            //}
        }
        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();
        public void Dispose() => _timer.Stop();
    }

}
