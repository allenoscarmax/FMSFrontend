using CommunityToolkit.Mvvm.ComponentModel;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Mappings;
using FMSFrontend.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace FMSFrontend.Features.Singleton
{
    public partial class ElectrodeStore : ObservableObject
    {
        [ObservableProperty] public ElectrodeModel electrode = new();

        public void ApplyElectrodeDto(ElectrodeDto dto)
        {
            var disp = Application.Current?.Dispatcher;
            void apply()
            {
                dto.ApplyElectrodeDto(Electrode);
            }
            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }
    }
}
