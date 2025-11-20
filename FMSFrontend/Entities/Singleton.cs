using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using FMSFrontend.Models;

namespace FMSFrontend.Entities
{
    public class FrontendConfigs
    {
        public string Theme { get; set; } = "";
        public string Language { get; set; } = "";
        public string Version { get; set; } = "";
    }
}
