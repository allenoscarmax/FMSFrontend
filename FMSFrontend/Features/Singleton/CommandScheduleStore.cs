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
                /* 
                 // 假資料測試用
                for (int i =0;i<5;i++)
                {
                    var model = new CommandScheduleModel()
                    {
                        // 假資料：可視化測試
                        Priority = i,
                        CommandType = i,                    // 保持預設或依需要調整
                        ProgressPercent = i*10,               // 0–100 先填 100%
                        StartPoint = $"S{i + 1:00}",
                        EndPoint = $"E{i + 1:00}",
                        InsertTimeString = DateTime.Now.AddMinutes(-i * 5).ToString("yyyy/MM/dd HH:mm:ss"),
                        TaskSource = "假資料"
                    };
                    models.Add(model);
                }
                */
                CommandSchedules.CommandSchedules = models;
            }
            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }
    }
}
