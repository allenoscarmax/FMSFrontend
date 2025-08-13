using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ControlzEx.Standard;
using FMSFrontend.Extensions;
using FMSFrontend.Interfaces;
using FMSFrontend.ViewModels.Windows;

namespace FMSFrontend.Services
{
    public class WindowService : IWindowService
    {
        public void ShowUploadSheetWindow()
        {
            var window = new UploadsheetsWindow();
            window.ShowDialog();
        }

        public void ShowMessage(string message)
        {
            var dialog = new DialogMessageWindow(message);
            dialog.ShowDialog();
        }

        public bool ShowYesNoDialog( string message)
        {
            var dialog = new DialogYesNoWindow(message);
            return dialog.ShowDialog() == true;
        }
        public bool ShowMaterialTypeSelectWindow(out bool isElectrode)
        {
            var window = new MaterialTypeSelectWindow();
            bool? retu = window.ShowDialog();
            isElectrode = window.IsElectrodeSelected;
            return retu == true;
        }
        public void ShowMaterialPairWindow(bool isElectrode)
        {

            var window = new MaterialPairWindow(isElectrode); // 讓 ViewModel 在 window 裡綁定
            window.ShowDialog();
        }

    }
}
