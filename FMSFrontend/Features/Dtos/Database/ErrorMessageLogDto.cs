using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Dtos.Database
{
    public class ErrorMessageLogDto
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("timeStamp")]
        public DateTime TimeStamp { get; set; } //Time

        [JsonPropertyName("errorCode")]
        public string ErrorCode { get; set; } = ""; //Code

        [JsonPropertyName("message_cn")]
        public string MessageCn { get; set; } = ""; //mes...

        [JsonPropertyName("message_en")]
        public string MessageEn { get; set; } = "";

        [JsonPropertyName("note")]
        public string Note { get; set; } = "";

        [JsonPropertyName("whichLine")]
        public string WhichLine { get; set; } = "";

        [JsonPropertyName("isAverted")]
        public bool IsAverted { get; set; }
    }
}
