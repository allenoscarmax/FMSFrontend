using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Dtos
{
    public class WorksheetIncludeTimelineDto
    {
        // ===== 基本資料（從 Worksheets 衍生的主要屬性） =====

        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("worksheetnumber")]
        public string WorkSheetNumber { get; set; }

        [JsonPropertyName("workpiecename")]
        public string WorkpieceName { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("targetedm")]
        public string TargetEDM { get; set; }

        [JsonPropertyName("createdtime")]
        public DateTime? CreatedTime { get; set; }

        // ===== Timeline 區 =====

        [JsonPropertyName("setuptime")]
        public DateTime? SetupTime { get; set; }

        [JsonPropertyName("dispatchtime")]
        public DateTime? DispatchTime { get; set; }

        [JsonPropertyName("edmstarttime")]
        public List<DateTime?> EDMStartTime { get; set; } = new();

        [JsonPropertyName("edmendtime")]
        public List<DateTime?> EDMEndTime { get; set; } = new();

        [JsonPropertyName("cleaningstarttime")]
        public List<DateTime?> CleaningStartTime { get; set; } = new();

        [JsonPropertyName("cleaningendtime")]
        public List<DateTime?> CleaningEndTime { get; set; } = new();

        [JsonPropertyName("measuringstarttime")]
        public List<DateTime?> MeasuringStartTime { get; set; } = new();

        [JsonPropertyName("measuringendtime")]
        public List<DateTime?> MeasuringEndTime { get; set; } = new();

        [JsonPropertyName("completedtime")]
        public DateTime? CompletedTime { get; set; }

        [JsonPropertyName("failedtime")]
        public DateTime? FailedTime { get; set; }
    }
}
