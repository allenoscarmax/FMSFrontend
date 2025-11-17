using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;


namespace FMSFrontend.Features.Dtos
{
    public class WorksheetsDto
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = "";
        public string worksheetNumber { get; set; } = "";
        public string workpieceName { get; set; } = "";
        public int? workPriority { get; set; }
        public bool? workEnabled { get; set; }
        public string workStatus { get; set; } = "";
        public string workPercentage { get; set; } = "";
        public string targetEDM { get; set; } = "";
        public int? processStep { get; set; }
        public int? totalProcessStep { get; set; }
        public string coordinate { get; set; } = "";
        public string setupUser { get; set; } = "";
    }

    public enum WorksheetStatus
    {
        [Description("新加入工單")]
        New, //灰
        [Description("等待工件可派工(組裝量測OK)")]
        Waiting,
        [Description("待派工")]
        Queue, //黃
        [Description("加工中")]
        Processing, //綠
        [Description("加工完成")]
        Completed, //藍色
        [Description("已暫停")]
        Paused,  //
        [Description("失敗")]
        Failure, //紅色
    }
    public enum WorkCommand
    {
        [Description("設定")]
        Setup,
        [Description("派工")]
        Dispatched,
        [Description("開始")]
        Start,
        [Description("結束")]
        Finish
    }
    
}