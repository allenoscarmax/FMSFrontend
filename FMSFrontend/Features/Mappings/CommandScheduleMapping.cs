using FMSFrontend.Features.Dtos;
using FMSFrontend.Models;
namespace FMSFrontend.Features.Mappings
{
    public static class CommandScheduleMapping
    {
        public static void ApplyCommandScheduleDto(this CommandStructDto dto, CommandScheduleModel model)
        {
            model.Priority = dto.Priority;
            model.TaskSource = dto.TaskSource;
            model.ProgressPercent = 100;
            model.CommandType = dto.CommandType;
            model.InsertTimeString = dto.InsertTimeString ?? "";
            model.StartPoint = dto.StartPoint ?? "";
            model.EndPoint = dto.EndPoint ?? "";
        }
    }
}
