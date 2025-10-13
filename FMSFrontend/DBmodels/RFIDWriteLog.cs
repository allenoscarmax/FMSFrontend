using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class RFIDWriteLog
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)] // 強制存本地時間
        public DateTime TimeStamp {  get; set; }
        public string Type {  get; set; }
        public string SrialNo {  get; set; }
        public string TagSerial {  get; set; }
        public string ObjName {  get; set; }
        public string SetupUser {  get; set; }
    }
}
