using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;
using System;

namespace FMSFrontend.SQL.Dtos
{
    /// <summary>
    /// 工單主檔。
    /// </summary>
    public class WorkOrderDto
    {
        /// <summary>
        /// 工單號碼，例如 `WO-260413-001`。
        /// </summary>
        public string OrderID { get; set; } = "";

        /// <summary>
        /// 關聯製程編號，可為空，支援動態自由組合模式。
        /// </summary>
        public string ProcessID { get; set; } = "";

        /// <summary>
        /// 工件 RFID 或條碼。
        /// </summary>
        public string WorkpieceTag { get; set; } = "";

        /// <summary>
        /// 優先權，數字越大越優先。
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 工單狀態，例如 `Pending`、`Running`、`Finished`、`Error`。
        /// </summary>
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// 品質結果。
        /// </summary>
        public string QualityResult { get; set; } = "";

        /// <summary>
        /// 工單底下的分支總數。
        /// </summary>
        public int TotalRouteCount { get; set; }

        /// <summary>
        /// 已全數完成的分支數量。
        /// </summary>
        public int CompletedRouteCount { get; set; }

        /// <summary>
        /// 開單時間。
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 實際開始時間。
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 整張工單完成時間。
        /// </summary>
        public DateTime? EndTime { get; set; }
    }

    /// <summary>
    /// 工單任務明細，支援平行分支。
    /// </summary>
    public class WorkOrderTaskDto
    {
        /// <summary>
        /// 任務流水號。
        /// </summary>
        public int TaskID { get; set; }

        /// <summary>
        /// 關聯工單，可為空，支援手動獨立任務。
        /// </summary>
        public string OrderID { get; set; } = "";

        /// <summary>
        /// 分支代碼，預設為 `Main`。
        /// </summary>
        public string RouteCode { get; set; } = "Main";

        /// <summary>
        /// 分支內步驟順序。
        /// </summary>
        public int StepSequence { get; set; }

        /// <summary>
        /// 動作類型。
        /// </summary>
        public string ActionType { get; set; } = "";

        /// <summary>
        /// 任務備註。
        /// </summary>
        public string Remark { get; set; } = "";

        /// <summary>
        /// 時間統計分類代號。
        /// </summary>
        public string TimeStatCode { get; set; } = "";

        /// <summary>
        /// 任務所需角色代號，例如 `Main_Workpiece`、`Elec_Rough_L`。
        /// </summary>
        public string RequiredRoles { get; set; } = "";

        /// <summary>
        /// 任務參數 JSON。
        /// </summary>
        public string ParametersJson { get; set; } = "{}";

        /// <summary>
        /// 外傳傳送參數。
        /// </summary>
        public string ComParams { get; set; } = "";

        /// <summary>
        /// 指定執行節點。
        /// </summary>
        public string ExecuteNode { get; set; } = "";

        /// <summary>
        /// 實際派工設備編號。
        /// </summary>
        public string TargetDeviceID { get; set; } = "";

        /// <summary>
        /// 任務狀態，預設為 `Waiting_Material`。
        /// </summary>
        public string TaskStatus { get; set; } = "Waiting_Material";

        /// <summary>
        /// 優先權等級，`1` 最急、`5` 一般。
        /// </summary>
        public int PriorityLevel { get; set; } = 5;

        /// <summary>
        /// 是否阻塞，`true` 表示執行完不釋放資源。
        /// </summary>
        public bool IsBlocking { get; set; }

        /// <summary>
        /// 錯誤詳細訊息。
        /// </summary>
        public string ErrorMessage { get; set; } = "";

        /// <summary>
        /// 發生錯誤的時間。
        /// </summary>
        public DateTime? ErrorTime { get; set; }

        /// <summary>
        /// 任務開始時間。
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 任務結束時間。
        /// </summary>
        public DateTime? EndTime { get; set; }
    }

    /// <summary>
    /// 工單資源需求明細。
    /// </summary>
    public class WorkOrderResourceDto
    {
        /// <summary>
        /// 資源需求流水號。
        /// </summary>
        public int UID { get; set; }

