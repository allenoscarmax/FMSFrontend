using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OSCARMAXFMS_V3.DBmodels
{
    // 工單主檔資料模型（不含時間軸；含基本屬性與進度摘要）
  //  [BsonIgnoreExtraElements]
    public class Worksheets
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string? WorksheetNumber { get; set; }
        public string? WorkpieceName { get; set; }
        public int? WorkPriority { get; set; }
        public bool? WorkEnabled { get; set; }
        public string? WorkStatus { get; set; }
        public string? WorkPercentage { get; set; }
        public string? TargetEDM { get; set; }
        public int? ProcessStep { get; set; }
        public int? TotalProcessStep { get; set; }
        public string? Coordinate { get; set; }
        public string? SetupUser { get; set; }
    }
}
