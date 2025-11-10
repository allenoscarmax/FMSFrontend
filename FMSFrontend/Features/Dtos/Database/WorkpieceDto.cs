using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace FMSFrontend.Features.Dtos
{
    public class WorkpieceDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = "";

        public string tagSerial { get; set; } = "";
        public string worksheetNumber { get; set; } = "";
        public string workpieceName { get; set; } = "";
        public string status { get; set; } = "";
        public bool restriction { get; set; }
        public string pairedEDM { get; set; } = "";
        public string currentLocation { get; set; } = "";
        public string tempRetSLocation { get; set; } = ""; // WR:1:1:1:1
        public bool isCompleted { get; set; }
        public string edmpgm { get; set; } = "";          // ← 修正自 eDMPGM
        public bool? needInspect { get; set; }
        public bool? inspected { get; set; }
        public string inspectStatus { get; set; } = "";
        public string inspectOffset { get; set; } = "";
        public string setupUser { get; set; } = "";
    }
}
