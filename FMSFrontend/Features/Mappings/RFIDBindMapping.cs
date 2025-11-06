using FMSFrontend.Features.Dtos;
using FMSFrontend.Models;
using System.Collections.Generic;

namespace FMSFrontend.Features.Mappings
{
    public static class RFIDBindMapping
    {
        public static void ApplyRFIDBindPageDto(this List<RFIDWriteLogDto> dtos, RFIDBindData output)
        {
            foreach (var dto in dtos)
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
}
