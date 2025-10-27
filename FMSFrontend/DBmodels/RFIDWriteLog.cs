using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace OSCARMAXFMS_V3.DBmodels
{
    // RFID 寫入紀錄（寫入標籤時的操作日誌）
    public class RFIDWriteLog
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = ""; // ID（MongoDB 物件識別碼）
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)] // 強制存本地時間
        public DateTime timeStamp { get; set; } // 寫入時間（本地時區）
        public string type { get; set; } = "";// 物件類型（例如：Electrode/Workpiece/Fixture 等）
        public string srialNo { get; set; } = ""; //工單編號
        public string tagSerial { get; set; } = "";// 標籤序號（RFID/NFC Tag UID）
        public string objName { get; set; } = "";// 物件名稱（工件名稱 / 電極名稱）
        public string setupUser { get; set; } = "";// 操作人員（帳號或姓名）
    }
}
