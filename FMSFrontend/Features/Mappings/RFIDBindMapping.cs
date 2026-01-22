using ControlzEx.Standard;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Windows.Media.Animation;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FMSFrontend.Features.Mappings
{
    public static class RFIDBindMapping
    {
        public static void ApplyRFIDBindPageDto(this List<RFIDWriteLogDto> dtos, RFIDBindModel output)
        {
            if (dtos == null || output == null) return; //檢查是否為空
            output.BurnHistoryList.Clear(); //清空現有的紀錄
            foreach (var dto in dtos)
            {
                var recordDate = dto.timeStamp.Date; //檢查是否違範圍內的資料
                if ((output.from.HasValue && recordDate >= output.from.Value.Date) &&
                    (output.to.HasValue && recordDate <= output.to.Value.Date))
                {
                    output.BurnHistoryList.Add(new BurnRecord
                    {
                        Time = dto.timeStamp,
                        MaterialType = string.IsNullOrWhiteSpace(dto.type) ? (dto.objName ?? string.Empty) : dto.type,
                        SerialNo = dto.srialNo ?? string.Empty,
                        TagSerial = dto.tagSerial ?? string.Empty
                    });
                }
            }
        }
        public static void ApplyParasDto(this RFIDParasDto dtos, RFIDBindModel output, int TagNumber)
        {
            output.ConnectedBrush = dtos.rFID_Is_Present[TagNumber] ? RFIDBindModel.LightOn : RFIDBindModel.LightOff;
        }

        public static void ApplyTagDto(this string? dtos, RFIDBindModel output)
        {
            output.TagSerial = dtos ?? "";
            output.TagBrush = string.IsNullOrWhiteSpace(dtos) ? RFIDBindModel.LightOff : RFIDBindModel.LightOn;
        }
        public static void ApplyEleDto(this ElectrodeDto dtos, RFIDBindModel output)
        {
            output.ReadElectrodeFlag = true;
            output.electrode.Id = dtos._id;
            output.electrode.Name = dtos.electrodeName ?? "";
            output.electrode.No = "";
            output.electrode.Type = dtos.electrodeType ?? "";
            output.electrode.Status = dtos.state ?? "";
            output.electrode.TagSerial = dtos.tagSerial ?? "";
            output.electrode.MaxDischargeCount = dtos.lifeTimes.ToString() ?? "";
            output.electrode.Compensation = dtos.offset ?? "";
            output.electrode.ProcessedCount = dtos.useTimes?.ToString() ?? "";
            output.electrode.ElecRestriction = dtos.restriction ?? false;
        }
        public static void ApplyWpDto(this WorkpieceDto dtos, RFIDBindModel output)
        {
            output.ReadWorkpieceFlag = true;
            output.workpiece.Id = dtos._id;
            output.workpiece.JigSerial = "";
            output.workpiece.Name = dtos.workpieceName ?? "";
            output.workpiece.No = "";
            output.workpiece.WorkType = "";
            output.workpiece.PartNo = "";
            output.workpiece.OrderNo = dtos.worksheetNumber ?? "";
            output.workpiece.ClampNo = "";
            output.workpiece.Status = dtos.status ?? "";
            output.workpiece.BatchNo = "";
            output.workpiece.PartName = "";
            output.workpiece.SerialCode = "";
            output.workpiece.RouteNo = "";
            output.workpiece.WorkRestriction = dtos?.restriction ?? false;
        }
    }
}
