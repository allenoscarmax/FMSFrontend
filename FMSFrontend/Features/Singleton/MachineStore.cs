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
    public partial class MachineStore : ObservableObject
    {
        [ObservableProperty] public ObservableCollection<MachineModel> machines = new();
        int Cnt = 0;
        public void ApplyMachinesDto(List<MachinesDto> dtos)
        {
            var disp = Application.Current?.Dispatcher;
            void apply()
            {
                for (int i = 0; i < dtos.Count; i++)
                {
                    if (Machines.Count == i) Machines.Add(new MachineModel());
                    dtos[i].ApplyMachinesDto(Machines[i]);
                }
                while (Machines.Count > dtos.Count)
                {
                    Machines.RemoveAt(Machines.Count - 1);
                }
            }
            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }
        public void ApplyElectrodeDto(ElectrodeDto dto,int inedx)
        {
            var disp = Application.Current?.Dispatcher;
            void apply()
            {
                    dto.ApplyElectrodeDto(Machines[inedx]);
            }
            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }
        public void ApplyWorkpieceDto(WorkpieceDto dto, int inedx)
        {
            var disp = Application.Current?.Dispatcher;
            void apply()
            {
                dto.ApplyWorkpieceDto(Machines[inedx]);
            }
            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }
        public void ApplyOscarmaxMachineParaDto(OscarmaxMachineParaDto dto, int inedx)
        {
            var disp = Application.Current?.Dispatcher;
            void apply()
            {
                dto.ApplyOscarmaxMachineParaDto(Machines[inedx]);
            }
            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }
    }
}
