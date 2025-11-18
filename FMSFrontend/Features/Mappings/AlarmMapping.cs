using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Dtos.Database;
using FMSFrontend.Models;
namespace FMSFrontend.Features.Mappings
{
    public static class AlarmMapping
    {
        public static void ApplyErrorMessageLogDto(this ErrorMessageLogDto dto, AlarmModel model)
        {
            model.TimeStamp = dto.TimeStamp;
            model.ErrorCode = dto.ErrorCode;
            model.MessageCn = dto.MessageCn;
            model.MessageEn = dto.MessageEn;
        }
    }
}
