
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class Storage
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } //現有：ID

        //StorageOverviewControl
        public string StorageName{ get; set; } //現有：儲位名稱
        public string StorageNumber { get; set; } //現有：儲位編號
        public int Region { get; set; } //現有：區域                   //No Use
        public int Column { get; set; } //現有：列
        public int Row { get; set; } //現有：行
        public string OndeskTagserial { get; set; } //現有：桌上型標籤序號
        public string State { get; set; }  //現有：狀態 (可用/不可用/維護中)
        public bool? Restriction { get; set; } //現有：是否有限制  (true/false)
        public string Note {  get; set; } //現有：說明
    }
}
