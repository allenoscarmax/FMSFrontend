using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class OperationMessageLog // 操作訊息日誌：記錄系統或使用者在某時刻的操作訊息
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = ""; // ID（MongoDB 物件識別碼）
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)] // 強制存本地時間
        public DateTime timeStamp {  get; set; } // 發生時間（本地時區）
        public string message_cn {  get; set; } = "";// 訊息內容（中文）
        public string mesage_en {  get; set; } = "";// 訊息內容（英文；名稱可能應為 Message_en）
        public string setupUser { get; set; } = ""; // 操作人員（帳號或姓名）

    }
}
