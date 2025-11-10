using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Dtos
{
    public class DevicesDto
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("devicenumber")]
        public string DeviceNumber { get; set; }

        [JsonPropertyName("devicename")]
        public string DeviceName { get; set; }

        [JsonPropertyName("deviceip")]
        public string DeviceIP { get; set; }

        [JsonPropertyName("deviceport")]
        public string DevicePort { get; set; }

        [JsonPropertyName("productionline")]
        public int ProductionLine { get; set; }

        [JsonPropertyName("setupuser")]
        public string SetupUser { get; set; }
    }
}
