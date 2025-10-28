using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace OSCARMAXFMS_V3.DBmodels
{
    // 錯誤訊息日誌：記錄系統/設備在某時刻發生的錯誤與處置狀態
    public class ErrorMessageLog
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = ""; // ID（MongoDB 物件識別碼）
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)] // 強制存本地時間
        public DateTime timeStamp { get; set; } // 發生時間（本地時區）
        public string errorCode { get; set; } = "";// 錯誤代碼（來源系統/設備的錯誤碼）
        public string message_cn { get; set; } = "";// 錯誤訊息（中文）
        public string message_en { get; set; } = "";// 錯誤訊息（英文）
        public string note { get; set; } = ""; // 備註（處置說明/附註）
        public string whichLine { get; set; } = ""; // 所屬線別/產線（例如 Line1；視實際定義也可用於群組或設備區）
        public bool isAverted { get; set; } // 是否已排除/解除（true=已排除）
    }
}
