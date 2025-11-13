using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Dtos
{
    public class AsrsParameterDto
    {
        public bool isRobotConnected { get; set; }
        public bool isRobotBusy { get; set; }
        public bool isRobotError { get; set; }
        public bool isRobotBePaused { get; set; }
        public bool isASRSControlStop { get; set; }
        public bool isRobotWorkingOnScaning { get; set; }

        public string? robotPosition { get; set; }
        public int robotStatus { get; set; }
        public bool battAlarm { get; set; }
        public string? robotAlarmMessage { get; set; }
        public int? robotNumber { get; set; }

        public bool asrsControlStart { get; set; }
        public bool asrsControlPause { get; set; }
        public bool asrsControlStop { get; set; }
        public bool asrsProcessWarning { get; set; }

        public string? robotDoingNow { get; set; }
        public string? robotDoingNext { get; set; }

        public bool robotMaintenanceNotice { get; set; }
        public bool robotInSaftyArea { get; set; }

        public bool dispatchSwitch { get; set; }
        public bool dispatchNeedToStop { get; set; }
        public bool dispatchIsStoped { get; set; }
        public bool robotCantReceiveNewTask { get; set; }
    }
    public enum RobotDoEnum
    {
        [Description("前往機台")]
        MoveToMachine,
        [Description("前往指定電極倉")]
        MoveToEleStorage,
        [Description("前往指定工件倉")]
        MoveToWorkPieceStorage,
        [Description("前往三次元")]
        MoveToCMM,
        [Description("前往清洗機")]
        MoveToCleaningStation,
        [Description("前往組裝站")]
        MoveToASE,

        [Description("卸電極")]
        EleRemoval,
        [Description("等待卸刀訊號")]
        WaitEleUnloadingSignal,
        [Description("放電極回倉")]
        EleReturneStorage,

        [Description("裝電極")]
        EleInstall,
        [Description("等待取刀訊號")]
        WaitEleLoadingSignal,
        [Description("取指定電極")]
        TakeDesignatedEle,

        [Description("卸工件")]
        PartRemoval,
        [Description("等待卸工件訊號")]
        WaitPartUnloadingSignal,
        [Description("放工件回倉")]
        PartReturneStorage,

        WaitLoadingSignal,
        WaitUnloadingSignal,

        [Description("裝工件")]
        PartInstall,
        [Description("等待取刀訊號")]
        WaitPartLoadingSignal,
        [Description("取指定工件")]
        TakeDesignatedPart,

        [Description("手臂移動中")]
        RobotMoving,
        [Description("手臂掃描中")]
        RobotScaning,

    }
    public enum RobotDoErrorEnum
    {
        [Description("前往機台失敗")]
        MoveToMachineFail,
        [Description("前往指定電極倉失敗")]
        MoveToEleStorageFail,
        [Description("前往指定工件倉失敗")]
        MoveToPartStorageFail,
        [Description("前往三次元失敗")]
        MoveToCMMFail,
        [Description("前往清洗機失敗")]
        MoveToCleaningStationFail,

        MoveToASEFail,

        [Description("卸電極失敗")]
        EleRemovalFail,
        [Description("等待卸刀訊號失敗")]
        WaitEleUnloadingSignalFail,
        [Description("放電極回倉失敗")]
        EleReturneStorageFail,

        [Description("裝電極失敗")]
        EleInstallFail,
        [Description("取指定電極失敗")]
        TakeDesignatedEleFail,
        [Description("等待取刀訊號失敗")]
        WaitEleLoadingSignalFail,

        [Description("卸工件失敗")]
        PartRemovalFail,
        [Description("等待卸工件訊號失敗")]
        WaitPartUnloadingSignalFail,
        [Description("放工件回倉失敗")]
        PartReturneStorageFail,

        [Description("裝工件失敗")]
        PartInstallFail,
        [Description("等待取刀訊號失敗")]
        WaitPartLoadingSignalFail,
        [Description("取指定工件失敗")]
        TakeDesignatedPartFail,
        WaitLoadingSignalFail,
        WaitUnloadingSignalFail,

        [Description("手臂移動失敗")]
        RobotMovingFail,
        [Description("手臂移動失敗")]
        RobotScaningFail,
    }
    public enum Pre_Type
    {
        ElectrodeMagazine = 1,
        WorkpieceMagazine = 2,
        EDM = 3,
        WashingStation = 4,
        CMM = 5,
        AssemblyStation = 6,
        ElectrodeExchageMagazine = 7,
        WorkpieceExchageMagazine = 8,
        ElectrodeBackpack = 9,
        WorkpieceBackpack = 10,
    }
}
