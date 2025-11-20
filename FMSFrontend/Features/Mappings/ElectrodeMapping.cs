using FMSFrontend.Features.Dtos;
using FMSFrontend.Models;
namespace FMSFrontend.Features.Mappings
{
    public static class ElectrodeModelMapping
    {
        public static void ApplyElectrodeDto(this ElectrodeDto dto, ElectrodeModel model)
        {
            model.Id = dto._id;
            model.Name = dto.electrodeName ?? ""; //電極名稱
            model.Type = dto.electrodeType ?? "";
            model.Status = dto.state ?? ""; //電極狀態
            model.TagSerial = dto.tagSerial ?? ""; //序號
            model.MaxDischargeCount = dto.lifeTimes.ToString() ?? "";
            model.Compensation = dto.offset ?? "";
            model.ProcessedCount = dto.useTimes?.ToString() ?? "";
            model.ElecRestriction = dto.restriction ?? false; //鎖定
            //model.UsageRate = "";                               //未定義 use/ life *100%
            //model.No = "";                                      //未定義 名稱
            //model.HolderNo = "";                                //未定義 刪除
            //model.JigSerial = "";                               //未定義 刪除
        }
        public static void ApplyProbeDto(this ProbeDto dto, ElectrodeModel model)
        {
            model.Id = dto._id;
            model.Name = dto.probeName ?? "";   // 電極名稱
            model.No = "";                          //未定義
            model.Type = ""; //未定義
            model.Status = dto.state ?? "";  //電極狀態
            model.TagSerial = dto.tagSerial ?? "";
            model.MaxDischargeCount = ""; //未定義
            model.Compensation = ""; //未定義
            model.ProcessedCount = ""; //未定義
            model.ElecRestriction = dto.restriction ?? false;
        }
        public static void ApplyStorageDto(this StorageDto dto, ElectrodeModel model)
        {
            model.StoragStatus = dto.state; // 倉儲狀態
            model.StorageRestriction = dto.restriction?? false; //倉儲是否鎖定
        }
    }
}
