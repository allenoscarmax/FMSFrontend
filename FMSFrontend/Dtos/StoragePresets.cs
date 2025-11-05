using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace OSCARMAXFMS_V3.DBmodels
{
    // 儲位（單位倉）預設設定：定義各單位倉的版型/尺寸
    public class StoragePresets
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = ""; // ID（MongoDB 物件識別碼）
        //未知
        public string storageName { get; set; } = "";// 單位倉名稱（如 ES1、W3）
        public string storageNumber { get; set; } = "";// 單位倉內碼/代碼
        public int regionNumber { get; set; } // 區域編號/群組
        public int columnCount { get; set; } // 行數（對應 VM.Col；水平方向格數）
        public int rowCount { get; set; } // 列數（對應 VM.Row；垂直方向格數）
        public string note { get; set; } = "";// 備註
    }
}
