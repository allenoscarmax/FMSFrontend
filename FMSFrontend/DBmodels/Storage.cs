
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class Storage
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string StorageName{ get; set; }
        public string StorageNumber { get; set; }
        public int Region {  get; set; }
        public int Column { get; set; }
        public int Row { get; set; }
        public string OndeskTagserial {  get; set; }
        public string State { get; set; }
        public bool? Restriction { get; set; }
        public string Note {  get; set; }
    }
}
