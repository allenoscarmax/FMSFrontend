using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Models;
using FMSFrontend.SQL.Server;
using FMSFrontend.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FMSFrontend.SQL.Timer
{
    public class CycLiveUpdater : IDisposable
    {
        enum Page
        {
            ProductionLines = 0,
        }
        private readonly ISqlServer _sqlServer;
        private readonly IStorageService _svc_Storage;
        private readonly IElectrodeService _svc_electrode;
        private readonly IWorkpieceService _svc_Workpiece;
        private readonly IDevicesService _svc_devices;
        private readonly IWorksheetsService _svc_Worksheets;
        private readonly IAlarmService _svc_alarm;


        private readonly StorageStore _store;
        private readonly DispatcherTimer _timer;

        public string SelectTitle = "";

        //private CancellationTokenSource? _currentUpdateCts; // 取消目前更新的 CancellationTokenSource
        private bool _isUpdating; // 用於避免重入的旗標

        public CycLiveUpdater(ISqlServer sqlServerService,
            IStorageService storageService,
            IElectrodeService electrodeService,
            IWorkpieceService workpieceService,
            IDevicesService devicesService,
            IWorksheetsService worksheetsService,
            IAlarmService alarmService)
        {
            _sqlServer = sqlServerService;
            _svc_Storage = storageService;
            _svc_electrode = electrodeService;
            _svc_Workpiece = workpieceService;
            _svc_devices = devicesService;
            _svc_Worksheets = worksheetsService;
            _svc_alarm = alarmService;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.8) };
            _timer.Tick += async (_, __) =>
            {
                // 避免重入
                if (_isUpdating) return;
                _isUpdating = true;
                try
                {
                   // await UpdateStatusAsync();
                    await UpdateStatusAsyncBySQL();
                }
                finally
                {
                    _isUpdating = false;
                }
            };
        }
        public async Task<bool> UpdateStatusAsyncBySQL()
        {
            var storage = await _svc_Storage.GetAllStorageAsync();
            var ElectrodeDtos = await _svc_electrode.DB_GetAllElectrodeAsync();
            var WorkpieceDtos = await _svc_Workpiece.GetAllWorkpieceAsync();


            if (storage == null) return false;
            _store.ApplyStorageDto(storage);

            var electrodeLookup = (ElectrodeDtos ?? new List<ElectrodeDto>())
                .Where(x => !string.IsNullOrWhiteSpace(x.tagSerial))
                .GroupBy(x => x.tagSerial)
                .ToDictionary(g => g.Key, g => g.First());

            var workpieceLookup = (WorkpieceDtos ?? new List<WorkpieceDto>())
                .Where(x => !string.IsNullOrWhiteSpace(x.tagSerial))
                .GroupBy(x => x.tagSerial)
                .ToDictionary(g => g.Key, g => g.First());

            for (int i = 0; i < _store.StorageGroup.Storage.Count; i++)
            {
                StorageModel s = _store.StorageGroup.Storage[i];
                for (int j = 0; j < s.Slots.Count; j++)
                {
                    var slot = s.Slots[j];
                    if (!string.IsNullOrWhiteSpace(slot.Serial))
                    {
                        if (s.Kind == MaterialType.Electrode) //檢查是否為電極
                        {
                            if (electrodeLookup.TryGetValue(slot.Serial, out var eleDto))
                            {
                                if (IsProbe(eleDto))
                                {
                                    _store.ApplyProbeDto(ToProbeDto(eleDto), i, j); // 傳入 index
                                }
                                else
                                {
                                    _store.ApplyElectrodeDto(eleDto, i, j); // 傳入 index
                                }
                            }
                            else
                            {
                                _store.ApplyNullDto(i, j); // 傳入 index
                            }
                        }
                        else if (s.Kind == MaterialType.Workpiece) //檢查是否為工件
                        {
                            if (workpieceLookup.TryGetValue(slot.Serial, out var workpieceDto))
                            {
                                _store.ApplyWorkpieceDto(workpieceDto, i, j); // 傳入 index
                            }
                            else
                            {
                                _store.ApplyNullDto(i, j); // 傳入 index
                            }
                        }
                        else
                        {
                            _store.ApplyNullDto(i, j); // 傳入 index
                        }
                    }
                    else
                    {
                        _store.ApplyNullDto(i, j); // 傳入 index
                    }
                }
            }
            _store.ApplyStatusCount();
            _store.ApplySelectStorage(SelectTitle);
            return true;
        }
        public void Start()
        {
            _ = UpdateStatusAsyncBySQL();
            _timer.Start();
        }
        public void Stop() => _timer.Stop();
        public void Dispose() => _timer.Stop();

        private static bool IsProbe(ElectrodeDto dto)
            => !string.IsNullOrWhiteSpace(dto.electrodeType) && dto.electrodeType.Contains("Probe", StringComparison.OrdinalIgnoreCase)
               || !string.IsNullOrWhiteSpace(dto.electrodeName) && dto.electrodeName.Contains("Probe", StringComparison.OrdinalIgnoreCase);

        private static ProbeDto ToProbeDto(ElectrodeDto dto)
            => new ProbeDto
            {
                _id = dto._id,
                tagSerial = dto.tagSerial,
                probeName = dto.electrodeName,
                probeType = dto.electrodeType,
                currentLocation = dto.currentLocation,
                state = dto.state,
                restriction = dto.restriction,
                pairedEDM = dto.pairedEDM
            };
    }
        

}
