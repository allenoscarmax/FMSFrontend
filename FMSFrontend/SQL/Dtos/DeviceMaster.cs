using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;
using System;

namespace FMSFrontend.SQL.Dtos
{
    /// <summary>
    /// 設備資源主檔。
    /// </summary>
    public class DeviceMasterDto
    {
        /// <summary>
        /// 設備代號，例如 `Arm_01`、`CMM_02`。
        /// </summary>
        public string DeviceID { get; set; } = "";

        /// <summary>
        /// 設備類型，例如 `RobotArm`、`Machine`、`AGV`。
        /// </summary>
        public string DeviceType { get; set; } = "";

        /// <summary>
        /// 廠牌或驅動器類型。
        /// </summary>
        public string Brand { get; set; } = "";

        /// <summary>
        /// 是否啟用。
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 支援的動作列表。
        /// </summary>
        public string SupportedActions { get; set; } = "";

        /// <summary>
        /// 可支援或可存取的設備列表。
        /// </summary>
        public string AccessibleDevices { get; set; } = "";

        /// <summary>
        /// 設備狀態，例如 `Idle`、`Busy`、`Error`。
        /// </summary>
        public string DeviceStatus { get; set; } = "Idle";

        /// <summary>
        /// 當前佔用者。
        /// </summary>
        public string CurrentOwnerID { get; set; } = "";

        /// <summary>
        /// 設備所屬節點。
        /// </summary>
        public string OwnerNodeID { get; set; } = "";

        /// <summary>
        /// 連線與硬體參數 JSON。
        /// </summary>
        public string ConnectionConfigJson { get; set; } = "{}";

        /// <summary>
        /// 最後更新時間。
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }
    public class RobotDetal
    {

    }
    public class EDMDetal
    {

    }



}
