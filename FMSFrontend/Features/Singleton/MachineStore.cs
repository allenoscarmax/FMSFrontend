using CommunityToolkit.Mvvm.ComponentModel;         
using FMSFrontend.Features.Dtos;                      
using FMSFrontend.Features.Mappings;              
using FMSFrontend.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;                              

namespace FMSFrontend.Features.Singleton
{
    public partial class MachineStore : ObservableObject
    {
        [ObservableProperty] public ObservableCollection<MachineModel> machines = new();

        public void ApplyMachinesDto(List<MachinesDto> dtos)
        {
            var disp = Application.Current?.Dispatcher;
            void apply()
            {
                for (int i = 0; i < dtos.Count; i++)
                {
                    //if (Machines.Count == i) Machines.Add(new MachineModel());
                    if (!Machines.Any(m => string.Equals(m.MachineId, dtos[i].machineCode, StringComparison.OrdinalIgnoreCase)))
                    {
                        Machines.Add(new MachineModel());
                    }
                    dtos[i].ApplyMachinesDto(Machines[i]);
                }
            }
            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }
        public void ApplyNull(int inedx,bool isElectrode)
        {
            var disp = Application.Current?.Dispatcher;
            void apply()
            {
                if (isElectrode)
                {
                    Machines[inedx].ElectrodeName = "";
                    Machines[inedx].ElectrodeShortName = "";
                }
                else
                {
                    Machines[inedx].WorkpieceName = "";
                    Machines[inedx].WorkpieceShortName = "";
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
        public void ApplyProbeDto(ProbeDto dto, int inedx)
        {
            var disp = Application.Current?.Dispatcher;
            void apply()
            {
                dto.ApplyProbeDto(Machines[inedx]);
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

        public void ApplyFanucCNCParaDto(FanucCNCDto dto)
        {
            var disp = Application.Current?.Dispatcher;
            void apply()
            {
                var model = Machines.FirstOrDefault(m =>
                    (!string.IsNullOrEmpty(m.MachineId) && m.MachineId.Contains("FanucCNC", StringComparison.OrdinalIgnoreCase)));

                if (model != null)
                    dto.ApplyFanucCNCParaDto(model);
            }
            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }

        public void ApplySiemensCNCParaDto(SiemensCNCDto dto)
        {
            var disp = Application.Current?.Dispatcher;
            void apply()
            {
                var model = Machines.FirstOrDefault(m =>
                    (!string.IsNullOrEmpty(m.MachineId) && m.MachineId.Contains("SiemensCNC", StringComparison.OrdinalIgnoreCase)));

                if (model != null)
                    dto.ApplySiemensCNCParaDto(model);
            }
            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }
    }
}
