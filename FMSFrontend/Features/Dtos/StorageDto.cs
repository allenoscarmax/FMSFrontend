using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FMSFrontend.Features.Dtos
{
    [BsonIgnoreExtraElements]
    public class Storage
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = "";//現有：ID
        public string storageName{ get; set; } = "";//現有：儲位名稱
        public string storageNumber { get; set; } = "";//現有：儲位編號
        public int region { get; set; } //現有：區域               
        public int column { get; set; } //現有：列
        public int row { get; set; } //現有：行
        public string ondeskTagserial { get; set; } = "";//現有：桌上型標籤序號
        public string state { get; set; } = "";//現有：狀態 (可用/不可用/維護中)
        public bool? restriction { get; set; } //現有：是否有限制  (true/false)
        public string note {  get; set; } = ""; //現有：說明
    }
}
