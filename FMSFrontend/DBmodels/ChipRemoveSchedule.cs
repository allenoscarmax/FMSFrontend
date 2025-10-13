using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class ChipRemoveSchedule
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string ChipRemoveMachineName {  get; set; }
        public string ProductionLine { get; set; } //第x條產線
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)] // 強制存本地時間
        public DateTime FixedScheduleTime { get; set; } //固定當日排屑時間
        public string addTime { get; set; } //下次的間隔 (單位小時)
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)] // 強制存本地時間
        public DateTime UnusualTimeSchedule { get; set; } //異常觸發後的時間 (第一筆時間是固定時間+間格時間的一半)
        public bool UnusualTimeTrigger {  get; set; }
    }
}
