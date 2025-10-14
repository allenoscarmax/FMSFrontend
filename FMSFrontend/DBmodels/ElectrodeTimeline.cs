using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace OSCARMAXFMS_V3.DBmodels
{
    // 電極時間軸事件紀錄：記錄電極在製程中的關鍵事件與關聯對象
    public class ElectrodeTimeline
    {

        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } // ID（MongoDB 物件識別碼）
       
        //工作列表頁面
        public string ElectrodeId { get; set; } // 相關電極 ID（對應 Electrode._id）
        public string WorkCommand { get; set; } // 作業事件/命令（如 入庫/檢驗完成/加工開始/加工完成/出庫…）

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)] // 強制存本地時間    
        public DateTime TimeStamp { get; set; } // 事件時間戳記（本地時區）
        public string WorksheetId { get; set; } // 相關工單 ID（對應 Worksheets._id）
        public string EDMNumber { get; set; } // 哪一台 EDM（如 EDM1/EDM2）
       
        //未知
        public string WorkpieceId { get; set; } // 相關工件 ID（對應 Workpiece._id）

    }
}
