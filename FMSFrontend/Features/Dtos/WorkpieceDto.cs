using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace FMSFrontend.Features.Dtos
{
    public class WorkpieceDto
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = "";
        public string TagSerial { get; set; } = "";
        public string WorksheetNumber {  get; set; } = "";
        public string WorkpieceName { get; set; } = "";
        public string Status {  get; set; } = "";
        public bool Restriction { set; get; }
        public string PairedEDM { get; set; } = "";
        public string CurrentLocation {  get; set; } = "";
        public string TempRetSLocation { get; set; } = ""; //WR:1:1:1:1
        public bool IsCompleted {  set; get; }
        public string EDMPGM { get; set; } = "";
        public bool? NeedInspect { get; set; }
        public bool? Inspected { get; set; }
        public string InspectStatus { get; set; } = "";
        public string InspectOffset {  get; set; } = "";
        public string SetupUser {  get; set; } = "";
    }
}
