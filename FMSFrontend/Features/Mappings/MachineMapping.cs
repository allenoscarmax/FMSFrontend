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
        public static void ApplyProbeDto(this ProbeDto dto, MachineModel model)
        {
            if (dto == null || model == null) return;
            model.ElectrodeName = dto.probeName;
            model.ElectrodeShortName = "probe"; //20260120佑義要求修改電極名稱規則
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
            model.OscarEdm.MachiningCode = dto.MainProgramName ?? "";                            //機台型號
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

        public static void ApplyFanucCNCParaDto(this FanucCNCDto dto, MachineModel model)
        {
            if (dto == null || model == null || model.SunmillFanucCNC == null) return;
            //CNC 色燈  1:綠 2:黃 3:紅 0:沒亮
            model.SunmillFanucCNC.MachineStatus = dto.CNC_light switch
            {
                1 => "Running",
                2 => "Idle",
                3 => "Alarm",
                _ => ""
            };
            model.SunmillFanucCNC.MachineMode = dto.CNC_Operation_Mode ?? "";     //主程式名稱
            model.SunmillFanucCNC.CanControl = dto.CanControl;

            model.SunmillFanucCNC.ABS_X = dto.AxisX.ToString("0.###");
            model.SunmillFanucCNC.ABS_Y = dto.AxisY.ToString("0.###");
            model.SunmillFanucCNC.ABS_Z = dto.AxisZ.ToString("0.###");

            model.SunmillFanucCNC.MCH_X = dto.AxisX.ToString("0.###");
            model.SunmillFanucCNC.MCH_Y = dto.AxisY.ToString("0.###");
            model.SunmillFanucCNC.MCH_Z = dto.AxisZ.ToString("0.###");
        }

        public static void ApplySiemensCNCParaDto(this SiemensCNCDto dto, MachineModel model)
        {
            if (dto == null || model == null || model.SunmillSiemensCNC == null) return;

            model.SunmillSiemensCNC.MainProgramName = dto.ProgramName ?? "";
            model.SunmillSiemensCNC.CanControl = dto.CanControl;

            model.SunmillSiemensCNC.MachineNumber = model.MachineName ?? "";
            model.SunmillSiemensCNC.MachineStatus = dto.MachineStatusRaw ?? "";
            model.SunmillSiemensCNC.ToolName = dto.ToolIdentifier ?? "";
            model.SunmillSiemensCNC.MachiningCode = dto.ProgramName ?? "";
            model.SunmillSiemensCNC.CycleTime = dto.CycleTime?.ToString() ?? dto.CycleTimeRaw ?? "";
            model.SunmillSiemensCNC.MachiningWorkingPercentage = dto.IsProcessing ? "Processing" : "";
            model.SunmillSiemensCNC.CurrentWorksheet = model.OnDeckWorksheetSerial ?? "";
            model.SunmillSiemensCNC.MachiningTool = dto.ActiveToolNumber > 0 ? $"T-{dto.ActiveToolNumber:00}" : "";

            model.SunmillSiemensCNC.PositionID = dto.ActiveFrameIndex.ToString();

            model.SunmillSiemensCNC.ABS_X = dto.WorkPosX.ToString("0.###");
            model.SunmillSiemensCNC.ABS_Y = dto.WorkPosY.ToString("0.###");
            model.SunmillSiemensCNC.ABS_Z = dto.WorkPosZ.ToString("0.###");
            model.SunmillSiemensCNC.ABS_B = dto.WorkPosB.ToString("0.###");
            model.SunmillSiemensCNC.ABS_C = dto.WorkPosC.ToString("0.###");

            model.SunmillSiemensCNC.MCH_X = dto.MachinePosX.ToString("0.###");
            model.SunmillSiemensCNC.MCH_Y = dto.MachinePosY.ToString("0.###");
            model.SunmillSiemensCNC.MCH_Z = dto.MachinePosZ.ToString("0.###");
            model.SunmillSiemensCNC.MCH_B = dto.MachinePosB.ToString("0.###");
            model.SunmillSiemensCNC.MCH_C = dto.MachinePosC.ToString("0.###");

            model.SunmillSiemensCNC.FeedRate = dto.FeedSpeed.ToString();
            model.SunmillSiemensCNC.SpindleSpeed = dto.SpindleSpeed.ToString();
        }
        private static int Cnt = 0;
        public static void ApplyTest(MachineModel model)
        {
            Cnt++;

        }
    }
}
