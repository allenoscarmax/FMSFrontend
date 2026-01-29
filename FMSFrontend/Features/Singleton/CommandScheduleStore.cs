using CommunityToolkit.Mvvm.ComponentModel;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Dtos.Database;
using FMSFrontend.Features.Mappings;
using FMSFrontend.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.Marshalling;
using System.Windows;

namespace FMSFrontend.Features.Singleton
{
    public partial class CommandScheduleStore : ObservableObject
    {
        [ObservableProperty] public CommandScheduleGroupModel commandSchedules = new();
        public void ApplyCommandScheduleDto(List<CommandStructDto> dtos)
        {
            var disp = Application.Current?.Dispatcher;
            void apply()
            {
                if (dtos == null) dtos = new List<CommandStructDto>();
                var models = new ObservableCollection<CommandScheduleModel>();
                foreach (var dto in dtos)
                {
                    var model = new CommandScheduleModel();
                    dto.ApplyCommandScheduleDto(model);
                    models.Add(model);
                }
                CommandSchedules.CommandSchedules = models;
            }
            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }
    }
}
