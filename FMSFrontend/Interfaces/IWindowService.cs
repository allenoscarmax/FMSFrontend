using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Interfaces
{
    public interface IWindowService
    {
        void ShowUploadSheetWindow();

        void ShowMessage(string message);

        bool ShowYesNoDialog(string message);

        bool ShowMaterialTypeSelectWindow(out bool Isele);
        void ShowMaterialPairWindow(bool isElectrode); 
    }
}
