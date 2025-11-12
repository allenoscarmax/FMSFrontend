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
        public string tagSerial { get; set; } = "";
        public string probeName { get; set; } = "";
        public string probeType { get; set; } = "";
        public string currentLocation { get; set; } = "";
        public string state { get; set; } = "";
        public bool? restriction { get; set; }
        public string pairedEDM { get; set; } = "";

    }
}
