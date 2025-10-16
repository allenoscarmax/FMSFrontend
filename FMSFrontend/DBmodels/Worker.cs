using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OSCARMAXFMS_V3.DBmodels
{
    // 人員主檔
    public class Worker
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } // ID（MongoDB 物件識別碼）

        //未知
        public string WorkerNumber { get; set; } // 員工編號
        public string WorkerName { get; set; } // 員工姓名
        public string AccountGroup { get; set; } // 權限群組/角色
        public string AccountName { get; set; } // 登入帳號
        public string Password { get; set; } // 密碼（建議儲存雜湊，例如使用 SHA256 或 BCrypt）
    }
}
