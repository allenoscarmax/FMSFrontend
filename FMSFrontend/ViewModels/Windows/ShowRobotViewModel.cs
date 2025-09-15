using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FMSFrontend.ViewModels.Windows
{
    public partial class ShowRobotViewModel : ObservableObject
    {
        [ObservableProperty] private RobotDisplayData displayData = new();

        public ShowRobotViewModel()
        {
            // demo：初始化
            displayData = new RobotDisplayData
            {
                RobotName = "主線機器人",
                EquipmentName = "Fanuc Robot",
                EquipmentType = "Robot",
                EquipmentModel = "M-20iD/25",
                CurrentProgram = "MAIN.PROG",
                MachineState = "運轉中",
                CurrentMaterial = "WRP20250512"
            };
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
        public string EquipmentName { get; set; } = "";
        public string EquipmentType { get; set; } = "";
        public string EquipmentModel { get; set; } = "";
        public string CurrentProgram { get; set; } = "";
        public string MachineState { get; set; } = "";
        public string CurrentMaterial { get; set; } = "";
    }
}
