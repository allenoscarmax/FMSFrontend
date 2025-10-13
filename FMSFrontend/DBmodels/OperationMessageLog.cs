using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class OperationMessageLog
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)] // 強制存本地時間
        public DateTime TimeStamp {  get; set; }
        public string Message_cn {  get; set; }
        public string Mesage_en {  get; set; }
        public string SetupUser { get; set; }

    }
}
