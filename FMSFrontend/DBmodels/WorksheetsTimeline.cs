using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class WorksheetsTimeline
    {

            [BsonRepresentation(BsonType.ObjectId)]
            public string _id { get; set; }
            public int WorkSheetId { get; set; }
            public string WorkSheetSerial { get; set; }
            public string WorkCommand { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)] // 強制存本地時間
            public DateTime TimeStampe { get; set; }
            public string ElectrodeSerial { get; set; }
            public string EDMnumber { get; set; }
            

    }
}
