using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace FMSFrontend.Models
{
    public partial class CommandScheduleGroupModel : ObservableObject
    {
        [ObservableProperty] public ObservableCollection<CommandScheduleModel> commandSchedules = new();
    }
    public partial class CommandScheduleModel : ObservableObject
    {
        [ObservableProperty] private int priority;    //<--
        [ObservableProperty] private int commandType = 17; //<--
        [ObservableProperty] public double progressPercent;  // 0–100  先填100%
        public string CommandString => ((Enum_QueueCommandType)CommandType).ToString();
        partial void OnCommandTypeChanged(int value) => OnPropertyChanged(nameof(CommandString));
        [ObservableProperty] private string startPoint = ""; //<--
        [ObservableProperty] private string endPoint = ""; //<--
        [ObservableProperty] private string tagSerial = "";
        [ObservableProperty] private string insertTimeString = ""; //<--
        [ObservableProperty] private string taskSource = "";//EDM/CMM/CNC/... //<--
    }
    public enum Enum_QueueCommandType
    {
        InstallElectrodeToEDM, 
        UnInstallElectrodeFromEDM, 
        InstallElectrodeToCMM, 
        UnInstallElectrodeFromCMM,
        InstallWorkpieceToEDM, 

        UnInstallWorkpieceFromEDM, 
        TurnOverProcessing,
        InstallWorkpieceToCMM,
        UnInstallWorkpieceFromCMM,
        InstallWorkpieceToCleaningStation,

        UnInstallWorkpieceFromCleaningStation,
        PutWorkpieceToASE,
        TakeWorkpieceFromASE,
        MoveEleBetweenStorage,
        MovePartBetweenStorage,

        ChipRemoval,
    }
}



















