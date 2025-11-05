using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OSCARMAXFMS_V3.DBmodels
{
    // 機台主檔：記錄機台識別、通訊、當前夾持與狀態等資訊
    public class Machines
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = ""; // ID（MongoDB 物件識別碼）

        //設備總覽頁面
        //設備總覽
        public string machineName { get; set; } = "";// 機台名稱（顯示用）
        public string status { get; set; } = "";// 狀態（如 Idle/Running/Alarm/Offline）
        public string onDeckElectrodeSerial { get; set; } = "";// 夾持中電極標籤序號（RFID）
        public string onDeckWorksheetSerial { get; set; } = "";// 當前工單號/序號    
        public string onDeckWorkpieceSerial { get; set; } = "";// 夾持中工件標籤序號（RFID）
        //
        //加工程式??
        //
        //加工時間??
        //
        //加工進度??
        //
        //刀具號碼??
        //
        //機台溫度??
        //
        //主軸轉速??
        //
        //油位狀態??
        //
        //冷卻液量??
        //
        //工作座標??
        //
        //加工參數??
        //
        //Type?

        //未知
        public string MachineCode { get; set; } = "";// 機台代碼（系統內唯一識別，如 EDM1）
        public int ProductionLine { get; set; } // 所屬產線編號
        public int MachineNumber { get; set; } // 機台編號（現場資產/牌號）
        public string IP { get; set; } = "";// 機台 IP 位址
        public string Port { get; set; } = ""; // 通訊埠（字串；若需比對/排序可考慮改為數值型別）
        public string RemotePassword { get; set; } = "";// 遠端連線密碼（建議儲存雜湊，不存明文）
        public string RemotePath { get; set; } = "";// 遠端路徑（如 NC 程式/共享路徑）
        public string SetupUser { get; set; } = "";// 建置/設定人員（帳號或姓名）
    }
}
