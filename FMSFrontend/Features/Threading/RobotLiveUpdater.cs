using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Models;
using OSCARMAXFMS_V3.DBmodels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FMSFrontend.Features.Threading
{
    public class RobotLiveUpdater : IDisposable
    {
        private readonly IRobotService _svc;
        private readonly IElectrodeService _svc_electrode;
        private readonly IWorkpieceService _svc_Workpiece;
        private readonly IProbeService _svc_Probe;

        private readonly RobotStore _store;
        private readonly DispatcherTimer _timer;
        private string MaterialSerial = "";
        public RobotLiveUpdater(IRobotService svc, IElectrodeService electrodeService,
           IWorkpieceService workpieceService, IProbeService probeService, RobotStore store)
        {
            _svc = svc;
            _svc_electrode = electrodeService;
            _svc_Workpiece = workpieceService;
            _svc_Probe = probeService;

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
                    if (string.IsNullOrEmpty(_store.Robot.OnDeckObjSerial))
                    {
                        _store.Robot.MaterialName = "";
                        _store.Robot.MaterialShortName = "—";
                        _store.Robot.MaterialKind = "None";
                        MaterialSerial = _store.Robot.OnDeckObjSerial;
                    }
                    else if (MaterialSerial != _store.Robot.OnDeckObjSerial)
                    {
                        var e = await _svc_electrode.DB_GetElectrodesByTagSerialAsync(_store.Robot.OnDeckObjSerial);
                        if (e != null && e.FirstOrDefault() != null) //為電極
                        {
                            _store.Robot.MaterialName = ((ElectrodeDto)(e.First())).electrodeName;
                            _store.Robot.MaterialShortName ="電極 : "+
                                Regex.Match(_store.Robot.MaterialName, @"_(\d+-[A-Za-z0-9]+)").Groups[1].Value;
                            _store.Robot.MaterialKind = "Electrode";
                            MaterialSerial = _store.Robot.OnDeckObjSerial;
                            return;
                        }
                        var w = await _svc_Workpiece.GetWorkpieceByTagSerialAsync(_store.Robot.OnDeckObjSerial);
                        if (w != null) //為工件
                        {
                            _store.Robot.MaterialName = ((WorkpieceDto)w).workpieceName;
                            _store.Robot.MaterialKind = "Workpiece";
                            _store.Robot.MaterialShortName = "工件 : " +
                              Regex.Match(_store.Robot.MaterialName, @"-(\d+_\d+-[A-Za-z]+)$").Groups[1].Value;
                            MaterialSerial = _store.Robot.OnDeckObjSerial;
                            return;
                        }
                        var p = await _svc_Probe.DB_GetProbeByTagSerialAsync(_store.Robot.OnDeckObjSerial);
                        if (p != null) //為探針
                        {
                            _store.Robot.MaterialName = ((ProbeDto)p).probeName;
                            _store.Robot.MaterialShortName = _store.Robot.MaterialName;
                            
                               
                            _store.Robot.MaterialKind = "Probe";
                            MaterialSerial = _store.Robot.OnDeckObjSerial;
                            return;
                        }
                        // 若皆無符合，則設為空
                        _store.Robot.MaterialName = "";
                        _store.Robot.MaterialShortName = "—";
                        _store.Robot.MaterialKind = "None";
                        MaterialSerial = _store.Robot.OnDeckObjSerial;
                    }
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
