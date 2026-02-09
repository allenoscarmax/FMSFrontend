using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Models;
using FMSFrontend.Views.Windows;
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
    public class MachineLiveUpdater : IDisposable
    {
        private readonly IMachinesService _svc_Machines;
        private readonly IElectrodeService _svc_electrode;
        private readonly IWorkpieceService _svc_Workpiece;
        private readonly IProbeService _svc_Probe;

        private readonly MachineStore _store;
        private readonly DispatcherTimer _timer;

        public string SelectName = ""; // 選擇的機台編號

        //private CancellationTokenSource? _currentUpdateCts; // 取消目前更新的 CancellationTokenSource
        private bool _isUpdating; // 用於避免重入的旗標

        public MachineLiveUpdater(IMachinesService machinesService,
            IElectrodeService electrodeService,
            IWorkpieceService workpieceService,
            IProbeService probeService,
            MachineStore store)
        {
            _svc_Machines = machinesService;
            _svc_electrode = electrodeService;
            _svc_Workpiece = workpieceService;
            _svc_Probe = probeService;

            _store = store;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.4) };
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
            try
            {
                //取得所有機器基本資料
                List<MachineModel> machines = new List<MachineModel>();
                List<MachinesDto>? mDtos = await _svc_Machines.GetAllMachinesAsync();
                if (mDtos != null && mDtos.Count > 0)
                {
                    _store.ApplyMachinesDto(mDtos);
                    for (int i = 0; i < mDtos.Count; i++)
                    {
                        //如果有電極序號 讀取電極資訊
                        if (!string.IsNullOrEmpty(mDtos[i].onDeckElectrodeSerial))
                        {
                            string serial = mDtos[i].onDeckElectrodeSerial;
                            List<ElectrodeDto>? eDtos = await _svc_electrode.DB_GetElectrodesByTagSerialAsync(serial);
                            if (eDtos != null && eDtos.Count > 0)
                            {
                                var eDto = eDtos.FirstOrDefault();
                                if (eDto != null)
                                    _store.ApplyElectrodeDto(eDto, i);
                            }
                            else
                            {
                                ProbeDto? pDto = await _svc_Probe.DB_GetProbeByTagSerialAsync(serial);

                                if (pDto != null)
                                {
                                    _store.ApplyProbeDto(pDto, i);
                                }
                                else _store.ApplyNull(i, true);
                            }
                        }
                        else _store.ApplyNull(i, true);
                        //如果有工件序號讀取,工件資訊
                        if (!string.IsNullOrEmpty(mDtos[i].onDeckWorkpieceSerial))
                        {
                            string serial = mDtos[i].onDeckWorkpieceSerial;
                            var wDto = await _svc_Workpiece.GetWorkpieceByTagSerialAsync(serial);
                            if (wDto != null)
                            {
                                _store.ApplyWorkpieceDto(wDto, i);
                            }
                            else _store.ApplyNull(i, false);
                        }
                        else _store.ApplyNull(i, false);
                        //取得機台資訊
                        if (mDtos[i].machineCode.Contains("EDM"))
                        {
                            var oscarDto = await _svc_Machines.GetMachineDataAsync(mDtos[i].machineNumber - 1);
                            if (oscarDto != null)
                            {
                                _store.ApplyOscarmaxMachineParaDto(oscarDto, i);
                            }
                        }
                    }
                }
                return true;
            }
            catch //(Exception ex)
            {
                return false;
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
