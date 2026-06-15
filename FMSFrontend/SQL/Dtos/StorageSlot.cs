using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;
using System;

namespace FMSFrontend.SQL.Dtos
{
    /// <summary>
    /// 智慧倉儲格位資料。
    /// </summary>
    public class StorageSlotDto
    {
        /// <summary>
        /// 格位代碼。
        /// </summary>
        public string LocationCode { get; set; } = "";

        /// <summary>
        /// 倉庫名稱，例如 `W` 為工件架、`E` 為電極架。
        /// </summary>
        public string WarehouseName { get; set; } = "";

        /// <summary>
        /// 實體料架編號。
        /// </summary>
        public int? RackID { get; set; }

        /// <summary>
        /// 區域編號。
        /// </summary>
        public int? ZoneID { get; set; }

        /// <summary>
        /// 列索引。
        /// </summary>
        public int? RowIdx { get; set; }

        /// <summary>
        /// 層索引。
        /// </summary>
        public int? LayerIdx { get; set; }

        /// <summary>
        /// 允許存放的物料類型。
        /// </summary>
        public string AllowedPayloads { get; set; } = "Workpiece,Electrode";

        /// <summary>
        /// 邏輯分區，例如 `Raw`、`Finished`、`Exchange`。
        /// </summary>
        public string LogicalZone { get; set; } = "";

        /// <summary>
        /// 是否啟用。
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 格位狀態，例如 `Empty`、`Occupied`、`Reserved_In`、`Reserved_Out`。
        /// </summary>
        public string SlotStatus { get; set; } = "Empty";

        /// <summary>
        /// 預約或鎖定該格位的工單號碼。
        /// </summary>
        public string LockingOrderID { get; set; } = "";

        /// <summary>
        /// 內容物類型。
        /// </summary>
        public string TagType { get; set; } = "";

        /// <summary>
        /// 內容物 Tag ID。
        /// </summary>
        public string TagID { get; set; } = "";

        /// <summary>
        /// 最後一次異常訊息。
        /// </summary>
        public string LastErrorMessage { get; set; } = "";

        /// <summary>
        /// 最後更新時間。
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// 最後交易識別碼。
        /// </summary>
        public string LastTransactionID { get; set; } = "";
    }
}