        /// <summary>
        /// 關聯工單號碼。
        /// </summary>
        public string OrderID { get; set; } = "";

        /// <summary>
        /// 資源角色代號。
        /// </summary>
        public string ResourceRole { get; set; } = "";

        /// <summary>
        /// 需求型號。
        /// </summary>
        public string RequiredModel { get; set; } = "";

        /// <summary>
        /// 實體綁定的 Tag ID。
        /// </summary>
        public string BoundTagID { get; set; } = "";

        /// <summary>
        /// 資源類型，例如 `Workpiece`、`Electrode`。
        /// </summary>
        public string ResourceType { get; set; } = "";
    }

    /// <summary>
    /// 母工單歷史資料。
    /// </summary>
    public class WorkOrderHistoryDto
    {
        /// <summary>
        /// 工單號碼。
        /// </summary>
        public string OrderID { get; set; } = "";

        /// <summary>
        /// 製程編號。
        /// </summary>
        public string ProcessID { get; set; } = "";

        /// <summary>
        /// 工件 Tag。
        /// </summary>
        public string WorkpieceTag { get; set; } = "";

        /// <summary>
        /// 優先權。
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 工單狀態。
        /// </summary>
        public string Status { get; set; } = "";

        /// <summary>
        /// 品質結果。
        /// </summary>
        public string QualityResult { get; set; } = "";

        /// <summary>
        /// 分支總數。
        /// </summary>
        public int TotalRouteCount { get; set; }

        /// <summary>
        /// 已完成分支數量。
        /// </summary>
        public int CompletedRouteCount { get; set; }

        /// <summary>
        /// 建立時間。
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 開始時間。
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 結束時間。
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 移入歷史區的時間。
        /// </summary>
        public DateTime ArchivedTime { get; set; }
    }

    /// <summary>
    /// 子任務歷史資料。
    /// </summary>
    public class WorkOrderTaskHistoryDto
    {
        /// <summary>
        /// 原始任務流水號。
        /// </summary>
        public int TaskID { get; set; }

        /// <summary>
        /// 關聯工單號碼。
        /// </summary>
        public string OrderID { get; set; } = "";

        /// <summary>
        /// 分支代碼。
        /// </summary>
        public string RouteCode { get; set; } = "";

        /// <summary>
        /// 步驟順序。
        /// </summary>
        public int StepSequence { get; set; }

        /// <summary>
        /// 動作類型。
        /// </summary>
        public string ActionType { get; set; } = "";

        /// <summary>
        /// 任務備註。
        /// </summary>
        public string Remark { get; set; } = "";

        /// <summary>
        /// 時間統計分類代號。
        /// </summary>
        public string TimeStatCode { get; set; } = "";

        /// <summary>
        /// 所需角色代號。
        /// </summary>
        public string RequiredRoles { get; set; } = "";

        /// <summary>
        /// 任務參數 JSON。
        /// </summary>
        public string ParametersJson { get; set; } = "";

        /// <summary>
        /// 外傳傳送參數。
        /// </summary>
        public string ComParams { get; set; } = "";

        /// <summary>
        /// 執行節點。
        /// </summary>
        public string ExecuteNode { get; set; } = "";

        /// <summary>
        /// 目標設備編號。
        /// </summary>
        public string TargetDeviceID { get; set; } = "";

        /// <summary>
        /// 任務狀態。
        /// </summary>
        public string TaskStatus { get; set; } = "";

        /// <summary>
        /// 優先權等級。
        /// </summary>
        public int PriorityLevel { get; set; }

        /// <summary>
        /// 是否阻塞。
        /// </summary>
        public bool IsBlocking { get; set; }

        /// <summary>
        /// 錯誤訊息。
        /// </summary>
        public string ErrorMessage { get; set; } = "";

        /// <summary>
        /// 錯誤時間。
        /// </summary>
        public DateTime? ErrorTime { get; set; }

        /// <summary>
        /// 開始時間。
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 結束時間。
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 移入歷史區的時間。
        /// </summary>
        public DateTime ArchivedTime { get; set; }
    }
}
