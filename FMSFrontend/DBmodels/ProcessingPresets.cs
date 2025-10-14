using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class ProcessingPresets // 製程預設（流程與機群關聯的模板）
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } // ID（MongoDB 物件識別碼）
        public string ProcessName { get; set; } // 製程名稱（如 EDM/清洗/量測）
        public string MachineGroupNumber { get; set; } // 機群編號（可指定可執行此製程的設備群組）
        public string OperationNumber { get; set; } // 作業/工序代碼（對應 OperationPresets.OperationNumber）
        public string SetupUser { get; set; } // 建置/設定人員（帳號或姓名）
    }
}
