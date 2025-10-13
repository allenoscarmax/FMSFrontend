using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class EquipmentUtillization
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string MachineCode {get;set;}
        public string Status { get;set;}
        public int StatusId {  get;set;}
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)] // 強制存本地時間
        public DateTime StartTime { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)] // 強制存本地時間
        public DateTime EndTime { get; set; }


    }
    public enum EquipmentStatus
    {
        Disconnection,
        Running,
        Stopping,
        EmergencyStop,
    }
}
