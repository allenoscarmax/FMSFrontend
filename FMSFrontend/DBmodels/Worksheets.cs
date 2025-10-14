using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OSCARMAXFMS_V3.DBmodels
{
    // 工單主檔資料模型（不含時間軸；含基本屬性與進度摘要）
    public class Worksheets
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } // ID（MongoDB 物件識別碼）

        //表格資料
        public string WorksheetNumber { get; set; }  // 工單編號
        public string WorkpieceName { get; set; }    // 零件名稱
        public string WorkStatus { get; set; }       // 工單狀態（例如：Queued/Running/Completed/Failed 等）
        public string TargetEDM { get; set; }        // 目標 EDM 機台代號/指定機
        public string Coordinate { get; set; }       // 座標/治具座標資訊（字串格式，依系統定義）
        public string SetupUser { get; set; }        // 建置/設定人員（帳號或姓名）

        //未知
        public string WorkPercentage { get; set; }   // 進度百分比（字串，如 "0%"、"75%"、"100%"）
        public int? ProcessStep { get; set; }        // 目前步驟序號（1-based）
        public int? TotalProcessStep { get; set; }   // 總步驟數
        public int? WorkPriority { get; set; }       // 作業優先權等級（數字大小與優先順序由系統定義）
        public bool? WorkEnabled { get; set; }       // 是否啟用/可排程（true=可排程）
    }
}
