using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Models
{
    public class GlobalProperties
    {
        public string _SplashScreenMessage = "None";  //None
        public string SplashScreenMessage { get { return _SplashScreenMessage; } set { _SplashScreenMessage = value; } }
    }
}
