using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Dtos
{
    public class CommandStructDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("hasDone")]
        public bool HasDone { get; set; } //

        [JsonPropertyName("priority")]
        public int Priority { get; set; }   //<--

        // 後端是 Enum_QueueCommandType，前端先用 int 承接
        [JsonPropertyName("commandType")]
        public int CommandType { get; set; } //<--

        [JsonPropertyName("startPoint")]
        public string StartPoint { get; set; } = ""; //<--

        [JsonPropertyName("endPoint")]
        public string EndPoint { get; set; } = ""; //<--

        [JsonPropertyName("tagSerial")]
        public string TagSerial { get; set; } = "";

        [JsonPropertyName("insertTimeString")]
        public string InsertTimeString { get; set; } = ""; //<--

        public string TaskSource { get; set; } = "";//EDM/CMM/CNC/... //<--
    }
    public enum Enum_QueueCommandType
    {
        InstallElectrodeToEDM, //
        UnInstallElectrodeFromEDM, //
        InstallElectrodeToCMM, //
        UnInstallElectrodeFromCMM,//

        InstallWorkpieceToEDM, //
        UnInstallWorkpieceFromEDM, //
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
