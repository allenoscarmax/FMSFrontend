using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class ProcessingPresets
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string ProcessName {  get; set; }
        public string MachineGroupNumber { get; set; }
        public string OperationNumber {  get; set; }
        public string SetupUser {  get; set; }
    }
}
