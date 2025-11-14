using CommunityToolkit.Mvvm.ComponentModel;         // ✅ ObservableObject / ObservableProperty
using FMSFrontend.Features.Dtos;                      // ✅ AsrsParameterDto
using FMSFrontend.Features.Mappings;                 // ✅ ApplyTo 擴充方法
using FMSFrontend.Features.Services; // needed for FirstOrDefault
using FMSFrontend.Models;
using FMSFrontend.ViewModels.Factory;
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
        public void ApplyMachinesDto(List<MachinesDto> data)
        {
            var disp = Application.Current?.Dispatcher;
            void apply()
            {
                Cnt++;
                for (int i = 0; i < data.Count; i++)
                {
                    if (Machines.Count == i) Machines.Add(new MachineModel());
                    Machines[i].Type = data[i].machineCode;
                    Machines[i].MachineName = data[i].machineName;
                    Machines[i].Status = data[i].status;
                    Machines[i].Restriction = false;
                    Machines[i].onDeckElectrodeSerial = data[i].onDeckElectrodeSerial;
                    Machines[i].onDeckWorkpieceSerial = data[i].onDeckWorkpieceSerial;
                    Machines[i].onDeckWorksheetSerial = data[i].onDeckWorksheetSerial;
                }
                while (Machines.Count > data.Count)
                {
                    Machines.RemoveAt(Machines.Count - 1);
                }
            }
            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }
    }
}
