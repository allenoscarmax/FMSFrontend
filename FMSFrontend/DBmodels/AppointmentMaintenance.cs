using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSCARMAXFMS_V3.DBmodels
{
    // 保養/維護預約排程（週期性設定）
    public class AppointmentMaintenance
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = ""; // ID（MongoDB 物件識別碼）
        public string maintenance { get; set; } = "";  // 保養項目或設備名稱（例：EDM1 潤滑）
        public string productLine {  get; set; } = "";  // 所屬產線（第 x 條產線）
        public int index {  get; set; } // 該產線中的第幾台
        public string type {  get; set; } = "";   // 排程型別（例：None/Weekly/Monthly）
        public List<int> day_values { get; set; } = new List<int>();  // 週期日清單：Weekly=1(日)~7(6?)(六)；Monthly=1~31（第幾天）
        public int hour { get; set; } // 幾點（時），24 小時制
        public int minute { get; set; } // 幾分（0~59）
        public bool isEnabled { get; set; } = true; // 是否啟用（預設啟用）
        public string note {  get; set; } = ""; // 說明/備註
    }
}
