using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class ErrorMessageLog
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)] // 強制存本地時間
        public DateTime TimeStamp {  get; set; }
        public string ErrorCode {  get; set; }
        public string Message_cn {  get; set; }
        public string Message_en {  get; set; }
        public string Note { get; set; }
        public string WhichLine {  get; set; }
        public bool IsAverted {  get; set; }


    }
}
