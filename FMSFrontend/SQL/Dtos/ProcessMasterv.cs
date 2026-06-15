using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;
using System;

namespace FMSFrontend.SQL.Dtos
{
    /// <summary>
    /// 製程主檔。
    /// </summary>
    public class ProcessMasterDto
    {
        /// <summary>
        /// 製程編號。
        /// </summary>
        public string ProcessID { get; set; } = "";

        /// <summary>
        /// 製程名稱。
        /// </summary>
        public string ProcessName { get; set; } = "";

        /// <summary>
        /// 備註說明。
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// 是否啟用。
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 建立時間。
        /// </summary>
        public DateTime CreatedTime { get; set; }
    }

    /// <summary>
    /// 製程步驟明細。
    /// </summary>
    public class ProcessStepDto
    {
        /// <summary>
        /// 步驟流水號。
        /// </summary>
        public int StepID { get; set; }

        /// <summary>
        /// 所屬製程編號。
        /// </summary>
        public string ProcessID { get; set; } = "";

        /// <summary>
        /// 步驟執行順序。
        /// </summary>
        public int StepSequence { get; set; }

        /// <summary>
        /// 動作類型。
        /// </summary>
        public string ActionType { get; set; } = "";

        /// <summary>
        /// 動作參數 JSON。
        /// </summary>
        public string ParametersJson { get; set; } = "{}";

        /// <summary>
        /// 指定執行節點。
        /// </summary>
        public string ExecuteNode { get; set; } = "";

        /// <summary>
        /// 是否為阻塞步驟。
        /// </summary>
        public bool IsBlocking { get; set; }
    }
}
