using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace OSCARMAXFMS_V3.DBmodels
{
    // 設備主檔資料模型：記錄設備識別、通訊與所屬產線等基本資訊
    public class Devices
    {
        [BsonRepresentation(BsonType.ObjectId)]
        
        //未知
        public string _id { get; set; } = "";// ID（MongoDB 物件識別碼）
        public string deviceNumber { get; set; } = ""; // 設備編號（資產/內碼）
        public string deviceName { get; set; } = ""; // 設備名稱（顯示用）
        public string deviceIP { get; set; } = ""; // 設備 IP 位址（IPv4/IPv6）
        public string devicePort { get; set; } = ""; // 通訊埠（字串存放；如需比對/排序可考慮改為數值型別）
        public int productionLine { get; set; } // 所屬產線編號
        public string setupUser { get; set; } = ""; // 建置/設定人員（帳號或姓名）
    }
}
