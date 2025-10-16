using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class WorksheetsTimeline // 工單時間軸事件紀錄：記錄工單在製程中的關鍵事件
    {

            [BsonRepresentation(BsonType.ObjectId)]
            public string _id { get; set; } // ID（MongoDB 物件識別碼）
            public int WorkSheetId { get; set; } // 工單內碼/整數識別
            public string WorkSheetSerial { get; set; } // 工單序號/編碼（顯示或關聯用）
            public string WorkCommand { get; set; } // 作業事件/命令（如 入庫/派工/開始/完成/出庫 等）
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)] // 強制存本地時間
            public DateTime TimeStampe { get; set; } // 事件時間戳記（本地時區；名稱可能為 Timestamp）
            public string ElectrodeSerial { get; set; } // 相關電極標籤序號（RFID；若無可留空）
            public string EDMnumber { get; set; } // 相關 EDM 機台（如 EDM1；名稱建議統一為 EDMNumber）
            

    }
}
