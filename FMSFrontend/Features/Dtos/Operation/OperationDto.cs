using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;

namespace FMSFrontend.Features.Dtos
{
    public class OperationDto
    {
        public DateTime? Time { get;set; }
        public string Operation { get;set; } = "";
        public string worksheetDone { get; set; } = "";

    }
}
