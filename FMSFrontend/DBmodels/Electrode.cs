using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OSCARMAXFMS_V3.DBmodels
{
    // 電極主檔（資料庫模型）：描述電極的身分、位置、配對、補償、壽命與檢驗等資訊
    public class Electrode
    {

        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } // MongoDB 物件識別碼

        //電極資訊頁面
        //
        //治具名稱??
        // public string JigSerial { get; set; } 
        public string ElectrodeName { get; set; } // 電極名稱/品名（如 E01、ELE-001）
        //
        //電極編號??
        public string ElectrodeType { get; set; } // 電極型式（Square/Round/Custom…）
        public string State { get; set; } // 狀態（Verified /Working /Error /Completed /Reserved）
        //
        //電極夾定氣編號?
        public string TagSerial { get; set; } // 標籤序號（RFID/NFC UID），前端多處用於辨識/顯示
        public int? LifeTimes { get; set; } // 壽命上限（最大可用次數）        
        //
        //使用率??
        public string Offset { get; set; } // 補償量（字串存 xyzabc，例："x=...,y=...,z=...,a=...,b=...,c=..."）
        public int? UseTimes { get; set; } // 已使用次數
        public bool? Restriction { get; set; } // 限制/鎖定（true=不可用/預留）
        //            

        //未知
        public string WorksheetNumber { get; set; } // 所屬工單號（關聯工單）
        public string WorksheetDone { get; set; } // 工單完成註記/狀態（字串；若僅需布林建議改為 bool）
        public string CurrentLocation { get; set; } // 目前所在位置（例如倉位碼 E:Line:Row:Col:Layer 或 onEDMx）
        public string PairedEDM { get; set; } // 已配對之 EDM 機台（EDM1/EDM2…）
        public int? OffsetStatus { get; set; } // 補償流程模式：0=未補償；1=量測含加工（量→加→量→加）；2=量測含加工（全量後→全加工）；3=無量測載入補償加工
        public bool ComplementUpload { get; set; } // 僅在狀態3需要載入補償；載入後設為 true
        public string UnderSize { get; set; } // 欠量/負公差設定（字串；若需運算建議改為數值）

        public string EDMPGM { get; set; } // EDM 加工程式代碼（P1/P2…）
        public bool? Measuremented { get; set; } // 是否已量測（對應 OffsetStatus 1 或 2 才需）
        public string MeasurementStatus { get; set; } // 量測結果（PASS/FAIL/NG/OK…）
        public string TempRetSLocation { get; set; } // 暫存返回儲位（例：ER:1:1:1:1 或 WR:...）
        public string SetupUser { get; set; } // 建置/設定人員（帳號或姓名）
    }
}
