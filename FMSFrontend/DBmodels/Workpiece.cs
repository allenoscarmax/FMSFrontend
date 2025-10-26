using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OSCARMAXFMS_V3.DBmodels
{
    [BsonIgnoreExtraElements]
    public class Workpiece // 工件
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = "";// ID（MongoDB 物件識別碼）
        public string tagSerial { get; set; } = "";// 物料標籤序號（RFID/NFC）
        public string worksheetNumber { get; set; } = ""; // 工單編號
        public string workpieceName { get; set; } = ""; // 工件名稱
        public string status { get; set; } = "";// 狀態（加工/占用等狀態字串，如 Working/Verified/Completed/Reserved/Error/Empty）
        public bool? restriction { set; get; } // 是否限制/鎖定（true=限制使用）
        public string pairedEDM { get; set; } = "";// 配對之 EDM 機台/流程識別
        public string currentLocation { get; set; } = ""; // 目前所在位置（可為機台代碼或儲位碼，如 E:1:1:1:1）
        public string tempRetSLocation { get; set; } = "";// 暫存返回儲位位置（例如 WR:1:1:1:1）
        public bool isCompleted { set; get; } // 是否已完成（流程終結）
        public string edmpgm { get; set; } = "";// EDM 加工程式名稱/代碼
        public bool? needInspect { get; set; } // 是否需要檢驗（可為 null 表示未知）
        public bool? inspected { get; set; } // 是否已檢驗（可為 null 表示未知）
        public string inspectStatus { get; set; } = ""; // 檢驗結果狀態（例如 PASS/FAIL/NG/待檢）
        public string inspectOffset { get; set; } = ""; // 檢驗偏移/量測值（文字表示，單位/格式依系統定義）
        public string setupUser { get; set; } = ""; // 建置/設定人員帳號或名稱
    }
}
