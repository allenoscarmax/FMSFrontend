using FMSFrontend.Features.Dtos;
using FMSFrontend.Models;
using OSCARMAXFMS_V3.DBmodels;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace FMSFrontend.Features.Mappings
{
    public static class StorageMapping
    {
        public static void ApplyStorageDto(this List<StorageDto> dtos, ObservableCollection<StorageModel>  models)
        {
            if (dtos == null || models == null) return;
            // 根據庫名稱與庫編號分組
            var groups = dtos 
            .GroupBy(s => new { s.storageName, s.storageNumber })
            .OrderBy(g => g.Key.storageName)
            .ThenBy(g => g.Key.storageNumber)
            .ToList();
            for (int i = 0; i < groups.Count; i++) // 針對每個儲存庫進行處理
            {
                if (models.Count == i) models.Add(new StorageModel());
                var g = groups[i];
                models[i].Name = g.Key.storageName;                       // 庫名稱
                models[i].Number = g.Key.storageNumber;                   // 庫編號
                models[i].Rows = Math.Max(1, g.Max(x => x.row));          //最大行數
                models[i].Columns = Math.Max(1, g.Max(x => x.column));    //最大列數
                int SlotsCnt = 0;
                for (int r = 1; r <= models[i].Rows; r++)
                {
                    for (int c = 1; c <= models[i].Columns; c++)
                    {
                        if (models[i].Slots.Count == SlotsCnt)
                            models[i].Slots.Add(new Slot());
                        var rec = g.FirstOrDefault(x => x.row == r && x.column == c);
                        models[i].Slots[SlotsCnt].Kind = MaterialType.None;
                        models[i].Slots[SlotsCnt].Serial = rec?.ondeskTagserial ?? "";
                        models[i].Slots[SlotsCnt].StorageStatus = rec?.state ?? "";
                        models[i].Slots[SlotsCnt].StorageRestriction = rec?.restriction ?? false;
                        models[i].Slots[SlotsCnt].SlotCode = $"{rec?.storageName}:{rec?.storageNumber}:{rec?.region}:{rec?.column}:{rec?.row}";
                        SlotsCnt++;
                    }
                }
                while (models[i].Slots.Count > SlotsCnt)
                {
                    models[i].Slots.RemoveAt(models[i].Slots.Count - 1);
                }
            }
            while (models.Count > dtos.Count)
            {
                models.RemoveAt(models.Count - 1);
            }
        }
        public static void ApplyElectrodeDto(this ElectrodeDto dto, Slot model)
        {
            if (dto == null || model == null) return;

            model.Kind = MaterialType.Electrode;
            model.Name = dto?.electrodeName ?? "";
            model.ShortName = Regex.Match(model.Name, @"_(\d+-[A-Za-z0-9]+)").Groups[1].Value;
            if (model.ShortName == "") model.ShortName = model.Name;
            model.MaterialStatus = dto?.state ?? "";
            model.MaterialRestriction = dto?.restriction ?? false;
        }
        public static void ApplyWorkpieceDto(this WorkpieceDto dto, Slot model)
        {
            if (dto == null || model == null) return;
            model.Kind = MaterialType.Workpiece;
            model.Name = dto?.workpieceName ?? "";
            model.ShortName = Regex.Match(model.Name, @"-(\d+_\d+-[A-Za-z]+)$").Groups[1].Value;
            if (model.ShortName == "") model.ShortName = model.Name;
            model.MaterialStatus = dto?.status ?? "";
            model.MaterialRestriction = dto?.restriction ?? false;
        }
        public static void ApplyProbeDto(this ProbeDto dto, Slot model)
        {
            if (dto == null || model == null) return;
            model.Kind = MaterialType.Probe;
            model.Name = dto.probeName ?? "";
            model.ShortName = model.Name;
            model.MaterialStatus = dto.state ?? "";
            model.MaterialRestriction = dto.restriction ?? false;
        }

    }
}
