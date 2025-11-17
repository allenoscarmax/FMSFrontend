using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Models;
namespace FMSFrontend.ViewModels.Windows
{
    public partial class ShowRobotViewModel : ObservableObject
    {
        [ObservableProperty] private RobotDisplayData displayData = new();

        public ShowRobotViewModel(Robot robot)
        {
            DisplayData.RobotName = robot.Name;
            DisplayData.EquipmentType = robot.EquipmentType;
            DisplayData.EquipmentModel = robot.EquipmentModel;
            DisplayData.CurrentProgram = robot.CurrentProgram;
            DisplayData.MachineState = robot.Status;
            DisplayData.CurrentMaterial = robot.MaterialShortName;
        }

        [RelayCommand]
        private void MoveOut()
        {
            // TODO: 呼叫你的 MCC / TAS 邏輯
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
