using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.ViewModels.Windows
{
    public partial class UploadSheetViewModel : ObservableObject
    {
        

        [ObservableProperty]
        private string selectedWorkCsvPath = "尚未選擇檔案";

        [ObservableProperty]
        private string selectedEleCsvPath = "尚未選擇檔案";

        [ObservableProperty]
        private bool showElectrodeSection;

        [ObservableProperty]
        private bool showWorkpieceSection;

        [RelayCommand]
        private void SelectCsvFile(string type)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "CSV 檔案 (*.csv)|*.csv",
                Title = "選擇檔案",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                if (type == "Work")
                    SelectedWorkCsvPath = dialog.FileName;
                else if (type == "Ele")
                    SelectedEleCsvPath = dialog.FileName;
            }
        }


    }
}
