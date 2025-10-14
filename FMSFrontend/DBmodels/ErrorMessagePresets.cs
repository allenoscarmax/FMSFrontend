using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSCARMAXFMS_V3.DBmodels
{
    // 錯誤訊息預設字典：以 ErrorCode 對應中/英文訊息，供告警/日誌顯示使用
    public class ErrorMessagePresets
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } // ID（MongoDB 物件識別碼）
        public string ErrorCode { get; set; } // 錯誤代碼鍵（用於查表）
        public string Message_cn { get; set; } // 中文預設訊息
        public string Message_en { get; set; } // 英文預設訊息
    }
}
