using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Dtos
{
    public class RFIDParasDto
    {
        public bool[] rFID_Is_Present { get; set; } = new bool[4] { false, false, false, false };
        public bool[] rFID_Is_connect { get; set; } = new bool[4] { false, false, false, false };
    }
}
