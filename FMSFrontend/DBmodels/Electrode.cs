using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OSCARMAXFMS_V3.DBmodels
{
    // 電極主檔（資料庫模型）：描述電極的身分、位置、配對、補償、壽命與檢驗等資訊
    [BsonIgnoreExtraElements]
    public class Electrode
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } // MongoDB 物件識別碼
        public string worksheetNumber { get; set; } // 所屬工單號（關聯工單）
        public string worksheetDone { get; set; } // 工單完成註記/狀態（允許 null）
        public string tagSerial { get; set; } // 標籤序號（RFID/NFC UID），前端多處用於辨識/顯示
        public string electrodeName { get; set; } // 電極名稱/品名（如 E01、ELE-001）
        public string electrodeType { get; set; } // 電極型式（Square/Round/Custom…）
        public string currentLocation { get; set; } // 目前所在位置（例如倉位碼 E:Line:Row:Col:Layer 或 onEDMx）
        public string state { get; set; } // 狀態（New/Verified/Working/Error/Completed/Reserved）
        public string pairedEDM { get; set; } // 已配對之 EDM 機台（EDM1/EDM2…）
        public int? offsetStatus { get; set; } // 補償流程模式：0=未補償；1=量測含加工；2=量測含加工（全量後→全加工）；3=無量測載入補償加工
        public string offset { get; set; } // 補償量（字串存 xyzabc 或單一數值字串）
        public bool complementUpload { get; set; } // 僅在狀態3需要載入補償；載入後設為 true
        public int? lifeTimes { get; set; } // 壽命上限（最大可用次數）
        public int? useTimes { get; set; } // 已使用次數
        public string underSize { get; set; } // 欠量/負公差設定（允許 null）
        public bool restriction { get; set; } // 限制/鎖定（false=可用/非預留）
        public string edmpgm { get; set; } // EDM 加工程式代碼（P1/P2…）

        // 新增對應 JSON 的欄位名稱（允許 null）
        [BsonElement("edM_offsetPGM")]
        public string edM_offsetPGM { get; set; }

        public bool measuremented { get; set; } // 是否已量測（對應 OffsetStatus 1 或 2 才需）
        public string measurementStatus { get; set; } // 量測結果（PASS/FAIL/NG/OK…）
        public string tempRetSLocation { get; set; } // 暫存返回儲位（例：ER:1:1:1:1 或 WR:...）
        public string setupUser { get; set; } // 建置/設定人員（帳號或姓名）
    }
}
