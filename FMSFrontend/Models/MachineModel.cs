using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace FMSFrontend.Models
{
    public partial class MachineGroupModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<MachineModel> machines = new();
        [ObservableProperty] private string selectName = "";
        [ObservableProperty] private OscarEdmModel selectMachine = new();
    }
    public partial class MachineModel : ObservableObject
    {
        // 機台基本資訊
        [ObservableProperty] private string machineName = ""; //機台名稱
        [ObservableProperty] private string status = ""; //機台狀態
        [ObservableProperty] private string type = ""; //機台類型
        [ObservableProperty] private bool restriction; //機台鎖定
        [ObservableProperty] private int machineNumber; //機台編號
        // 夾持中資料 (RFID / 工單)
        [ObservableProperty] private string onDeckElectrodeSerial = ""; //夾持中電極標籤序號（RFID）
        [ObservableProperty] private string onDeckWorkpieceSerial = ""; //夾持中工件標籤序號（RFID）
        [ObservableProperty] private string onDeckWorksheetSerial = ""; //當前工單號/序號

        // 相關模型
        [ObservableProperty] private string workpieceName = "";         //工件名稱
        [ObservableProperty] private string workpieceShortName = "";    //工件短名稱
        [ObservableProperty] private string electrodeName = "";         //電極名稱
        [ObservableProperty] private string electrodeShortName = "";    //電極短名稱
        
        [ObservableProperty] private OscarEdmModel oscarEdm = new(); //機台類型 詳細資訊
    }

    public partial class OscarEdmModel : ObservableObject
    {
        [ObservableProperty] private string mainProgramName = "1"; //MachineModel  machineNumber
        [ObservableProperty] private bool canControl = false; //MachineModel  machineNumber

        [ObservableProperty] private string machineNumber = "1"; //MachineModel  machineNumber
        [ObservableProperty] private string machineStatus = ""; //MachineModel Status
        [ObservableProperty] private string usingElectrode = ""; //MachineModel electrodeShortName
        [ObservableProperty] private string machiningCode = ""; //MachineModel  type
        [ObservableProperty] private string cycleTime = ""; // Cycle Time 
        [ObservableProperty] private string machiningWorkingPercentage = ""; //45%
        [ObservableProperty] private string currentWorksheet = "";//WS20250717
        [ObservableProperty] private string machiningTool = ""; //T-01

        [ObservableProperty] private string machineTemperature = ""; //38°C
        [ObservableProperty] private string spindleRPM = "";  //1200 RPM
        [ObservableProperty] private string oilLevelStatus = ""; //正常
        [ObservableProperty] private string coolantLevel = "";//75%

        [ObservableProperty] private string positionID = "1";
        [ObservableProperty] private string aBS_X = "1886.6";
        [ObservableProperty] private string aBS_Y = "186.6";
        [ObservableProperty] private string aBS_Z = "176.6";
        [ObservableProperty] private string aBS_A = "183.0";

        [ObservableProperty] private string aBS_B = "18.69";
        [ObservableProperty] private string aBS_C = "186";
        [ObservableProperty] private string mCH_X = "6886";
        [ObservableProperty] private string mCH_Y = "1874.6";
        [ObservableProperty] private string mCH_Z = "1456.6";

        [ObservableProperty] private string speed = "3000";
        [ObservableProperty] private string servo = "5";
        [ObservableProperty] private string gap = "0.25";
        [ObservableProperty] private string oB = "0.03";
        [ObservableProperty] private string e_SPD = "100";
        [ObservableProperty] private string pol = "POS";
        [ObservableProperty] private string pulse = "50";

        [ObservableProperty] private string e_Code = "A2";
        [ObservableProperty] private string t_ON = "25ms";
        [ObservableProperty] private string t_OFF = "5ms";
        [ObservableProperty] private string lV = "120V";
        [ObservableProperty] private string hV = "210V";
        [ObservableProperty] private string jT = "12";
        [ObservableProperty] private string jD = "3";
    }
}



















