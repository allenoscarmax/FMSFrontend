using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using FMSFrontend.Models;

namespace FMSFrontend.Entities
{
    public static class Singleton
    {
        public static GlobalProperties GlobalProperties { get; } = new();

       public static FrontendConfigs FrontendConfigs { get; set; } = new();

        // 若未來有設備模組、Web 設定等，可繼續加
    }
    public class FrontendConfigs
    {
        public string Theme { get; set; }
        public string Language { get; set; }
        public string Version { get; set; }
    }
}
