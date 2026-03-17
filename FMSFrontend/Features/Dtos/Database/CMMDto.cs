using System.Text.Json.Serialization;

namespace FMSFrontend.Features.Dtos
{
    public class CMMDto
    {
        // ---- 氣壓與夾治具狀態 ----
        [JsonPropertyName("airPressureDetection")]
        public bool AirPressureDetection { get; set; }

        [JsonPropertyName("chuck_ele_close")]
        public bool ChuckEleClose { get; set; }

        [JsonPropertyName("chuck_workpiece_close")]
        public bool ChuckWorkpieceClose { get; set; }

        [JsonPropertyName("chuck_ele_onDesk")]
        public bool ChuckEleOnDesk { get; set; }

        [JsonPropertyName("chuck_workpiece_onDesk")]
        public bool ChuckWorkpieceOnDesk { get; set; }

        // ---- 量測結果 ----
        [JsonPropertyName("measureValue")]
        public double? MeasureValue { get; set; }

        [JsonPropertyName("quality_pass")]
        public bool QualityPass { get; set; }

        [JsonPropertyName("finishedMeasure")]
        public bool FinishedMeasure { get; set; }

        // ---- 錯誤與安全訊號 ----
        [JsonPropertyName("fatal_error")]
        public bool FatalError { get; set; }

        [JsonPropertyName("mtM_error")]
        public bool MtMError { get; set; }

        [JsonPropertyName("recovery_error")]
        public bool RecoveryError { get; set; }

        [JsonPropertyName("abS_done")]
        public bool AbsDone { get; set; }

        [JsonPropertyName("axis_home_position")]
        public bool AxisHomePosition { get; set; }

        [JsonPropertyName("main_power_check_signal")]
        public bool MainPowerCheckSignal { get; set; }

        [JsonPropertyName("emergency_output")]
        public bool EmergencyOutput { get; set; }

        // ---- 系統狀態 ----
        [JsonPropertyName("answerStatus")]
        public string? AnswerStatus { get; set; }

        [JsonPropertyName("executionStatus")]
        public string ExecutionStatus { get; set; } = "";

        [JsonPropertyName("functionalMode")]
        public string? FunctionalMode { get; set; }

        [JsonPropertyName("functionalStatus")]
        public string? FunctionalStatus { get; set; }

        [JsonPropertyName("cmM_program_exception")]
        public bool CmmProgramException { get; set; }

        [JsonPropertyName("connectState")]
        public bool ConnectState { get; set; }

        [JsonPropertyName("workState")]
        public int WorkState { get; set; }

        [JsonPropertyName("errorMes")]
        public string? ErrorMes { get; set; }

        // ---- 程式與週期資訊 ----
        [JsonPropertyName("mainProgramName")]
        public string? MainProgramName { get; set; }

        [JsonPropertyName("subProgramName")]
        public string? SubProgramName { get; set; }

        [JsonPropertyName("cycleTime")]
        public string? CycleTime { get; set; }

        // ---- 控制權限 ----
        [JsonPropertyName("canControl")]
        public bool CanControl { get; set; }
    }
}
