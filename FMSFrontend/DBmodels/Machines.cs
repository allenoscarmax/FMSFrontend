using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class Machines
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string MachineCode { get; set; }
        public int ProductionLine {  get; set; }
        public int MachineNumber { get; set; }
        public string MachineName { get; set; }
        public string IP { get; set; }
        public string Port { get; set; }
        public string RemotePassword { get; set; }
        public string RemotePath {  get; set; }
        public string Status { get; set; }
        public string OnDeckElectrodeSerial { get; set; }
        public string OnDeckWorkpieceSerial { get; set; }
        public string OnDeckWorksheetSerial { get; set; }
        public string SetupUser { get; set; }
    }
}
