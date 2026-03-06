using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Dtos
{
    public class SiemensCNCDto
    {
        // ---- Connection / Meta ----
        [JsonPropertyName("cNC_Connection")]
        public bool CNC_Connection { get; set; }
        [JsonPropertyName("lastHeartbeatUtc")]
        public DateTime LastHeartbeatUtc { get; set; }
        [JsonPropertyName("lastErrorMessage")]
        public string LastErrorMessage { get; set; } = "";

        // ---- Mode / State ----
        [JsonPropertyName("cNC_Operation_Mode")]
        public string CNC_Operation_Mode { get; set; } = "";
        [JsonPropertyName("machineStatusRaw")]
        public string MachineStatusRaw { get; set; } = "";
        [JsonPropertyName("cNC_light")]
        public int CNC_light { get; set; } //CNC 色燈  1:綠 2:黃 3:紅 0:沒亮

        // ---- Alarm ----
        [JsonPropertyName("hasAlarm")]
        public bool HasAlarm { get; set; }
        [JsonPropertyName("alarmText")]
        public string AlarmText { get; set; } = ""; // or AlarmRaw

        // ---- Control ----
        [JsonPropertyName("allowEntry")]
        public bool AllowEntry { get; set; }
        [JsonPropertyName("canControl")]
        public bool CanControl { get; set; }

        // ---- Processing ----
        [JsonPropertyName("programName")]
        public string ProgramName { get; set; } = ""; // 目前執行的程式名稱，對應 Siemens CNC 的當前程式資訊
        [JsonPropertyName("cycleTimeRaw")]
        public string CycleTimeRaw { get; set; } = ""; // 原始的 CycleTime 字串，格式可能是 "00:00:00" 或 "0h 0m 0s" 等，視 Siemens CNC 的回傳格式而定
        [JsonPropertyName("cycleTime")]
        public TimeSpan? CycleTime { get; set; } // 從 CycleTimeRaw 解析出來的 TimeSpan，方便前端顯示為「00:00:00」格式
        [JsonPropertyName("activeToolNumber")]
        public int ActiveToolNumber { get; set; } // 目前使用的刀具號碼，對應 Siemens CNC 的刀具補正號碼（T1, T2, ...）
        [JsonPropertyName("toolIdentifier")]
        public string ToolIdentifier { get; set; } = ""; // 可包含刀具號碼、名稱或其它識別資訊

        [JsonPropertyName("feedSpeed")]
        public int FeedSpeed { get; set; }
        [JsonPropertyName("spindleSpeed")]
        public int SpindleSpeed { get; set; }
        [JsonPropertyName("isFeeding")]
        public bool IsFeeding { get; set; }
        [JsonPropertyName("isSpindleRunning")]
        public bool IsSpindleRunning { get; set; }
        [JsonPropertyName("isProcessing")]
        public bool IsProcessing { get; set; }
        [JsonPropertyName("starttoMill")]
        public bool StarttoMill { get; set; } // 你原本的保留也可

        // ---- Coordinates ----
        [JsonPropertyName("activeFrameIndex")]
        public int ActiveFrameIndex { get; set; } = 0; // 目前使用的坐標系索引（0-9），對應 Siemens CNC 的 G54-G59 等坐標系

        [JsonPropertyName("workPosX")]
        public double WorkPosX { get; set; } // 工作座標 X
        [JsonPropertyName("workPosY")]
        public double WorkPosY { get; set; }// 工作座標 Y
        [JsonPropertyName("workPosZ")]
        public double WorkPosZ { get; set; } // 工作座標 Z
        [JsonPropertyName("workPosB")]
        public double WorkPosB { get; set; } // 工作座標 B
        [JsonPropertyName("workPosC")]
        public double WorkPosC { get; set; } // 工作座標 C


        [JsonPropertyName("machinePosX")]
        public double MachinePosX { get; set; } // 機械座標 X
        [JsonPropertyName("machinePosY")]
        public double MachinePosY { get; set; } // 機械座標 Y
        [JsonPropertyName("machinePosZ")]
        public double MachinePosZ { get; set; } // 機械座標 Z
        [JsonPropertyName("machinePosB")]
        public double MachinePosB { get; set; } // 機械座標 B
        [JsonPropertyName("machinePosC")]
        public double MachinePosC { get; set; } // 機械座標 C

        // ---- Chuck / Air ----
        [JsonPropertyName("chuckStatus")]
        public string ChuckStatus { get; set; } = ""; // 夾頭狀態

        [JsonPropertyName("isChuckClosed")]
        public bool IsChuckClosed { get; set; } // 夾頭狀態，true: 夾緊中, false: 夾緊完成或未啟動
        [JsonPropertyName("airLeakStatus")]
        public string AirLeakStatus { get; set; } = ""; // 氣密狀態
        [JsonPropertyName("isAirLeakOn")]
        public bool IsAirLeakOn { get; set; } // 氣密狀態，true: 氣密中, false: 氣密完成或未啟動

        // ---- File trace ----
        [JsonPropertyName("lastFileNodeIdPath")]
        public string LastFileNodeIdPath { get; set; } = ""; // 最後一次傳輸的檔案在 CNC 內部的節點 ID 路徑，例如 "/Program/子目錄/程式.prg"
        [JsonPropertyName("lastFileTransferOk")]
        public bool LastFileTransferOk { get; set; } // 最後一次檔案傳輸是否成功
        [JsonPropertyName("lastFileContentLength")]
        public int LastFileContentLength { get; set; } // 最後一次傳輸的檔案內容長度（字元數），可用於驗證檔案是否完整
    }
}
