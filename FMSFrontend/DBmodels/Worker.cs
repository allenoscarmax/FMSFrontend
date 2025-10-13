using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class Worker
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string WorkerNumber {  get; set; }
        public string WorkerName { get; set; }
        public string AccountGroup {  get; set; }
        public string AccountName { get; set; }
        public string Password {  get; set; }

        
    }
}
