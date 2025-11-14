using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Dtos
{
    public class AppointmentMaintenanceDto
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("maintenance")]
        public string Maintenance { get; set; } = "";// 保養設備或名稱

        [JsonPropertyName("productline")]
        public string ProductLine { get; set; } = "";   // 第 x 條產線

        [JsonPropertyName("index")]
        public int Index { get; set; }            // 第幾台

        [JsonPropertyName("type")]
        public string Type { get; set; } = "";        // 保養類型 (e.g. daily/weekly/monthly)

        [JsonPropertyName("day_values")]
        public List<int> DayValues { get; set; } = new List<int>();// 例如 [1,3,5] 表示週日、週二、週四

        [JsonPropertyName("hour")]
        public int Hour { get; set; }             // 幾點 (24 小時制)

        [JsonPropertyName("minute")]
        public int Minute { get; set; }           // 幾分

        [JsonPropertyName("isenabled")]
        public bool IsEnabled { get; set; } = true; // 是否啟用

        [JsonPropertyName("note")]
        public string Note { get; set; } = "";         // 備註說明
    }
}
