using CommunityToolkit.Mvvm.ComponentModel;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Mappings;
using FMSFrontend.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace FMSFrontend.Features.Singleton
{
    public partial class PlcStore : ObservableObject
    {
        [ObservableProperty] private MagazinePara magazinePara = new();

        public void ApplyMagazineParaDto(MagazineParaDto dto)
        {
            var disp = Application.Current?.Dispatcher;
            void apply() => dto.ApplyMagazineParaDto(MagazinePara);

            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }
    }
}
