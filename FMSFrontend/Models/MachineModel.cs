
namespace FMSFrontend.Models
{
    public partial class MachineModel
    {
        public string MachineName = ""; //機台名稱
        public string Status = ""; //機台狀態
        public string Type = ""; //機台類型
        public bool Restriction; //機台鎖定
        public int MachineNumber; //         機台編號??
        public string OnDeckElectrodeSerial { get; set; } = "";// 夾持中電極標籤序號（RFID）
        public string OnDeckWorkpieceSerial { get; set; } = "";// 夾持中工件標籤序號（RFID）
        public string OnDeckWorksheetSerial { get; set; } = "";// 當前工單號/序號  
        //電極資訊
        public ElectrodeModel Electrode { get; set; } = new(); //電極名稱                                                     //工件資訊
        public WorkpieceModel Workpiece { get; set; } = new(); //工件名稱
        //機台類型 詳細資訊
        public OscarEdmModel OscarEdm { get; set; } = new();
    }
    public class OscarEdmModel
    {
        public string MachineNumber = "1";
        public string MachineStatus = "Disconnection";
        public string UsingElectrode = "E-03";
        public string MachiningCode = "EDM101";
        public string MachiningWorkingTime = "02:35:20";
        public string MachiningWorkingPercentage = "45%";
        public string CurrentWorksheet = "WS20250717";
        public string MachiningTool = "T-01";

        public string MachineTemperature = "38°C";
        public string SpindleRPM = "1200 RPM";
        public string OilLevelStatus = "正常";
        public string CoolantLevel = "75%";

        public string PositionID = "1";
        public string ABS_X = "1886.6";
        public string ABS_Y = "186.6";
        public string ABS_Z = "176.6";
        public string ABS_A = "183.0";

        public string ABS_B = "18.69";
        public string ABS_C = "186";
        public string MCH_X = "6886";
        public string MCH_Y = "1874.6";
        public string MCH_Z = "1456.6";

        public string Speed = "3000";
        public string Servo = "5";
        public string Gap = "0.25";
        public string OB = "0.03";
        public string E_SPD = "100";
        public string Pol = "POS";
        public string Pulse = "50";

        public string E_Code = "A2";
        public string T_ON = "25ms";
        public string T_OFF = "5ms";
        public string LV = "120V";
        public string HV = "210V";
        public string JT = "12";
        public string JD = "3";
    }
}



















