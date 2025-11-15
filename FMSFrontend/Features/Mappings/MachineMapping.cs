using FMSFrontend.Features.Dtos;
using FMSFrontend.Models;
using System.Windows.Controls;

namespace FMSFrontend.Features.Mappings
{
    public static class MachineMapping
    {
        public static void ApplyMachinesDto(this MachinesDto dto, MachineModel model)
        {
            if (dto == null || model == null) return;
            model.MachineName = dto.machineName;
            model.Status = dto.status;
            model.Type = dto.machineCode;
            model.Restriction = false;
            model.MachineNumber = dto.machineNumber;//未定義
        }
        public static void ApplyElectrodeDto(this ElectrodeDto dto, MachineModel model)
        {
            if (dto == null || model == null) return;
            model.Electrode.Name = dto.electrodeName;
        }
        public static void ApplyWorkpieceDto(this WorkpieceDto dto, MachineModel model)
        {
            if (dto == null || model == null) return;
            model.Workpiece.Name = dto.workpieceName;
        }
        public static void ApplyOscarmaxMachineParaDto(this OscarmaxMachineParaDto dto, MachineModel model)
        {
            if (dto == null || model == null || model.OscarEdm == null ) return;

            model.OscarEdm.MachineNumber = model.MachineName;
            model.OscarEdm.MachineStatus = model.Status;
            model.OscarEdm.UsingElectrode = model.Electrode.Name;
            model.OscarEdm.MachiningCode = model.Type;
            model.OscarEdm.MachiningWorkingTime = dto.CycleTime;                //??
            model.OscarEdm.MachiningWorkingPercentage = $"{dto.ProgressBar}%";  //??
            model.OscarEdm.CurrentWorksheet = dto.WorkNumber_now.ToString();    //??
            model.OscarEdm.MachiningTool = $"T-{dto.Run_Tool:00}";

            model.OscarEdm.MachineTemperature = ""; //找不到???
            model.OscarEdm.SpindleRPM = "";         //??
            model.OscarEdm.OilLevelStatus = "";     //??
            model.OscarEdm.CoolantLevel = "";       //??

            // 絕對座標
            model.OscarEdm.PositionID = "";         //??
            model.OscarEdm.ABS_X = dto.ABS_X;
            model.OscarEdm.ABS_Y = dto.ABS_Y;
            model.OscarEdm.ABS_Z = dto.ABS_Z;
            model.OscarEdm.ABS_A = dto.ABS_A;
            model.OscarEdm.ABS_B = dto.ABS_B;
            model.OscarEdm.ABS_C = dto.ABS_C;

            // 機械座標 (DTO: Mac_* -> Model: MCH_*)
            model.OscarEdm.MCH_X = dto.Mac_X;
            model.OscarEdm.MCH_Y = dto.Mac_Y;
            model.OscarEdm.MCH_Z = dto.Mac_Z;

            // EDM 參數
            model.OscarEdm.Speed = dto.Speed;
            model.OscarEdm.Servo = dto.Servo;
            model.OscarEdm.Gap = dto.Gap;
            model.OscarEdm.OB = dto.OB;
            model.OscarEdm.E_SPD = dto.E_SPD;
            model.OscarEdm.Pol = dto.Pol;
            model.OscarEdm.Pulse = dto.Pulse;

            model.OscarEdm.E_Code = dto.E_Code; // 命名差異：E_Cod <- E_Code
            model.OscarEdm.T_ON = dto.T_ON;
            model.OscarEdm.T_OFF = dto.T_OFF;
            model.OscarEdm.LV = dto.LV;
            model.OscarEdm.HV = dto.HV;
            model.OscarEdm.JT = dto.JT;
            model.OscarEdm.JD = dto.JD;
        }
    }
}
