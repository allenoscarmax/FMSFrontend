using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Dtos.Database;
using FMSFrontend.Models;
namespace FMSFrontend.Features.Mappings
{
    public static class StationMapping
    {
        public static void ApplyStationDto(this AssemblyStationParaDto dto, StationModel model)
        {
            model.Air_error = dto.Air_error;
            model.DoorisOpen = dto.DoorisOpen;
            model.RFIDisPolarization = dto.RFIDisPolarization;
            model.WorkpieceOnAssemblyStation = dto.WorkpieceOnAssemblyStation;
            model.Notification_IncomingPart = dto.Notification_IncomingPart;
         
            model.Notification_WaitingWorkpieceReturn = dto.Notification_WaitingWorkpieceReturn;
            model.Alarm = dto.Alarm;
            model.Require_IncomingPart = dto.Require_IncomingPart;
            model.Require_OutcomingPart = dto.Require_OutcomingPart;
            model.Notification_Doorislocked = dto.Notification_Doorislocked;
        }
    }
}
