using System.Collections.Generic;
using System.Windows;                              
using CommunityToolkit.Mvvm.ComponentModel;         // ✅ ObservableObject / ObservableProperty
using FMSFrontend.Features.Mappings;                 // ✅ ApplyTo 擴充方法
using FMSFrontend.Features.Dtos;                      // ✅ AsrsParameterDto
using FMSFrontend.Models;                               // ✅ RFIDBindPage

namespace FMSFrontend.Features.Singleton
{
    public partial class RFIDBindStore : ObservableObject
    {
        [ObservableProperty] private RFIDBindData rfidBind = new();
        public void ApplyRFIDBindPageDto(RFIDWriteLogDto dto)
        {
            var disp = Application.Current?.Dispatcher;
            void apply() => new List<RFIDWriteLogDto> { dto }.ApplyRFIDBindPageDto(RfidBind);

            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }
    }
}
