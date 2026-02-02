using FMSFrontend.Features.Dtos;
using FMSFrontend.Models;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace FMSFrontend.Features.Mappings
{
    public static class MachineMapping
    {
        public static void ApplyMachinesDto(this MachinesDto dto, MachineModel model)
        {
            if (dto == null || model == null) return;

            model.MachineName = dto.machineName ?? "";
            model.Status = dto.status ?? "";
            model.Type = dto.machineCode ?? "";
            model.Restriction = false;
            model.MachineNumber = dto.machineNumber;
            model.OnDeckElectrodeSerial = dto.onDeckElectrodeSerial ?? "";
            model.OnDeckWorkpieceSerial = dto.onDeckWorkpieceSerial ?? "";
            model.OnDeckWorksheetSerial = dto.onDeckWorksheetSerial ?? "";
        }

        public static void ApplyElectrodeDto(this ElectrodeDto dto, MachineModel model)
        {
            if (dto == null || model == null) return;
            model.ElectrodeName = dto.electrodeName;
            model.ElectrodeShortName = Conversion.ShortNameConversion(true, dto.electrodeName); //20260120佑義要求修改電極名稱規則
        }
        public static void ApplyWorkpieceDto(this WorkpieceDto dto, MachineModel model)
        {
            if (dto == null || model == null) return;

            model.WorkpieceName = dto.workpieceName;
            model.WorkpieceShortName = Conversion.ShortNameConversion(false, dto.workpieceName);   //20260120佑義要求修改電極名稱規則
        }

        public static void ApplyOscarmaxMachineParaDto(this OscarmaxMachineParaDto dto, MachineModel model)
        {
            if (dto == null || model == null || model.OscarEdm == null) return;
            //設備資訊
            model.OscarEdm.MainProgramName = dto.MainProgramName ?? "";     //主程式名稱
            model.OscarEdm.CanControl = dto.CanControl;                     //是否可控

            model.OscarEdm.MachineNumber = model.MachineName ?? "";                     //機台名稱
            model.OscarEdm.MachineStatus = model.Status ?? "";                          //機台狀態
            model.OscarEdm.UsingElectrode = model.ElectrodeShortName ?? "";             //電極名稱
            model.OscarEdm.MachiningCode = model.Type ?? "";                            //機台型號
            model.OscarEdm.CycleTime = dto.CycleTime ?? "";                             //加工持續時間
            model.OscarEdm.MachiningWorkingPercentage = $"{dto.ProgressBar}%" ?? ""; //加工進度
            model.OscarEdm.CurrentWorksheet = dto.WorkNumber_now.ToString() ?? ""; //?? 目前工單
            model.OscarEdm.MachiningTool = $"T-{dto.Run_Tool:00}" ?? ""; // 使用刀具 ?? 感覺是電極

            model.OscarEdm.MachineTemperature = ""; //??
            model.OscarEdm.SpindleRPM = ""; //??
            model.OscarEdm.OilLevelStatus = ""; //??
            model.OscarEdm.CoolantLevel = ""; //??

            model.OscarEdm.PositionID = dto.Coordinate ?? "";  //坐標系
            model.OscarEdm.ABS_X = dto.ABS_X ?? "";
            model.OscarEdm.ABS_Y = dto.ABS_Y ?? "";
            model.OscarEdm.ABS_Z = dto.ABS_Z ?? "";
            model.OscarEdm.ABS_A = dto.ABS_A ?? "";
            model.OscarEdm.ABS_B = dto.ABS_B ?? "";
            model.OscarEdm.ABS_C = dto.ABS_C ?? "";

            model.OscarEdm.MCH_X = dto.Mac_X ?? "";
            model.OscarEdm.MCH_Y = dto.Mac_Y ?? "";
            model.OscarEdm.MCH_Z = dto.Mac_Z ?? "";

            model.OscarEdm.Speed = dto.Speed ?? "";
            model.OscarEdm.Servo = dto.Servo ?? "";
            model.OscarEdm.Gap = dto.Gap ?? "";
            model.OscarEdm.OB = dto.OB ?? "";
            model.OscarEdm.E_SPD = dto.E_SPD ?? "";
            model.OscarEdm.Pol = dto.Pol ?? "";
            model.OscarEdm.Pulse = dto.Pulse ?? "";

            model.OscarEdm.E_Code = dto.E_Code ?? "";
            model.OscarEdm.T_ON = dto.T_ON ?? "";
            model.OscarEdm.T_OFF = dto.T_OFF ?? "";
            model.OscarEdm.LV = dto.LV ?? "";
            model.OscarEdm.HV = dto.HV ?? "";
            model.OscarEdm.JT = dto.JT ?? "";
            model.OscarEdm.JD = dto.JD ?? "";
        }
        private static int Cnt = 0;
        public static void ApplyTest(MachineModel model)
        {
            Cnt++;

        }
    }
}
