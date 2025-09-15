using FMSFrontend.ViewModels.Production;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Interfaces
{
    public interface IHasMaterial
    {
        MaterialRef Material { get; }
    }
}
