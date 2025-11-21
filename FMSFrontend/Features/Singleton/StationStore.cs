using CommunityToolkit.Mvvm.ComponentModel;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Dtos.Database;
using FMSFrontend.Features.Mappings;
using FMSFrontend.Features.Services;
using FMSFrontend.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace FMSFrontend.Features.Singleton
{
    public partial class StationStore : ObservableObject
    {
        [ObservableProperty] public StationModel station = new();
        public void ApplyStationDto(AssemblyStationParaDto dto)
        {

            var disp = Application.Current?.Dispatcher;
            void apply()
            {
                var model =new StationModel();
                dto.ApplyStationDto(model);
                Station = model;
            }
            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }
    }
}
