using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class Worksheets
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string WorksheetNumber { get; set; } 
        public string WorkpieceName { get; set; }
        public int? WorkPriority { get; set; }
        public bool? WorkEnabled { get; set; }
        public string WorkStatus { get; set; }
        public string WorkPercentage { get; set; }
        public string TargetEDM { get; set; }
        public int? ProcessStep { get; set; }
        public int? TotalProcessStep { get; set; }
        public string Coordinate { get; set; }
        public string SetupUser { get; set; }

    }
}
