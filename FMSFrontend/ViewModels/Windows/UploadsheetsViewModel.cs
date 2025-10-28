using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Extensions;
using FMSFrontend.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using OSCARMAXFMS_V3.DBmodels;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FMSFrontend.ViewModels.Windows
{
    public partial class UploadSheetViewModel : ObservableObject
    {
        private readonly IHttpService _httpService;

        // 支援 DI 與無 DI
        public UploadSheetViewModel() : this(
            FMSFrontend.App.ServiceProvider != null
                ? FMSFrontend.App.ServiceProvider.GetRequiredService<IHttpService>()
                : new HttpService()
        )
        { }

        public UploadSheetViewModel(IHttpService httpService)
        {
            _httpService = httpService;
        }

        [ObservableProperty]
        private string selectedWorkCsvPath = "尚未選擇檔案";

        [ObservableProperty]
        private string selectedEleCsvPath = "尚未選擇檔案";

        [ObservableProperty]
        private bool showElectrodeSection;

        [ObservableProperty]
        private bool showWorkpieceSection;

        // ▼ 下拉選單：機台
        [ObservableProperty]
        private ObservableCollection<string> availableEdms = new()
        {
            "EDM1","EDM2","EDM3"
        };

        [ObservableProperty]
        private string selectedEdm = "EDM1";

        // ▼ 下拉選單：座標
        [ObservableProperty]
        private ObservableCollection<string> availableCoordinates = new()
        {
            "G54","G55","G56","G57","G58","G59"
        };

        [ObservableProperty]
        private string selectedCoordinate = "G54";

        public ObservableCollection<ElectrodeItem> ElectrodeItems { get; set; } = new();
        public ObservableCollection<WorkItem> WorkItems { get; } = new();

        public string CurrentUserName { get; set; } = "Allen Lai"; // 依實際登入者取得方式調整

        [RelayCommand]
        private void SelectCsvFile(string type)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "CSV 檔案 (*.csv)|*.csv",
                Title = type == "Work"? "選擇檔案 (工件)" : "選擇檔案 (電極)",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                if (type == "Work")
                {
                    SelectedWorkCsvPath = dialog.FileName;
                    LoadWorkCsv(dialog.FileName);
                }
                else if (type == "Ele")
                {
                    SelectedEleCsvPath = dialog.FileName;
                    LoadElectrodeCsv(dialog.FileName);
                }
            }
        }

        private void LoadWorkCsv(string csvPath)
        {
            WorkItems.Clear();
            if (!File.Exists(csvPath)) return;

            var lines = File.ReadAllLines(csvPath);
            if (lines.Length == 0) return;

            // 假設第一列為標題，從第2列開始讀
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var cols = line.Split(',');
                if (cols.Length == 0) continue;

                var workpieceName = cols[0]?.Trim(); // A 欄 Workpieces
                if (string.IsNullOrWhiteSpace(workpieceName)) continue;

                // 量測程式：尋找 Workpieces\{工作名稱}\*.NCD 的第一個檔名
                var folder = Path.Combine(Path.GetDirectoryName(csvPath) ?? "", "Workpieces", workpieceName);
                var ncdFiles = Directory.Exists(folder) ? Directory.GetFiles(folder, "*.NCD") : Array.Empty<string>();
                var measurementProgram = ncdFiles.Length > 0 ? Path.GetFileName(ncdFiles[0]) : "";
                WorkItems.Add(new WorkItem
                {
                    IsSelected = true,
                    WorkpieceName = workpieceName,
                    Status = "new",
                    MeasurementProgram = measurementProgram ?? "",
                    SetupUser = CurrentUserName
                });
            }
        }

        private void LoadElectrodeCsv(string csvPath)
        {
            ElectrodeItems.Clear();
            var lines = File.ReadAllLines(csvPath);
            if (lines.Length < 2) return; // 沒有資料

            // 假設第一行是標題
            for (int i = 1; i < lines.Length; i++)
            {
                var cols = lines[i].Split(',');

                // 請根據你的CSV欄位順序調整索引
                var electrode = cols[0];      // A欄 Electrode
                var workPieces = cols[1];     // B欄 Work_pieces
                var offset = cols.Length > 18 ? cols[18] : ""; // S欄 Offset (第19欄, 索引18，先做長度保護)

                // 量測程式：尋找 Electrodes\{工作名稱} 資料夾下的 .NCD 檔名
                var folder = Path.Combine(Path.GetDirectoryName(csvPath) ?? "", "Electrodes", electrode);
                var ncdFiles = Directory.Exists(folder) ? Directory.GetFiles(folder, "*.NCD") : Array.Empty<string>();
                var measurementProgram = ncdFiles.Length > 0 ? Path.GetFileName(ncdFiles[0]) : "";

                ElectrodeItems.Add(new ElectrodeItem
                {
                    IsSelected = true,
                    ElectrodeName = electrode,
                    WorkPieces = workPieces,
                   // OffsetStatus = offset,
                    State = "new",
                    MeasurementProgram = measurementProgram,
                    SetupUser = CurrentUserName
                });
            }
        }

        [RelayCommand]
        private void ClearButton()
        {
            ElectrodeItems.Clear();
            WorkItems.Clear();
        }

        [RelayCommand]
        private async Task UpdataButton()
        {
            bool ok = false;
            var selectedWorks = WorkItems.Where(w => w.IsSelected).ToList();
            var selectedEles = ElectrodeItems.Where(e => e.IsSelected).ToList();

            if (selectedWorks.Count == 0 && selectedEles.Count == 0)
                return;

            string worksheetNumber = DateTime.Now.ToString("yyyyMMddHHmm"); // 年月日時分
            string targetEDM = MapTargetEdm(SelectedEdm);   // EDM1 -> EDM-1
            string pairedEDM = MapPairedEdm(SelectedEdm);   // EDM1 -> EMD1
            string coordinate = SelectedCoordinate;

            int extraOffset = selectedEles.Count(e => IsOffsetOneOrTwo(e.OffsetStatus));
            int totalProcessStep = selectedWorks.Count + selectedEles.Count + extraOffset;
          
            // 1) 先上傳工單
            var worksheetPayload = new
            {
                worksheetNumber,
                workpieceName = selectedWorks.FirstOrDefault()?.WorkpieceName ?? "",
                workPriority = 0,
                workEnabled = true,
                workStatus = "New",
                workPercentage = "0",
                targetEDM,          // EDM-1 風格
                processStep = 0,
                totalProcessStep,
                coordinate,         // 依座標下拉
                setupUser = "admin"
            };
            ok = await _httpService.SendPutAsync("Worksheet/DB_InsertNewWorkSheetData", worksheetPayload);
            if (!ok)
            {
                new DialogMessageWindow("工單上傳失敗").ShowDialog();
                return;
            }

            // 2) 依照工件數量上傳
            foreach (var w in selectedWorks)
            {
                var workPayload = new Workpiece
                {
                    tagSerial = "",
                    worksheetNumber = worksheetNumber,
                    workpieceName = w.WorkpieceName,
                    status = "New",
                    restriction = false,
                    pairedEDM = pairedEDM,
                    currentLocation = "",
                    tempRetSLocation = "",
                    isCompleted = false,
                    edmpgm = w.MeasurementProgram ?? "",
                    needInspect = true,
                    inspected = false,
                    inspectStatus = "",
                    inspectOffset = "",
                    setupUser = "admin"
                };
                ok = await _httpService.SendPutAsync("Workpiece/DB_InsertWorkpiece", workPayload);
                if (!ok)
                {
                    new DialogMessageWindow("工件上傳失敗: " + w.WorkpieceName.ToString()).ShowDialog();
                    return;
                }
            }

            // 3) 依照電極數量上傳
            string singleWorkName = selectedWorks.Count == 1 ? selectedWorks[0].WorkpieceName : string.Empty;

            foreach (var e in selectedEles)
            {
                var electrodePayload = new Electrode
                {
                    tagSerial = "",
                    worksheetNumber = worksheetNumber,
                    electrodeName = e.ElectrodeName,
                    state = "New",
                    restriction = false,
                    pairedEDM = pairedEDM,
                    currentLocation = "",
                    tempRetSLocation = "",
                    edmpgm = e.MeasurementProgram ?? "",
                    offset = "",
                    complementUpload = false,    // Electrode 類別為非 nullable bool，需給預設值
                    measuremented = false,       // 尚未量測
                    measurementStatus = "",
                    useTimes = null,
                    lifeTimes = e.LifeTimes,
                    offsetStatus = e.OffsetStatus,
                    underSize = "",
                    worksheetDone = "",
                    setupUser = "admin"
                };

                await _httpService.SendPutAsync("Electrode/DB_InsertElectrode", electrodePayload);
                if (!ok)
                {
                    new DialogMessageWindow("工件上傳失敗: " + electrodePayload).ShowDialog();
                    return;
                }
            }
            new DialogMessageWindow("上傳完成").ShowDialog();

        }

        private static bool IsOffsetOneOrTwo(int? n)
        {
            return n == 1 || n == 2;
        }

        // EDM1 -> EDM-1
        private static string MapTargetEdm(string edm)
        {
            if (string.IsNullOrWhiteSpace(edm)) return "EDM-1";
            return edm.StartsWith("EDM", StringComparison.OrdinalIgnoreCase) && edm.Length > 3
                ? $"EDM-{edm.Substring(3)}"
                : edm;
        }

        // EDM1 -> EMD1
        private static string MapPairedEdm(string edm)
        {
            if (string.IsNullOrWhiteSpace(edm)) return "EMD1";
            return edm.Replace("EDM", "EMD", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class ElectrodeItem
    {
        public bool IsSelected { get; set; }
        public string ElectrodeName { get; set; } = "";
        public string WorkPieces { get; set; } = "";
        public int LifeTimes { get; set; } = 0;

        public int OffsetStatus { get; set; } = 0;
        public string State { get; set; } = "new";
        public string MeasurementProgram { get; set; } = "";
        public string SetupUser { get; set; } = "";
    }

    public class WorkItem
    {
        public bool IsSelected { get; set; }
        public string WorkpieceName { get; set; } = "";
        public string Status { get; set; } = "new";
        public string MeasurementProgram { get; set; } = "";
        public string SetupUser { get; set; } = "";
    }
}
