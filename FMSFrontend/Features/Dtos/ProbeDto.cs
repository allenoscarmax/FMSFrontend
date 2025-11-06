using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Dtos
{
    public class ProbeDto
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = "";
        public string TagSerial { get; set; } = "";
        public string ProbeName { get; set; } = "";
        public string ProbeType { get; set; } = "";
        public string CurrentLocation { get; set; } = "";
        public string State { get; set; } = "";
        public bool? Restriction { get; set; }
        public string PairedEDM { get; set; } = "";

    }
}
