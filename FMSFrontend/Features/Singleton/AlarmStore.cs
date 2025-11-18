using CommunityToolkit.Mvvm.ComponentModel;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Dtos.Database;
using FMSFrontend.Features.Mappings;
using FMSFrontend.Features.Services;
using FMSFrontend.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace FMSFrontend.Features.Singleton
{
    public partial class AlarmStore : ObservableObject
    {
        [ObservableProperty] public AlarmGroupModel alarmGroup = new();
        public void ApplyErrorMessageLogDto(List<ErrorMessageLogDto>? dtos)
        {

            var disp = Application.Current?.Dispatcher;
            void apply()
            {
                AlarmGroup.IsAlarm = (dtos != null);
                if (AlarmGroup.IsAlarmWindowsOpen)
                {
                    if (dtos == null) dtos = new List<ErrorMessageLogDto>();
                    var models = new ObservableCollection<AlarmModel>();
                    foreach (var dto in dtos)
                    {
                        var model = new AlarmModel();
                        dto.ApplyErrorMessageLogDto(model);
                        models.Add(model);
                    }
                    AlarmGroup.AlarmModels = models;
                }
            }
            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }
    }
}
