using ControlzEx.Standard;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Models;
using OSCARMAXFMS_V3.DBmodels;
using System.Xml.Linq;
namespace FMSFrontend.Features.Mappings
{
    public static class WorkpieceModelMapping
    {
        public static void ApplyWorkpieceDto(this WorkpieceDto dto, WorkpieceModel model)
        {
            if (dto == null || model == null) return;
            model.Id = dto._id;
            model.JigSerial = "";                               //未定義
            model.Name = dto.workpieceName;//工件名稱
            model.No = "";                                      //未定義
            model.WorkType = "";                                //未定義
            model.PartNo = "";                                  //未定義
            model.OrderNo = dto.worksheetNumber;                
            model.ClampNo = "";                                 //未定義
            model.Status = dto.status;
            model.BatchNo = "";                                 //未定義
            model.PartName = "";                                //未定義
            model.SerialCode = "";                              //未定義
            model.RouteNo = "";                                 //未定義
        }
        public static void ApplyStorageDto(this StorageDto dto, WorkpieceModel model)
        {
            if (dto == null || model == null) return;
            model.StoragStatus = dto.state; // 倉儲狀態
            model.StorageRestriction = dto.restriction?? false; //倉儲是否鎖定
        }
    }
}
