using FMSFrontend.Features.Dtos;
using FMSFrontend.Models;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace FMSFrontend.Features.Mappings
{
    public static class MachineMapping
    {
        private static int Cnt = 0;
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

            //測試
            /*
            Cnt++;
            model.OscarEdm.MachineNumber = model.MachineNumber.ToString() + (Cnt + 1).ToString();
            model.OscarEdm.MachineStatus = model.MachineNumber.ToString() + (Cnt + 2).ToString();
            model.OscarEdm.UsingElectrode = model.MachineNumber.ToString() + (Cnt + 3).ToString();
            model.OscarEdm.MachiningCode = model.MachineNumber.ToString() + (Cnt + 4).ToString();
            model.OscarEdm.MachiningWorkingTime = model.MachineNumber.ToString() + (Cnt + 5).ToString();
            model.OscarEdm.MachiningWorkingPercentage = model.MachineNumber.ToString() + (Cnt + 6).ToString();
            model.OscarEdm.CurrentWorksheet = model.MachineNumber.ToString() + (Cnt + 7).ToString();
            model.OscarEdm.MachiningTool = model.MachineNumber.ToString() + $"T-{(Cnt + 8):00}";

            model.OscarEdm.MachineTemperature = model.MachineNumber.ToString() + (Cnt + 9).ToString();
            model.OscarEdm.SpindleRPM = model.MachineNumber.ToString() + (Cnt + 10).ToString();
            model.OscarEdm.OilLevelStatus = model.MachineNumber.ToString() + (Cnt + 11).ToString();
            model.OscarEdm.CoolantLevel = model.MachineNumber.ToString() + (Cnt + 12).ToString();

            model.OscarEdm.PositionID = model.MachineNumber.ToString() + (Cnt + 13).ToString();
            model.OscarEdm.ABS_X = model.MachineNumber.ToString() + (Cnt + 14).ToString();
            model.OscarEdm.ABS_Y = model.MachineNumber.ToString() + (Cnt + 15).ToString();
            model.OscarEdm.ABS_Z = model.MachineNumber.ToString() + (Cnt + 16).ToString();
            model.OscarEdm.ABS_A = model.MachineNumber.ToString() + (Cnt + 17).ToString();
            model.OscarEdm.ABS_B = model.MachineNumber.ToString() + (Cnt + 18).ToString();
            model.OscarEdm.ABS_C = model.MachineNumber.ToString() + (Cnt + 19).ToString();

            model.OscarEdm.MCH_X = model.MachineNumber.ToString() + (Cnt + 20).ToString();
            model.OscarEdm.MCH_Y = model.MachineNumber.ToString() + (Cnt + 21).ToString();
            model.OscarEdm.MCH_Z = model.MachineNumber.ToString() + (Cnt + 22).ToString();

            model.OscarEdm.Speed = model.MachineNumber.ToString() + (Cnt + 23).ToString();
            model.OscarEdm.Servo = model.MachineNumber.ToString() + (Cnt + 24).ToString();
            model.OscarEdm.Gap = model.MachineNumber.ToString() + (Cnt + 25).ToString();
            model.OscarEdm.OB = model.MachineNumber.ToString() + (Cnt + 26).ToString();
            model.OscarEdm.E_SPD = model.MachineNumber.ToString() + (Cnt + 27).ToString();
            model.OscarEdm.Pol = model.MachineNumber.ToString() + (Cnt + 28).ToString();
            model.OscarEdm.Pulse = model.MachineNumber.ToString() + (Cnt + 29).ToString();

            model.OscarEdm.E_Code = model.MachineNumber.ToString() + (Cnt + 30).ToString();
            model.OscarEdm.T_ON = model.MachineNumber.ToString() + (Cnt + 31).ToString();
            model.OscarEdm.T_OFF = model.MachineNumber.ToString() + (Cnt + 32).ToString();
            model.OscarEdm.LV = model.MachineNumber.ToString() + (Cnt + 33).ToString();
            model.OscarEdm.HV = model.MachineNumber.ToString() + (Cnt + 34).ToString();
            model.OscarEdm.JT = model.MachineNumber.ToString() + (Cnt + 35).ToString();
            model.OscarEdm.JD = model.MachineNumber.ToString() + (Cnt + 36).ToString();
            */
        }

        public static void ApplyElectrodeDto(this ElectrodeDto dto, MachineModel model)
        {
            if (dto == null || model == null) return;
            model.ElectrodeShortName =
                Regex.Match(dto.electrodeName, @"_(\d+-[A-Za-z0-9]+)").Groups[1].Value;
        }

        public static void ApplyWorkpieceDto(this WorkpieceDto dto, MachineModel model)
        {
            if (dto == null || model == null) return;
            model.WorkpieceShortName =
                Regex.Match(dto.workpieceName, @"-(\d+_\d+-[A-Za-z]+)$").Groups[1].Value;
        }
        
        public static void ApplyOscarmaxMachineParaDto(this OscarmaxMachineParaDto dto, MachineModel model)
        {
            if (dto == null || model == null || model.OscarEdm == null) return;
            model.OscarEdm.MachineNumber = model.MachineName;
            model.OscarEdm.MachineStatus = model.Status;
            model.OscarEdm.UsingElectrode = model.ElectrodeShortName;
            model.OscarEdm.MachiningCode = model.Type;
            model.OscarEdm.MachiningWorkingTime = dto.CycleTime;
            model.OscarEdm.MachiningWorkingPercentage = $"{dto.ProgressBar}%";
            model.OscarEdm.CurrentWorksheet = dto.WorkNumber_now.ToString();
            model.OscarEdm.MachiningTool = $"T-{dto.Run_Tool:00}";

            model.OscarEdm.MachineTemperature = "";
            model.OscarEdm.SpindleRPM = "";
            model.OscarEdm.OilLevelStatus = "";
            model.OscarEdm.CoolantLevel = "";

            model.OscarEdm.PositionID = "";
            model.OscarEdm.ABS_X = dto.ABS_X;
            model.OscarEdm.ABS_Y = dto.ABS_Y;
            model.OscarEdm.ABS_Z = dto.ABS_Z;
            model.OscarEdm.ABS_A = dto.ABS_A;
            model.OscarEdm.ABS_B = dto.ABS_B;
            model.OscarEdm.ABS_C = dto.ABS_C;

            model.OscarEdm.MCH_X = dto.Mac_X;
            model.OscarEdm.MCH_Y = dto.Mac_Y;
            model.OscarEdm.MCH_Z = dto.Mac_Z;

            model.OscarEdm.Speed = dto.Speed;
            model.OscarEdm.Servo = dto.Servo;
            model.OscarEdm.Gap = dto.Gap;
            model.OscarEdm.OB = dto.OB;
            model.OscarEdm.E_SPD = dto.E_SPD;
            model.OscarEdm.Pol = dto.Pol;
            model.OscarEdm.Pulse = dto.Pulse;

            model.OscarEdm.E_Code = dto.E_Code;
            model.OscarEdm.T_ON = dto.T_ON;
            model.OscarEdm.T_OFF = dto.T_OFF;
            model.OscarEdm.LV = dto.LV;
            model.OscarEdm.HV = dto.HV;
            model.OscarEdm.JT = dto.JT;
            model.OscarEdm.JD = dto.JD;
        }
    }
}
