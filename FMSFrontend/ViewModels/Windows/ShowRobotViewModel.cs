using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Models;
using FMSFrontend.Services;
using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FMSFrontend.ViewModels.Windows
{
    public partial class ShowRobotViewModel : ObservableObject
    {
        [ObservableProperty] private RobotDisplayData displayData = new();
        IRobotService   _robotService;
        public ShowRobotViewModel(IHttpService httpService, Robot robot)
        {
            _robotService = new RobotService(httpService);

            DisplayData.RobotName = robot.Name;
            DisplayData.EquipmentType = robot.EquipmentType;
            DisplayData.EquipmentModel = robot.EquipmentModel;
            DisplayData.CurrentProgram = robot.CurrentProgram;
            DisplayData.MachineState = robot.Status;
            DisplayData.CurrentMaterial = robot.MaterialShortName;
        }

        [RelayCommand]
        private async Task MoveOut()
        {
            // TODO: 呼叫你的 MCC / TAS 邏輯
            try
            {
                List<RobotDto> dtos = await _robotService.DB_GetAllRobotsAsync() ?? new();
                dtos[0].onDeckObjSerial = "";
                bool ok = await _robotService.DB_UpdateRobotDataAsync(dtos[0]);
            }
            catch { }
        }

        [RelayCommand]
        private void CloseWindow(object? window)
        {
            if (window is System.Windows.Window w) w.Close();
        }
    }

    public class RobotDisplayData
    {
        public string RobotName { get; set; } = "";
        public string EquipmentName { get; set; } = "";//no
        public string EquipmentType { get; set; } = "";
        public string EquipmentModel { get; set; } = "";
        public string CurrentProgram { get; set; } = "";
        public string MachineState { get; set; } = "";
        public string CurrentMaterial { get; set; } = "";//No
    }
}
