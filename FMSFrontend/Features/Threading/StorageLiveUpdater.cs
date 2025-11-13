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

        public StorageLiveUpdater(IElectrodeService electrodeService, IStorageService storageService,
            IWorkpieceService workpieceService, IProbeService probeService, StorageStore store)
        {
            _svc_electrode = electrodeService;
            _svc_Storage = storageService;
            _svc_Workpiece = workpieceService;
            _svc_Probe = probeService;

            _store = store;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += async (_, __) => await UpdateProductionLinesPageStatusAsync();
        }
        public async Task<bool> UpdateProductionLinesPageStatusAsync()
        {
            //try
            //{
            var storage = await _svc_Storage.GetAllStorageAsync();
            if (storage == null) return false;
            // 依 StorageName + StorageNumber 分組（例如 E + 1 → ES1、W + 1 → W1）
            var groups = storage
             .GroupBy(s => new { s.storageName, s.storageNumber })
             .OrderBy(g => g.Key.storageName)
             .ThenBy(g => g.Key.storageNumber)
             .ToList();     
            var storageGroup = new StorageGroupModel
            {
                Storage = new ObservableCollection<StorageModel>()
            };
            for (int i = 0; i < groups.Count; i++) // 建立每個庫
            {
                var g = groups[i];
                var Storage = new StorageModel();
                Storage.Name = g.Key.storageName;                       // 庫名稱
                Storage.Number = g.Key.storageNumber;                   // 庫編號
                Storage.Rows = Math.Max(1, g.Max(x => x.row));          //最大行數
                Storage.Columns = Math.Max(1, g.Max(x => x.column));    //最大列數
                Storage.Slots = new();
                for (int r = 1; r <= Storage.Rows; r++)
                {
                    for (int c = 1; c <= Storage.Columns; c++)
                    {
                        var rec = g.FirstOrDefault(x => x.row == r && x.column == c);
                        Slot slot = new Slot();
                        slot.Kind = MaterialType.None;
                        slot.Serial = rec?.ondeskTagserial ?? "";
                        slot.StorageStatus = rec?.state ?? "";
                        slot.StorageRestriction = rec?.restriction ?? false;
                       // slot.SlotCode = $"E:{}:{rec.row}:{rec.column}:1";
                        if (!string.IsNullOrWhiteSpace(slot.Serial))
                        {
                            if (Storage.Kind == MaterialType.Electrode) //檢查是否為電極
                            {
                                var eleDtos = await _svc_electrode.DB_GetElectrodesByTagSerialAsync(slot.Serial);
                                if (eleDtos != null && eleDtos.Any())
                                {
                                    var eleDto = eleDtos.FirstOrDefault();
                                    slot.Kind = MaterialType.Electrode;
                                    slot.Name = eleDto?.electrodeName ?? "";
                                    slot.ShortName = Regex.Match(slot.Name, @"_(\d+-[A-Za-z0-9]+)").Groups[1].Value;
                                    slot.MaterialStatus = eleDto?.state ?? "";
                                    slot.MaterialRestriction = eleDto?.restriction ?? false;
                                }
                                else //檢查是否為探針
                                {
                                    var probeDto = await _svc_Probe.DB_GetProbeByTagSerialAsync(slot.Serial);
                                    if (probeDto != null)
                                    {
                                        slot.Kind = MaterialType.Probe;
                                        slot.Name = probeDto.probeName ?? "";
                                        slot.ShortName = slot.Name;
                                        slot.MaterialStatus = probeDto.state ?? "";
                                        slot.MaterialRestriction = probeDto.restriction ?? false;
                                    }
                                }
                            }
                            else if (Storage.Kind == MaterialType.Workpiece) //檢查是否為工件
                            {
                                var workpieceDto = await _svc_Workpiece.GetWorkpieceByTagSerialAsync(slot.Serial);
                                if (workpieceDto != null)
                                {
                                    slot.Kind = MaterialType.Workpiece;
                                    slot.Name = workpieceDto?.workpieceName ?? "";
                                    slot.ShortName = Regex.Match(slot.Name, @"_(\d+-[A-Za-z0-9]+)").Groups[1].Value;
                                    slot.MaterialStatus = workpieceDto?.status ?? "";
                                    slot.MaterialRestriction = workpieceDto?.restriction ?? false;
                                }
                            }
                        }
                        Storage.Slots.Add(slot); // 加入槽位
                    }
                }

                //計算總數
                Storage.WaitingCount = Storage.Slots.Count(s => s.MaterialStatus == "Verified");
                Storage.ProcessingCount = Storage.Slots.Count(s => s.MaterialStatus == "Working");
                Storage.ErrorCount = Storage.Slots.Count(s => s.MaterialStatus == "Error");
                Storage.CompletedCount = Storage.Slots.Count(s => s.MaterialStatus == "Completed");
                Storage.RestrictionCount = Storage.Slots.Count(s => s.StorageRestriction == true);
                Storage.BookedCount = Storage.Slots.Count(s => s.StorageStatus == "Book");
                /*
                Storage.WaitingCount = Cnt;
                Storage.ProcessingCount = Cnt;
                Storage.ErrorCount = Cnt;
                Storage.CompletedCount = Cnt;
                Storage.RestrictionCount = Cnt;
                Storage.BookedCount = Cnt;
                */
                // 加入庫列表
                storageGroup.Storage.Add(Storage); 
            }
            // 若未指定 SelectTitle，預設選第一個
            if (string.IsNullOrWhiteSpace(SelectTitle) && storageGroup.Storage.Count > 0)
            {
                SelectTitle = storageGroup.Storage[0].Title;
            }
            if (!string.IsNullOrWhiteSpace(SelectTitle))
            {
                storageGroup.SelectStorage = storageGroup.Storage.FirstOrDefault(s => s.Title == SelectTitle) ?? new StorageModel();
            }
            storageGroup.WaitingTotal = storageGroup.Storage.Sum(s => s.WaitingCount);
            storageGroup.CompletedTotal = storageGroup.Storage.Sum(s => s.CompletedCount);
            storageGroup.ErrorTotal = storageGroup.Storage.Sum(s => s.ErrorCount);
            storageGroup.ProcessingTotal = storageGroup.Storage.Sum(s => s.ProcessingCount);
            storageGroup.RestrictionTotal = storageGroup.Storage.Sum(s => s.RestrictionCount);
            storageGroup.BookedTotal = storageGroup.Storage.Sum(s => s.BookedCount);
            //模擬測試
            Cnt++;
             _store.ApplyStorageGroupDto(storageGroup);
            return true;
            //}
            //catch //(Exception ex)
            //{
            // TODO: 可加 log
            // ex.Message 或紀錄至 LogService
            //return false;
            //}
        }
        public int Cnt = 0;
        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();
        public void Dispose() => _timer.Stop();
    }

}
