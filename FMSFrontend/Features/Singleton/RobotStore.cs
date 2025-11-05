using System.Windows;                              
using CommunityToolkit.Mvvm.ComponentModel;         // ✅ ObservableObject / ObservableProperty
using FMSFrontend.Features.Mappings;                 // ✅ ApplyTo 擴充方法
using FMSFrontend.Features.Dtos;                      // ✅ AsrsParameterDto
using FMSFrontend.Models;                               // ✅ Robot

namespace FMSFrontend.Features.Singleton
{
    public partial class RobotStore : ObservableObject
    {
        [ObservableProperty] private Robot robot = new();

        public void ApplyAsrsDto(AsrsParameterDto dto)
        {
            var disp = Application.Current?.Dispatcher;
            void apply() => dto.ApplyAsrsDto(Robot);

            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }
        public void ApplyRobotDto(RobotDto dto)
        {
            var disp = Application.Current?.Dispatcher;
            void apply() => dto.ApplyRobotDto(Robot);

            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }
    }
}
