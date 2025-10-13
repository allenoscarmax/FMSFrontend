using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class Devices
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string DeviceNumber { get; set; }
        public string DeviceName { get; set; }
        public string DeviceIP { get; set; }
        public string DevicePort { get; set; }
        public int ProductionLine {  get; set; }
        public string SetupUser { get; set; }
        

    }
}
