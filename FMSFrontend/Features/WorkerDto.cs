using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FMSFrontend.Features
{
    public class WorkerDto
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }                 // 對應 MongoDB 的 _id

        [JsonPropertyName("workernumber")]
        public string WorkerNumber { get; set; }       // 員工編號

        [JsonPropertyName("workername")]
        public string WorkerName { get; set; }         // 員工名稱

        [JsonPropertyName("accountgroup")]
        public string AccountGroup { get; set; }       // 帳號群組

        [JsonPropertyName("accountname")]
        public string AccountName { get; set; }        // 登入帳號

        [JsonPropertyName("password")]
        public string Password { get; set; }           // 密碼
    }
}
