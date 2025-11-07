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
        [ObservableProperty] private RFIDBindModel rfidBind = new();
        public void ApplyRFIDBindPageDto(List<RFIDWriteLogDto> dto)
        {
            var disp = Application.Current?.Dispatcher; //這樣才能在非UI執行緒更新UI綁定的屬性
            void apply() => dto.ApplyRFIDBindPageDto(RfidBind); //利用擴充方法進行映射

            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply); //如果不是UI執行緒就用Dispatcher.Invoke切換到UI執行緒
            else apply();
        }
        public void ApplyParasDto(RFIDParasDto dto)
        {
            var disp = Application.Current?.Dispatcher; //這樣才能在非UI執行緒更新UI綁定的屬性
            void apply() => dto.ApplyParasDto(RfidBind); //利用擴充方法進行映射

            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply); //如果不是UI執行緒就用Dispatcher.Invoke切換到UI執行緒
            else apply();
        }
        public void ApplyTagDto(string dto)
        {
            var disp = Application.Current?.Dispatcher; //這樣才能在非UI執行緒更新UI綁定的屬性
            void apply() => dto.ApplyTagDto(RfidBind); //利用擴充方法進行映射

            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply); //如果不是UI執行緒就用Dispatcher.Invoke切換到UI執行緒
            else apply();
        }
    }
}
