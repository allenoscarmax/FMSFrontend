using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;


namespace FMSFrontend.Features.Dtos
{
    public class MachinesDto
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = ""; // ID（MongoDB 物件識別碼）
        public string machineCode { get; set; } = "";// 機台代碼（系統內唯一識別，如 EDM1）
        public int productionLine { get; set; } // 所屬產線編號
        public int machineNumber { get; set; } // 機台編號（現場資產/牌號）
        public string machineName { get; set; } = "";// 機台名稱（顯示用）
        public string ip { get; set; } = "";// 機台 IP 位址
        public string port { get; set; } = ""; // 通訊埠（字串；若需比對/排序可考慮改為數值型別）
        public string remotePassword { get; set; } = "";// 遠端連線密碼（建議儲存雜湊，不存明文）
        public string remotePath { get; set; } = "";// 遠端路徑（如 NC 程式/共享路徑）
        public string status { get; set; } = "";// 狀態（如 Idle/Running/Alarm/Offline）
        public string onDeckElectrodeSerial { get; set; } = "";// 夾持中電極標籤序號（RFID）
        public string onDeckWorkpieceSerial { get; set; } = "";// 夾持中工件標籤序號（RFID）
        public string onDeckWorksheetSerial { get; set; } = "";// 當前工單號/序號    
        public string setupUser { get; set; } = "";// 建置/設定人員（帳號或姓名）
    }

    public enum MachineStatus
    {
        Connection = 4,
        [Description("離線中")]
        Disconnection = 0,
        [Description("運作中")]
        Running = 1,
        [Description("閒置中")]
        Stopping = 2,
        [Description("急停中")]
        EmergencyStop = 3
    }
}