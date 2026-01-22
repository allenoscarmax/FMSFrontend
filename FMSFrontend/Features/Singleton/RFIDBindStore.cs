using CommunityToolkit.Mvvm.ComponentModel;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Mappings;
using FMSFrontend.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Converters;
namespace FMSFrontend.Features.Singleton
{
    public partial class RFIDBindStore : ObservableObject
    {
        [ObservableProperty] public RFIDBindModel rfidBind = new();
        public void ApplyRFIDBindPageDto(List<RFIDWriteLogDto> dto)
        {
            var disp = Application.Current?.Dispatcher; //這樣才能在非UI執行緒更新UI綁定的屬性
            void apply() => dto.ApplyRFIDBindPageDto(RfidBind); //利用擴充方法進行映射

            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply); //如果不是UI執行緒就用Dispatcher.Invoke切換到UI執行緒
            else apply();
        }
        public void ApplyParasDto(RFIDParasDto dto, int TagNumber)
        {
            var disp = Application.Current?.Dispatcher; //這樣才能在非UI執行緒更新UI綁定的屬性
            void apply() => dto.ApplyParasDto(RfidBind, TagNumber); //利用擴充方法進行映射

            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply); //如果不是UI執行緒就用Dispatcher.Invoke切換到UI執行緒
            else apply();
        }
        public void ApplyTagDto(string? dto)
        {
            var disp = Application.Current?.Dispatcher; //這樣才能在非UI執行緒更新UI綁定的屬性
            void apply() => dto.ApplyTagDto(RfidBind); //利用擴充方法進行映射

            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply); //如果不是UI執行緒就用Dispatcher.Invoke切換到UI執行緒
            else apply();
        }

        public void ApplyEleDto(ElectrodeDto dto)
        {
            var disp = Application.Current?.Dispatcher; //這樣才能在非UI執行緒更新UI綁定的屬性

            void apply() => dto.ApplyEleDto(RfidBind); //利用擴充方法進行映射

            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply); //如果不是UI執行緒就用Dispatcher.Invoke切換到UI執行緒
            else apply();
        }

        public void ApplyWpDto(WorkpieceDto dto)
        {
            var disp = Application.Current?.Dispatcher; //這樣才能在非UI執行緒更新UI綁定的屬性
            void apply() => dto.ApplyWpDto(RfidBind); //利用擴充方法進行映射

            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply); //如果不是UI執行緒就用Dispatcher.Invoke切換到UI執行緒
            else apply();
        }

    }
}
