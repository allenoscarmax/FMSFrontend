using CommunityToolkit.Mvvm.ComponentModel;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Mappings;
using FMSFrontend.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

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
