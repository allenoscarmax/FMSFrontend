using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace FMSFrontend.Features.Dtos
{
    /*
    public class OperationDto
    {
        public DateTime? Time { get;set; }
        public string Operation { get;set; } = "";
        public string worksheetDone { get; set; } = "";

    }*/
    public class OperationMessageLogDto
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; } = ""; // 對應 MongoDB 的 _id

        [JsonPropertyName("TimeStamp")]
        public DateTime TimeStamp { get; set; } // 後台存 Local 時間

        [JsonPropertyName("Message_cn")]
        public string MessageCn { get; set; } = "";

        [JsonPropertyName("Mesage_en")]
        public string MessageEn { get; set; } = "";

        [JsonPropertyName("Source")]
        public string Source { get; set; } = ""; // UI / API / Scheduler / PLC

        [JsonPropertyName("Action")]
        public string Action { get; set; } = ""; // Login / Logout / Delete / Update

        [JsonPropertyName("SetupUser")]
        public string SetupUser { get; set; } = "";
    }
}
