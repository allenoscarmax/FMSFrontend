using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FMSFrontend.Features.Dtos
{
    public class AllCmmsDto
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = "";

        public int productionLine { get; set; }
        public string cmmNumber { get; set; } = "";
        public string cmmName { get; set; } = "";
        public string cmmip { get; set; } = "";
        public string cmmUser { get; set; } = "";
        public string cmmPassword { get; set; } = "";
        public string remotePath { get; set; } = "";
        public string status { get; set; } = "";
        public string? onDeckObjSerial { get; set; }
        public string setupUser { get; set; } = "";
    }
}
