using FMSFrontend.Features.Dtos;
using FMSFrontend.Models;
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
                // models[i].Rows = Math.Max(1, g.Max(x => x.row));          //最大行數為
                
                // 20260119 鋐興追加
                // 依 region 分組，計算每個 region 的最大 row，並依 region 排序
                var regionInfos = g
                    .GroupBy(x => x.region)
                    .OrderBy(grp => grp.Key)
                    .Select(grp => new
                    {
                        Region = grp.Key,
                        MaxRow = Math.Max(1, grp.Max(x => x.row))
                    })
                    .ToList();
                models[i].Rows = Math.Max(1, regionInfos.Sum(x => x.MaxRow));// Rows 為所有 region 的最大 row 數加總

                //End 20260119 鋐興追加

                models[i].Columns = Math.Max(1, g.Max(x => x.column));    //最大列數
                int SlotsCnt = 0;

                /*
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
                */

                // 依 region 逐一排列 row，再依 column
                foreach (var regionInfo in regionInfos)
                {
                    for (int r = 1; r <= regionInfo.MaxRow; r++)
                    {
                        for (int c = 1; c <= models[i].Columns; c++)
                        {
                            if (models[i].Slots.Count == SlotsCnt)
                                models[i].Slots.Add(new Slot());

                            // 取出該 region、row、column 的紀錄
                            var rec = g.FirstOrDefault(x => x.region == regionInfo.Region && x.row == r && x.column == c);

                            models[i].Slots[SlotsCnt].Kind = MaterialType.None;
                            models[i].Slots[SlotsCnt].Serial = rec?.ondeskTagserial ?? "";
                            models[i].Slots[SlotsCnt].StorageStatus = rec?.state ?? "";
                            models[i].Slots[SlotsCnt].StorageRestriction = rec?.restriction ?? false;
                            models[i].Slots[SlotsCnt].SlotCode = $"{rec?.storageName}:{rec?.storageNumber}:{rec?.region}:{rec?.column}:{rec?.row}";
                            SlotsCnt++;
                        }
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
           
            model.ShortName = ShortNameConversion(true, dto?.electrodeName ?? "");  //20260120佑義要求修改電極名稱規則

            model.MaterialStatus = dto?.state ?? "";
            model.MaterialRestriction = dto?.restriction ?? false;
            //庫存資訊
            model.Location = dto?.currentLocation ?? "";
            model.Worksheet = dto?.worksheetNumber ?? "";
            model.Program = dto?.edmpgm ?? "";


        }
        private static string ShortNameConversion(bool isElectrode, string Name)
        {
            // 電極名稱規則修改為 末三碼-序號+字母 (A,B,C...)，工件名稱規則修改為 末三碼-序號
            try
            {
                var parts = Name.Split('-');
                //顯示末三碼,不足三碼顯示全部
                var mainNo = parts[0].Length >= 3 ? parts[0].Substring(parts[0].Length - 3) : parts[0];
                //取得會最尾巴位文字
                var seqNo = parts.Length > 1 ? parts[parts.Length - 1] : "";
                int n = 0;
                if (isElectrode)
                {
                    if (int.TryParse(seqNo, out n))
                    {
                        return $"{mainNo}-{seqNo}{(char)('A' + int.Parse(seqNo) - 1)}";
                    }
                    else
                    {
                        return $"{mainNo}-{seqNo}";
                    }
                }
                else
                {
                    return $"{mainNo}-{seqNo}";
                }
            }
            catch { }
            return Name;
        }
        public static void ApplyWorkpieceDto(this WorkpieceDto dto, Slot model)
        {
            if (dto == null || model == null) return;
            model.Kind = MaterialType.Workpiece;
            model.Name = dto?.workpieceName ?? "";
            model.ShortName = ShortNameConversion(false, model.Name);  //20260120佑義要求修改電極名稱規則
            model.MaterialStatus = dto?.status ?? "";
            model.MaterialRestriction = dto?.restriction ?? false;
            //庫存資訊
            model.Location = dto?.currentLocation ?? "";
            model.Worksheet = dto?.worksheetNumber ?? "";
            model.Program = dto?.edmpgm ?? "";
        }
        public static void ApplyProbeDto(this ProbeDto dto, Slot model)
        {
            if (dto == null || model == null) return;
            model.Kind = MaterialType.Probe;
            model.Name = dto.probeName ?? "";
            model.ShortName = model.Name;
            model.MaterialStatus = dto.state ?? "";
            model.MaterialRestriction = dto.restriction ?? false;
            //庫存資訊
            model.Location = dto?.currentLocation ?? "";
            model.Worksheet = "";
            model.Program = "";
        }

    }
}
