using FMSFrontend.Features.Dtos;
using FMSFrontend.Models;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
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
        public static void ApplyParasDto(this RFIDParasDto dtos, RFIDBindModel output)
        {
            output.ConnectedBrush = dtos.rFID_Is_Present[2] ? RFIDBindModel.LightOff : RFIDBindModel.LightOn;
            Random rnd = new Random();
            //模擬資料
            //  output.ConnectedBrush = rnd.Next(2) == 1 ? RFIDBindModel.LightOff : RFIDBindModel.LightOn;
            //  output.TagBrush = rnd.Next(2) == 1 ? RFIDBindModel.LightOff : RFIDBindModel.LightOn;
            //  output.TagSerial = rnd.Next(10000).ToString();
        }

        public static void ApplyTagDto(this string dtos, RFIDBindModel output)
        {
            //尚須驗證
            output.TagSerial = dtos;
            output.TagBrush = string.IsNullOrWhiteSpace(dtos) ? RFIDBindModel.LightOff : RFIDBindModel.LightOn;
            Random rnd = new Random();
        }
    }
}
