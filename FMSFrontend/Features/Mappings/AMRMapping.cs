using FMSFrontend.Features.Dtos;
using FMSFrontend.Models;
using System;

namespace FMSFrontend.Features.Mappings
{
    public static class AMRMapping
    {
        public static void ApplyAmrDto(this AMRDto dto, AMRModel amr)
        {
            amr.Success = dto.Success;
            amr.Msg = dto.Msg ?? "";
            amr.AgvId = dto.AgvId ?? "";
            amr.Status = dto.Status ?? "";
            amr.AlarmsMsg = dto.AlarmsMsg ?? "";
            amr.Battery = dto.Battery ?? "";
            amr.Location = dto.Location ?? "";
            amr.CurrentRoute = dto.CurrentRoute ?? "";
            amr.WorkingStatus = dto.WorkingStatus ?? "";

        }
    }
}
