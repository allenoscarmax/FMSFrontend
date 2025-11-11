using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Models;
using FMSFrontend.Views.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FMSFrontend.Features.Threading
{
    public class ProductionLinesLiveUpdater : IDisposable
    {
        private readonly IElectrodeService _svc_electrode;
        private readonly IMachinesService _svc_Machine;
        private readonly IStorageService _svc_Storage;
        private readonly IWorkpieceService _svc_Workpiece;
  
        private readonly ProductionLinesStore _store;
        private readonly DispatcherTimer _timer;
        public string PageName = "";
        public ProductionLinesLiveUpdater(IElectrodeService electrodeService, IMachinesService machinesService, 
            IStorageService storageService, IWorkpieceService workpieceService, ProductionLinesStore store)
        {
            _svc_electrode = electrodeService;
            _svc_Machine = machinesService;
            _svc_Storage = storageService;
            _svc_Workpiece = workpieceService;

            _store = store;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += async (_, __) => await UpdateProductionLinesPageStatusAsync();
        }
        public async Task<bool> UpdateProductionLinesPageStatusAsync()
        {
            //try
            //{

            switch (PageName)
            {
                case "StorageUnitControl":
                case "StorageUnitMiniControl":
                    var storage = await _svc_Storage.GetAllStorageAsync();
                    if (storage == null) return false;
                    _store.ApplyStorageDto(storage);

                    var SerialList = _store.ReadSerialList();
                    foreach (Slot s in SerialList)
                    {
                        var wpDto = await _svc_Workpiece.GetWorkpieceByTagSerialAsync(s.Serial);
                        if (wpDto != null)
                            _store.ApplyWorkpieceDto(wpDto);
                        var eleDto = await _svc_electrode.GetElectrodesbyTagSerialAsync(s.Serial);
                        if (eleDto != null)
                            _store.ApplyElectrodeDto(eleDto);
                    }
                    break;
                case "ProductionLines":
                    break;
                case "ProductionLines":
                    break;
            }
            return true;
            //}
            //catch //(Exception ex)
            //{
            // TODO: 可加 log
            // ex.Message 或紀錄至 LogService
            //return false;
            //}
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();
        public void Dispose() => _timer.Stop();
    }

}
