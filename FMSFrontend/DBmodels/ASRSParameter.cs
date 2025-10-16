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

        public bool IsRobotConnected { get; set; } // 與控制器連線（通訊心跳正常）
        public bool IsRobotBusy { get; set; } // 忙碌中（執行任務）
        public bool IsRobotError { get; set; } // 錯誤/故障狀態
        public bool IsRobotBePaused { get; set; } // 已暫停（Pause）
        public bool IsASRSControlStop { get; set; } // ASRS 全域停止（Stop/E-Stop）
        public bool IsRobotWorkingOnScaning { get; set; } // 正在執行掃描作業（條碼/RFID）
        public string? RobotPosition { get; set; } // 目前位置/工位
        public int RobotStatus { get; set; } // 狀態碼（依後端定義）
        public bool BattAlarm { get; set; } // 電池警報（電量/健康度異常）
        public string? RobotAlarmMessage { get; set; } // 警報/錯誤訊息
        public int? RobotNumber { get; set; } // 機器人編號（多機場景）
        public bool AsrsControlStart { get; set; } // ASRS 控制啟動
        public bool AsrsControlPause { get; set; } // ASRS 控制暫停
        public bool AsrsControlStop { get; set; } // ASRS 控制停止
        public bool AsrsProcessWarning { get; set; } // 流程警告（非致命）
        public string? RobotDoingNow { get; set; } // 目前動作描述         <--
        public string? RobotDoingNext { get; set; } // 下一步動作描述      <--
        public bool RobotMaintenanceNotice { get; set; } // 保養/維護提醒
        public bool RobotInSaftyArea { get; set; } // 位於安全區域（Safety Area）
        public bool DispatchSwitch { get; set; } // 派工開關（允許派工）
        public bool DispatchNeedToStop { get; set; } // 派工請求停止（待處理）
        public bool DispatchIsStoped { get; set; } // 派工已停止（已生效）
        public bool RobotCantReceiveNewTask { get; set; } // 無法接收新任務（錯誤/暫停/條件不符）
    }
}   