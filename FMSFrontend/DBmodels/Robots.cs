using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class Robots
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string RobotCode { get; set; }
        public int ProductionLine { get; set; }
        public string RobotNumber { get; set; }
        public string RobotName { get; set; }
        public string RobotType { get; set; }
        public string Robot_IP { get; set; }
        public string Status { get; set; }
        public string Image { get; set; }
        public string OnDeckObjSerial { get; set; }
        public string SetupUser { get; set; }
    }
}
