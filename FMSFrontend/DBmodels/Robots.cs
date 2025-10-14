using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class Robots // 機器人主檔資料模型
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } // ID（MongoDB 物件識別碼）
        public string RobotName { get; set; } // 機器人名稱（顯示用）
        public string Status { get; set; } // 狀態（如 Idle/Running/Alarm/Offline 等）
 

        //待確認
        //目前位置??
        //private string currentLocation { get; set; }
        //目前動作??
        //public string currentAction { get; set; } // 狀態（如 Idle/Running/Alarm/Offline 等）
        //下一步??
        //public string nextAction { get; set; } // 狀態（如 Idle/Running/Alarm/Offline 等）

        //未知?
        public string RobotCode { get; set; } // 機器人代碼（內部識別碼）
        public int ProductionLine { get; set; } // 所屬產線/單元編號
        public string RobotNumber { get; set; } // 機器編號（現場標識/資產編號）
        public string RobotType { get; set; } // 機器種類/型號（如 6-Axis/SCARA/AGV 等）
        public string Robot_IP { get; set; } // 通訊 IP 位址
        public string Image { get; set; } // 圖片路徑或 URL（用於前端顯示）
        public string OnDeckObjSerial { get; set; } // 夾持/當前作業物件序號（標籤序，若有）
        public string SetupUser { get; set; } // 建置/設定人員（帳號或姓名）
    }
}
