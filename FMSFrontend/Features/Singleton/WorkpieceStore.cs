using CommunityToolkit.Mvvm.ComponentModel;         // ✅ ObservableObject / ObservableProperty
using FMSFrontend.Features.Dtos;                      // ✅ AsrsParameterDto
using FMSFrontend.Features.Mappings;                 // ✅ ApplyTo 擴充方法
using FMSFrontend.Features.Services; // needed for FirstOrDefault
using FMSFrontend.Models;
using FMSFrontend.ViewModels.Factory;
using OSCARMAXFMS_V3.DBmodels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;                               // ✅ ProductionLinesPage
using System.Windows;                              
using System.Windows.Threading;

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
                    dto.ApplyWorkpieceDto(workpiece);
            }
            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }

    }
}
