using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSCARMAXFMS_V3.DBmodels
{
    // 排屑排程設定：記錄每台排屑設備在產線上的固定與異常排程
    public class ChipRemoveSchedule
    {
        [BsonRepresentation(BsonType.ObjectId)] 
        public string _id { get; set; } // MongoDB 物件識別碼


        //未知
        public string ChipRemoveMachineName { get; set; } // 排屑設備/機台名稱
        public string ProductionLine { get; set; } // 所屬產線（第 x 條產線）
      
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)] // 強制存本地時間
        public DateTime FixedScheduleTime { get; set; } // 固定當日排屑時間（每日固定執行時間）
        public string addTime { get; set; } // 下次的間隔（單位：小時；字串存放，建議改為數值或 TimeSpan）
       
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)] // 強制存本地時間
        public DateTime UnusualTimeSchedule { get; set; } // 異常觸發後的排程時間（第一次為 固定時間 + 間隔時間的一半）
        public bool UnusualTimeTrigger { get; set; } // 是否啟用異常排程（true=已觸發）
    }
}
