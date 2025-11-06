using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace FMSFrontend.Features.Dtos
{
    public class WorksheetsDto
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = "";
        public string worksheetNumber { get; set; } = "";
        public string workpieceName { get; set; } = "";
        public int? workPriority { get; set; }
        public bool? workEnabled { get; set; }
        public string workStatus { get; set; } = "";
        public string workPercentage { get; set; } = "";
        public string targetEDM { get; set; } = "";
        public int? processStep { get; set; }
        public int? totalProcessStep { get; set; }
        public string coordinate { get; set; } = "";
        public string setupUser { get; set; } = "";
    }
}