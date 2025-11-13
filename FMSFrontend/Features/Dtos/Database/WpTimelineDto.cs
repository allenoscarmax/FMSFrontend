using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel;

namespace FMSFrontend.Features.Dtos
{
    public class WpTimelineDto // 工件時間軸事件紀錄
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = "";// ID（MongoDB 物件識別碼）

        public string workpieceSerialNumber { get; set; } = "";// 工件序號（或標籤序號）；對應 Workpiece.TagSerial／工件唯一識別

        public string workCommand { get; set; } = "";// 作業事件/命令（如 入庫/檢驗完成/加工開始/加工完成/出庫 等）

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)] // 強制存本地時間（避免落庫成 UTC 後與前端顯示偏差）
        public DateTime timeStampe { get; set; } // 事件時間戳記（本地時區）

        public string electrodeId { get; set; } = ""; // 相關電極 ID（若事件涉及對應電極則填）

        public string eDMNumber { get; set; } = ""; // 相關 EDM 機台編號（事件發生或關聯之機台）
    }
    public enum WorkpieceHistoryState
    {
        [Description("燒錄")]
        Setup,
        [Description("加工完成")]
        Finish,
        [Description("重做")]
        Rework,
        [Description("上機台")]
        Load,
        [Description("已派工")]
        Dispatched,
        [Description("被加工")]
        Machined,
        [Description("翻面加工")]
        TurnOver,
        [Description("開始加工")]
        Start,
        [Description("上清洗機")]
        Wash,
        [Description("上CMM")]
        Measure,
        [Description("出倉")]
        Offshlf,
    }
}
