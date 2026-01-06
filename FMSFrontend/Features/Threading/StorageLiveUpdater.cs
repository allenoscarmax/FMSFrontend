using ControlzEx.Standard;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Services.FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Models;
using FMSFrontend.Views.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;
namespace FMSFrontend.Features.Threading
{
    public class StorageLiveUpdater : IDisposable
    {
        private readonly IStorageService _svc_Storage;
        private readonly IElectrodeService _svc_electrode;
        private readonly IWorkpieceService _svc_Workpiece;
        private readonly IProbeService _svc_Probe;

        private readonly StorageStore _store;
        private readonly DispatcherTimer _timer;

        public string SelectTitle = "";

        //private CancellationTokenSource? _currentUpdateCts; // 取消目前更新的 CancellationTokenSource
        private bool _isUpdating; // 用於避免重入的旗標

        public StorageLiveUpdater(IElectrodeService electrodeService, IStorageService storageService,
            IWorkpieceService workpieceService, IProbeService probeService, StorageStore store)
        {
            _svc_electrode = electrodeService;
            _svc_Storage = storageService;
            _svc_Workpiece = workpieceService;
            _svc_Probe = probeService;

            _store = store;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.8) };
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
                var storage = await _svc_Storage.GetAllStorageAsync();
                if (storage == null) return false;
                _store.ApplyStorageDto(storage);
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
                                var eleDtos = await _svc_electrode.DB_GetElectrodesByTagSerialAsync(slot.Serial);
                                if (eleDtos != null)
                                {
                                    var eleDto = eleDtos.FirstOrDefault() ?? new ElectrodeDto();
                                    _store.ApplyElectrodeDto(eleDto, i, j); // 傳入 index
                                }
                                else //檢查是否為探針
                                {
                                    var probeDto = await _svc_Probe.DB_GetProbeByTagSerialAsync(slot.Serial);
                                    if (probeDto != null)
                                    {
                                        _store.ApplyProbeDto(probeDto, i, j); // 傳入 index
                                    }
                                    else
                                    {
                                        _store.ApplyNullDto(i, j); // 傳入 index
                                    }
                                }
                            }
                            else if (s.Kind == MaterialType.Workpiece) //檢查是否為工件
                            {
                                var workpieceDto = await _svc_Workpiece.GetWorkpieceByTagSerialAsync(slot.Serial);
                                if (workpieceDto != null)
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
            catch 
            {
                return false;
            }
        }
        public int Cnt = 0;
        public void Start()
        {
            _ = UpdateStatusAsync();
            _timer.Start();
        }
        public void Stop() => _timer.Stop();
        public void Dispose() => _timer.Stop();
    }
        

}
