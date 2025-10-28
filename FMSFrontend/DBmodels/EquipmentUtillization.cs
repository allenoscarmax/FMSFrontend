using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace OSCARMAXFMS_V3.DBmodels
{
    // 設備稼動/利用率狀態區間紀錄（每筆代表某機台在一段期間內的狀態）
    public class EquipmentUtillization
    {

        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = ""; // ID（MongoDB 物件識別碼）

        //未知
        public string machineCode { get; set; } = "";// 機台代碼（設備唯一識別，例如 EDM1）
        public string status { get; set; } = ""; // 狀態字串（與 StatusId/EquipmentStatus 對應；相容或顯示用）
        public int statusId { get; set; } // 狀態代碼（對應 EquipmentStatus 的整數值）
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)] // 強制存本地時間
        public DateTime startTime { get; set; } // 狀態開始時間（本地時區）
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)] // 強制存本地時間
        public DateTime endTime { get; set; } // 狀態結束時間（本地時區；若進行中可為目前時間或空值，依設計）
    }
    public enum EquipmentStatus
    {
        Disconnection, // 斷線
        Running, // 運轉
        Stopping, // 停止
        EmergencyStop, // 急停
    }
}
