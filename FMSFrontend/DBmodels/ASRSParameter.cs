using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace OSCARMAXFMS_V3.DBmodels
{
    /// <summary>
    /// ASRS 機器人與控制系統狀態參數資料模型
    /// </summary>
    public class ASRSParameter
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } // MongoDB 物件識別碼

        [JsonPropertyName("isRobotConnected")]
        public bool isRobotConnected { get; set; } // 與控制器連線（通訊心跳正常）

        [JsonPropertyName("isRobotBusy")]
        public bool isRobotBusy { get; set; } // 忙碌中（執行任務）

        [JsonPropertyName("isRobotError")]
        public bool isRobotError { get; set; } // 錯誤/故障狀態

        // JSON 裡是 "isRobotbePaused"（小寫 b），對應到 C# 的 isRobotBePaused
        [JsonPropertyName("isRobotbePaused")]
        public bool isRobotBePaused { get; set; } // 已暫停（Pause）

        [JsonPropertyName("isASRSControlStop")]
        public bool isASRSControlStop { get; set; } // ASRS 全域停止（Stop/E-Stop）

        [JsonPropertyName("isRobotWorkingOnScaning")]
        public bool isRobotWorkingOnScaning { get; set; } // 正在執行掃描作業（條碼/RFID）

        [JsonPropertyName("robotPosition")]
        public string? robotPosition { get; set; } // 目前位置/工位

        [JsonPropertyName("robotStatus")]
        public int robotStatus { get; set; } // 狀態碼（依後端定義）

        [JsonPropertyName("battAlarm")]
        public bool battAlarm { get; set; } // 電池警報（電量/健康度異常）

        [JsonPropertyName("robotAlarmMessage")]
        public string? robotAlarmMessage { get; set; } // 警報/錯誤訊息

        [JsonPropertyName("robotNumber")]
        public int? robotNumber { get; set; } // 機器人編號（多機場景）

        // JSON example uses keys like "asrScontrolStart" / "asrScontrolPause" / "asrScontrolStop"
        [JsonPropertyName("asrScontrolStart")]
        public bool asrsControlStart { get; set; } // ASRS 控制啟動

        [JsonPropertyName("asrScontrolPause")]
        public bool asrsControlPause { get; set; } // ASRS 控制暫停

        [JsonPropertyName("asrScontrolStop")]
        public bool asrsControlStop { get; set; } // ASRS 控制停止

        [JsonPropertyName("asrsProcessWarning")]
        public bool asrsProcessWarning { get; set; } // 流程警告（非致命）

        [JsonPropertyName("robotDoingNow")]
        public string? robotDoingNow { get; set; } // 目前動作描述

        [JsonPropertyName("robotDoingNext")]
        public string? robotDoingNext { get; set; } // 下一步動作描述

        [JsonPropertyName("robotMaintenanceNotice")]
        public bool robotMaintenanceNotice { get; set; } // 保養/維護提醒

        [JsonPropertyName("robotInSaftyArea")]
        public bool robotInSaftyArea { get; set; } // 位於安全區域（Safety Area）

        [JsonPropertyName("dispatchSwitch")]
        public bool dispatchSwitch { get; set; } // 派工開關（允許派工）

        // JSON key: "dispatchNeedtoStop" (注意大小寫/拼字)，對應 C# 的 dispatchNeedToStop
        [JsonPropertyName("dispatchNeedtoStop")]
        public bool dispatchNeedToStop { get; set; } // 派工請求停止（待處理）

        // JSON key: "dispatchisStoped" (注意大小寫/拼字)
        [JsonPropertyName("dispatchisStoped")]
        public bool dispatchIsStoped { get; set; } // 派工已停止（已生效）

        [JsonPropertyName("robotCantReceiveNewTask")]
        public bool robotCantReceiveNewTask { get; set; } // 無法接收新任務（錯誤/暫停/條件不符）
    }
}