using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OSCARMAXFMS_V3.DBmodels
{
    // 人員主檔
    public class Worker
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = ""; // ID（MongoDB 物件識別碼）

        //未知
        public string workerNumber { get; set; } = ""; // 員工編號
        public string workerName { get; set; } = ""; // 員工姓名
        public string accountGroup { get; set; } = ""; // 權限群組/角色
        public string accountName { get; set; } = ""; // 登入帳號
        public string password { get; set; } = ""; // 密碼（建議儲存雜湊，例如使用 SHA256 或 BCrypt）
    }
}
