using CommunityToolkit.Mvvm.ComponentModel;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Mappings;
using FMSFrontend.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace FMSFrontend.Features.Singleton
{
    public partial class WorkpieceStore : ObservableObject
    {
        [ObservableProperty] public WorkpieceModel workpiece = new();
        public void ApplyWorkpieceDto(WorkpieceDto dto)
        {
            var disp = Application.Current?.Dispatcher;
            void apply()
            {
                    dto.ApplyWorkpieceDto(Workpiece);
            }
            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }

    }
}
