using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Dtos
{
    using System.Text.Json.Serialization;

    public class OscarmaxMachineParaDto : MachineParaBase
    {
        [JsonPropertyName("dbmachinenumber")]
        public string DBMachineNumber { get; set; } = "";

        [JsonPropertyName("dispatcherunknownerror")]
        public bool DispatcherUnknownError { get; set; }

        [JsonPropertyName("dispatcherinputcompvalueerror")]
        public bool DispatcherInputCompValueError { get; set; }

        [JsonPropertyName("dispatcherupdatecompvalueerror")]
        public bool DispatcherUpdateCompValueError { get; set; }

        [JsonPropertyName("dispatcherncoffseterror")]
        public bool DispatcherNCOffsetError { get; set; }

        [JsonPropertyName("dispatcherncprogramchangeerror")]
        public bool DispatcherNCProgramChangeError { get; set; }

        [JsonPropertyName("dispatcherncstarterror")]
        public bool DispatcherNCStartError { get; set; }

        [JsonPropertyName("dispatcherbookedtooldisappearerror")]
        public bool DispatcherBookedToolDisappearError { get; set; }

        [JsonPropertyName("dispatcherbookedpartdisappearerror")]
        public bool DispatcherBookedPartDisappearError { get; set; }

        [JsonPropertyName("dispatchererrorelectrodename")]
        public bool DispatcherErrorElectrodeName { get; set; }

        [JsonPropertyName("dispatchernonextelectrodeerror")]
        public bool DispatcherNoNextElectrodeError { get; set; }

        [JsonPropertyName("waitingforworkpiece")]
        public bool WaitingforWorkpiece { get; set; }

        [JsonPropertyName("waitingforelectrode")]
        public bool WaitingforElectrode { get; set; }

        [JsonPropertyName("waitingforprobe")]
        public bool WaitingforProbe { get; set; }

        [JsonPropertyName("connectiononcetrigger")]
        public bool ConnectionOnceTrigger { get; set; } = true;

        [JsonPropertyName("user")]
        public string User { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("type")]
        public string Type { get; set; } = "";

        /// <summary>0:紅 1:黃 2:綠 3:閒置（依你註解）</summary>
        [JsonPropertyName("light_state")]
        public int Light_state { get; set; }

        [JsonPropertyName("progressbar")]
        public ushort ProgressBar { get; set; }

        [JsonPropertyName("run_no")]
        public ushort Run_No { get; set; }

        [JsonPropertyName("run_sno")]
        public ushort Run_sNo { get; set; }

        [JsonPropertyName("run_tool")]
        public ushort Run_Tool { get; set; }

        [JsonPropertyName("run_work")]
        public ushort Run_Work { get; set; }

        [JsonPropertyName("ai_mode")]
        public string AI_Mode { get; set; } = "";

        [JsonPropertyName("ai_edm_state")]
        public string AI_EDM_State { get; set; } = "";

        [JsonPropertyName("ai_mach_state")]
        public string AI_Mach_State { get; set; } = "";

        /// <summary>0: Alarm 1: Warning 2: Maintenance</summary>
        [JsonPropertyName("mach_message")]
        public int Mach_Message { get; set; }

        [JsonPropertyName("mach_alarm")]
        public bool Mach_Alarm { get; set; }

        [JsonPropertyName("mach_warning")]
        public bool Mach_Warning { get; set; }

        [JsonPropertyName("mach_maintenance")]
        public bool Mach_Maintenance { get; set; }

        /// <summary>true: 已完成原點</summary>
        [JsonPropertyName("origin_axis")]
        public bool[] Origin_Axis { get; set; } = new bool[8];

        // ****** DR 點 ****** //
        [JsonPropertyName("worknumber_now")]
        public ushort WorkNumber_now { get; set; }

        [JsonPropertyName("worknumber_need")]
        public ushort WorkNumber_need { get; set; }

        [JsonPropertyName("toolnumber_now")]
        public ushort ToolNumber_now { get; set; }

        [JsonPropertyName("toolnumber_need")]
        public ushort ToolNumber_need { get; set; }

        [JsonPropertyName("dr_14")]
        public ushort DR_14 { get; set; }

        // ****** I/O ****** //
        [JsonPropertyName("io_c")]
        public bool[] IO_C { get; set; } = new bool[65];

        // ****** Door State ****** //
        [JsonPropertyName("dooratlowest")]
        public bool DoorAtLowest { get; set; }

        [JsonPropertyName("determine_error")]
        public bool Determine_Error { get; set; }

        [JsonPropertyName("test_value")]
        public byte Test_Value { get; set; }

        // ****** Coordinate ****** //
        [JsonPropertyName("coordinate")]
        public string Coordinate { get; set; } = "";

        // 絕對座標
        [JsonPropertyName("abs_x")]
        public string ABS_X { get; set; } = "";
        [JsonPropertyName("abs_y")]
        public string ABS_Y { get; set; } = "";
        [JsonPropertyName("abs_z")]
        public string ABS_Z { get; set; } = "";
        [JsonPropertyName("abs_c")]
        public string ABS_C { get; set; } = "";
        [JsonPropertyName("abs_a")]
        public string ABS_A { get; set; } = "";
        [JsonPropertyName("abs_b")]
        public string ABS_B { get; set; } = "";

        // 機械座標
        [JsonPropertyName("mac_x")]
        public string Mac_X { get; set; } = "";
        [JsonPropertyName("mac_y")]
        public string Mac_Y { get; set; } = "";
        [JsonPropertyName("mac_z")]
        public string Mac_Z { get; set; } = "";
        [JsonPropertyName("mac_c")]
        public string Mac_C { get; set; } = "";
        [JsonPropertyName("mac_a")]
        public string Mac_A { get; set; } = "";
        [JsonPropertyName("mac_b")]
        public string Mac_B { get; set; } = "";

        // ***** E_Code ***** //
        [JsonPropertyName("e_code")]
        public string E_Code { get; set; } = "";
        [JsonPropertyName("t_on")]
        public string T_ON { get; set; } = "";
        [JsonPropertyName("t_off")]
        public string T_OFF { get; set; } = "";
        [JsonPropertyName("lv")]
        public string LV { get; set; } = "";
        [JsonPropertyName("hv")]
        public string HV { get; set; } = "";
        [JsonPropertyName("jt")]
        public string JT { get; set; } = "";
        [JsonPropertyName("jd")]
        public string JD { get; set; } = "";
        [JsonPropertyName("speed")]
        public string Speed { get; set; } = "";
        [JsonPropertyName("servo")]
        public string Servo { get; set; } = "";
        [JsonPropertyName("gap")]
        public string Gap { get; set; } = "";
        [JsonPropertyName("ob")]
        public string OB { get; set; } = "";
        [JsonPropertyName("e_spd")]
        public string E_SPD { get; set; } = "";
        [JsonPropertyName("pol")]
        public string Pol { get; set; } = "";
        [JsonPropertyName("pulse")]
        public string Pulse { get; set; } = "";
        [JsonPropertyName("nw")]
        public string NW { get; set; } = "";
        [JsonPropertyName("hv2")]
        public string HV2 { get; set; } = "";
        [JsonPropertyName("slope")]
        public string Slope { get; set; } = "";
        [JsonPropertyName("capic")]
        public string Capic { get; set; } = "";
        [JsonPropertyName("arc")]
        public string Arc { get; set; } = "";
        [JsonPropertyName("b_spd")]
        public string B_SPD { get; set; } = "";
        [JsonPropertyName("b_dis")]
        public string B_DIS { get; set; } = "";
        [JsonPropertyName("drill_slag")]
        public string Drill_Slag { get; set; } = "";
        [JsonPropertyName("drill_mag")]
        public string Drill_Mag { get; set; } = "";
        [JsonPropertyName("gain")]
        public string Gain { get; set; } = "";
        [JsonPropertyName("turbo")]
        public string Turbo { get; set; } = "";
        [JsonPropertyName("res_2")]
        public string Res_2 { get; set; } = "";
        [JsonPropertyName("res_3")]
        public string Res_3 { get; set; } = "";
        [JsonPropertyName("res_4")]
        public string Res_4 { get; set; } = "";

        [JsonPropertyName("remote_af")]
        public byte Remote_AF { get; set; }

        [JsonPropertyName("remote_ef")]
        public byte Remote_EF { get; set; }

        [JsonPropertyName("notify_almostfinishedprogram")]
        public bool Notify_AlmostFinishedProgram { get; set; }

        [JsonPropertyName("uninstalltoolrequirements")]
        public bool UninstallToolRequirements { get; set; }

        [JsonPropertyName("installtoolrequirements")]
        public bool InstallToolRequirements { get; set; }

        [JsonPropertyName("uninstallpiecerequirements")]
        public bool UninstallPieceRequirements { get; set; }

        [JsonPropertyName("installpiecerequirements")]
        public bool InstallPieceRequirements { get; set; }

        [JsonPropertyName("uninstalledtool")]
        public bool UninstalledTool { get; set; }

        [JsonPropertyName("installedtool")]
        public bool InstalledTool { get; set; }

        [JsonPropertyName("uninstalledpiece")]
        public bool UninstalledPiece { get; set; }

        [JsonPropertyName("installedpiece")]
        public bool InstalledPiece { get; set; }

        /// <summary>主軸夾爪打開</summary>
        [JsonPropertyName("spindlegripperopen")]
        public bool SpindleGripperOpen { get; set; }

        /// <summary>工件夾爪打開</summary>
        [JsonPropertyName("piecegripperopen")]
        public bool PieceGripperOpen { get; set; }

        /// <summary>停止測試連線</summary>
        [JsonPropertyName("stoptestconnect")]
        public bool StopTestConnect { get; set; }
    }

    public class MachineParaBase
    {
        [JsonPropertyName("connectstate")]
        public bool ConnectState { get; set; }

        [JsonPropertyName("workstate")]
        public int WorkState { get; set; }

        [JsonPropertyName("errormes")]
        public string ErrorMes { get; set; } = "";

        [JsonPropertyName("mainprogramname")]
        public string MainProgramName { get; set; } = "";

        [JsonPropertyName("subprogramname")]
        public string SubProgramName { get; set; } = "";

        [JsonPropertyName("cycletime")]
        public string CycleTime { get; set; } = "";

        /// <summary>決定機台可不可控制或派工</summary>
        [JsonPropertyName("cancontrol")]
        public bool CanControl { get; set; }
    }

}
