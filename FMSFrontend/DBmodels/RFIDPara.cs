using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSCARMAXFMS_V3.Models
{
    public class RFIDParas
    {
        public bool[] RFID_Is_Present { get; set; } = new bool[4] { false, false, false, false };
        public bool[] RFID_Is_connect { get; set; } = new bool[4] { false, false, false, false };
    }
}
