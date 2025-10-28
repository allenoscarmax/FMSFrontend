using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OSCARMAXFMS_V3.DBmodels
{
    // 工單主檔資料模型（不含時間軸；含基本屬性與進度摘要）
    [BsonIgnoreExtraElements]
    public class Worksheets
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = "";// ID（MongoDB 物件識別碼）
        public string worksheetNumber { get; set; } = ""; // 工單編號
        public string workpieceName { get; set; } = "";  // 零件名稱
        public string workStatus { get; set; } = "";  // 工單狀態（例如：Queued/Running/Completed/Failed 等）
        public string targetEDM { get; set; } = "";   // 目標 EDM 機台代號/指定機
        public string coordinate { get; set; } = "";   // 座標/治具座標資訊（字串格式，依系統定義）
        public string setupUser { get; set; } = "";      // 建置/設定人員（帳號或姓名）
        public string workPercentage { get; set; } = "";  // 進度百分比（字串，如 "0%"、"75%"、"100%"）
        public int? processStep { get; set; }        // 目前步驟序號（1-based）
        public int? totalProcessStep { get; set; }   // 總步驟數
        public int? workPriority { get; set; }       // 作業優先權等級（數字大小與優先順序由系統定義）
        public bool? workEnabled { get; set; }       // 是否啟用/可排程（true=可排程）
    }
}
