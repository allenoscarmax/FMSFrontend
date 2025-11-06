using CommunityToolkit.Mvvm.ComponentModel;         // ✅ ObservableObject / ObservableProperty
using FMSFrontend.Features.Dtos;                      // ✅ AsrsParameterDto
using FMSFrontend.Features.Mappings;                 // ✅ ApplyTo 擴充方法
using FMSFrontend.Models;                               // ✅ Robot
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
