using System.Text.Json.Serialization;

namespace FMSFrontend.Features.Dtos
{
    public class AMRDto
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("agvId")]
        public string? AgvId { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("alarmsMsg")]
        public string? AlarmsMsg { get; set; }

        [JsonPropertyName("battery")]
        public string? Battery { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("currentRoute")]
        public string? CurrentRoute { get; set; }

        [JsonPropertyName("workingStatus")]
        public string? WorkingStatus { get; set; }
    }
}
