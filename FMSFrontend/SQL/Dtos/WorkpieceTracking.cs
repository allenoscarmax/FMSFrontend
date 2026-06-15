using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;
using System;

namespace FMSFrontend.SQL.Dtos
{
    /// <summary>
    /// 工件追蹤與履歷資料。
    /// </summary>
    public class WorkpieceTrackingDto
    {
        /// <summary>
        /// 實體 RFID 或條碼。
        /// </summary>
        public string WorkpieceTag { get; set; } = "";

        /// <summary>
        /// 圖號或產品料號。
        /// </summary>
        public string PartNo { get; set; } = "";

        /// <summary>
        /// 綁定的工單號碼。
        /// </summary>
        public string BoundOrderID { get; set; } = "";

        /// <summary>
        /// 目前工件狀態，例如 `Raw`、`WIP`、`Finished`、`NG`。
        /// </summary>
        public string CurrentStatus { get; set; } = "Raw";

        /// <summary>
        /// 目前物理位置。
        /// </summary>
        public string CurrentLocation { get; set; } = "WIP_Zone";

        /// <summary>
        /// 工件專屬量測或補正資料 JSON。
        /// </summary>
        public string MeasuredDataJson { get; set; } = "{}";

        /// <summary>
        /// 預設歸位位置。
        /// </summary>
        public string HomeLocation { get; set; } = "";

        /// <summary>
        /// 建檔時間。
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 最後更新時間。
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
