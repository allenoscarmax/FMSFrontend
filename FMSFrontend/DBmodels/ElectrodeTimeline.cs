using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class ElectrodeTimeline
    {

        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string ElectrodeId { get; set; }
        public string WorkCommand { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)] // 強制存本地時間
        public DateTime TimeStamp { get; set; }
        public string WorkpieceId { get; set; }
        public string WorksheetId { get; set; }
        public string EDMNumber {  get; set; }// 哪一台EDM
    }
}
