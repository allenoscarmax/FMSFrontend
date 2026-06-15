using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;
using System;

namespace FMSFrontend.SQL.Dtos
{
    /// <summary>
    /// 電極或刀具庫存資料。
    /// </summary>
    public class ElectrodeInventoryDto
    {
        /// <summary>
        /// 實體電極 Tag。
        /// </summary>
        public string ElectrodeID { get; set; } = "";

        /// <summary>
        /// 電極規格型號。
        /// </summary>
        public string ElectrodeType { get; set; } = "";

        /// <summary>
        /// 目前位置，例如倉位或主軸位置。
        /// </summary>
        public string Location { get; set; } = "Magazine";

        /// <summary>
        /// 最大可使用次數。
        /// </summary>
        public int MaxUsageCount { get; set; }

        /// <summary>
        /// 目前已使用次數。
        /// </summary>
        public int CurrentUsageCount { get; set; }

        /// <summary>
        /// 刀具健康狀態，例如 `Idle`、`InUse`、`Depleted`。
        /// </summary>
        public string Status { get; set; } = "Idle";

        /// <summary>
        /// 物理加工狀態，例如 `Raw`、`Machined`、`Measured`。
        /// </summary>
        public string CurrentStatus { get; set; } = "Raw";

        /// <summary>
        /// 補正參數 JSON。
        /// </summary>
        public string OffsetsJson { get; set; } = "{}";

        /// <summary>
        /// 預設歸位位置。
        /// </summary>
        public string HomeLocation { get; set; } = "";

        /// <summary>
        /// 最後更新時間。
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }
}
