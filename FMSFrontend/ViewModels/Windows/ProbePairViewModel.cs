using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Extensions;
using FMSFrontend.Interfaces;
using FMSFrontend.Services;
using OSCARMAXFMS_V3.DBmodels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FMSFrontend.ViewModels.Windows
{
    public partial class ProbePairViewModel : ObservableObject
    {
        private readonly Window _window;
        private readonly IWindowService _windowService;
        private readonly IHttpService _httpService;
        public ProbePairViewModel(Window window, IWindowService windowService, IHttpService httpService)
        {
            _window = window;
            _windowService = windowService;
            _httpService = httpService;


        }

        // Pair
        [RelayCommand]
        private async Task Pair()
        {
            try
            {
                _windowService.ShowMessage("OK");
            }
            catch (Exception ex)
            {
                _windowService.ShowMessage("配對處理發生錯誤: " + ex.Message);
            }
        }

        [RelayCommand]
        private void Back()
        {
            _window?.Close();

            //  var selectWindow = new MaterialTypeSelectWindow();
            if (_windowService.ShowMaterialTypeSelectWindow(out MaterialKind kind))
            {
                Window? window = kind switch
                {
                    MaterialKind.Electrode => new MaterialPairWindow(isElectrode: true),
                    MaterialKind.Workpiece => new MaterialPairWindow(isElectrode: false),
                    MaterialKind.Probe => new FMSFrontend.Views.Windows.ProbePairWindow(),   // 新增的探針視窗
                    _ => null
                };

                window?.ShowDialog();
            }
        }
    }
}
