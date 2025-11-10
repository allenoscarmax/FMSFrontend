using System.Collections.Generic;
using System.Windows;                              
using CommunityToolkit.Mvvm.ComponentModel;         // ✅ ObservableObject / ObservableProperty
using FMSFrontend.Features.Mappings;                 // ✅ ApplyTo 擴充方法
using FMSFrontend.Features.Dtos;                      // ✅ AsrsParameterDto
using FMSFrontend.Models;                               // ✅ ProductionLinesPage

namespace FMSFrontend.Features.Singleton
{
    public partial class ProductionLinesStore : ObservableObject
    {
        [ObservableProperty] private ProductionLinesModel productionLinesModel = new();
        public void ApplyStorageDto(List<StorageDto> dto)
        {
            var disp = Application.Current?.Dispatcher; //這樣才能在非UI執行緒更新UI綁定的屬性
            void apply() => dto.ApplyStorageDto(ProductionLinesModel); //利用擴充方法進行映射 (使用產生的屬性)

            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply); //如果不是UI執行緒就用Dispatcher.Invoke切換到UI執行緒
            else apply();
        }
        public List<string> ReadSerialList() => ProductionLinesModel.SerialList; // 使用屬性

        public void ApplyElectrodeDto(List<ElectrodeDto> dto)
        {
            var disp = Application.Current?.Dispatcher; //這樣才能在非UI執行緒更新UI綁定的屬性
            void apply() => dto.ApplyElectrodeDto(ProductionLinesModel); //使用屬性

            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply); //如果不是UI執行緒就用Dispatcher.Invoke切換到UI執行緒
            else apply();
        }
        public void ApplyWorkpieceDto(List<WorkpieceDto> dto)
        {
            var disp = Application.Current?.Dispatcher; //這樣才能在非UI執行緒更新UI綁定的屬性
            void apply() => dto.ApplyWorkpieceDto(ProductionLinesModel); //使用屬性

            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply); //如果不是UI執行緒就用Dispatcher.Invoke切換到UI執行緒
            else apply();
        }
        public void ApplyEleTimelineDto(List<EleTimelineDto> dto)
        {
            var disp = Application.Current?.Dispatcher; //這樣才能在非UI執行緒更新UI綁定的屬性
            void apply() => dto.ApplyEleTimelineDto(ProductionLinesModel); //使用屬性

            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply); //如果不是UI執行緒就用Dispatcher.Invoke切換到UI執行緒
            else apply();
        }
        public void ApplyWpTimelineDto(List<WpTimelineDto> dto)
        {
            var disp = Application.Current?.Dispatcher; //這樣才能在非UI執行緒更新UI綁定的屬性
            void apply() => dto.ApplyWpTimelineDto(ProductionLinesModel); //使用屬性

            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply); //如果不是UI執行緒就用Dispatcher.Invoke切換到UI執行緒
            else apply();
        }
        public void ApplyMachinesDto(List<MachinesDto> dto)
        {
            var disp = Application.Current?.Dispatcher; //這樣才能在非UI執行緒更新UI綁定的屬性
            void apply() => dto.ApplyMachinesDto(ProductionLinesModel); //使用屬性

            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply); //如果不是UI執行緒就用Dispatcher.Invoke切換到UI執行緒
            else apply();
        }

    }
}
