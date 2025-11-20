using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Extensions;
using FMSFrontend.Interfaces;
using FMSFrontend.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Dtos;

namespace FMSFrontend.ViewModels.Windows
{
    public partial class UploadSheetViewModel : ObservableObject
    {
        private readonly IWindowService _windowService;

        private readonly IElectrodeService  _electrodeService;
        private readonly IWorkpieceService  _workpieceService;
        private readonly IWorksheetsService _worksheetsService;

        public UploadSheetViewModel(
            IWindowService windowService,
            IElectrodeService electrodeService,
            IWorkpieceService workpieceService,
            IWorksheetsService worksheetsService)
        {
            _windowService = windowService;
            _worksheetsService = worksheetsService;
            _electrodeService = electrodeService;
            _workpieceService = workpieceService;
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
            "Unset","EDM1","EDM2","EDM3"
        };

        [ObservableProperty]
        private string selectedEdm = "Unset";

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

            var baseDir = Path.GetDirectoryName(csvPath) ?? "";

            // 先檢查是否有任一 .NCD 檔以及可能的 Workpieces/EDM 資料夾
            bool hasWorkpiecesFolder = Directory.Exists(Path.Combine(baseDir, "Workpieces")) || Directory.Exists(Path.Combine(baseDir, "EDM", "Workpieces"));
            bool hasAnyNcd = false;
            try { hasAnyNcd = Directory.GetFiles(baseDir, "*.NCD", SearchOption.AllDirectories).Any(); }
            catch { /* 忽略權限錯誤 */ }

            if (!hasWorkpiecesFolder || !hasAnyNcd)
            {
                new DialogMessageWindow("Format Error").ShowDialog();
                return;
            }

            string? FindFirstNcd(params string[] folders)
            {
                try
                {
                    foreach (var f in folders)
                    {
                        if (string.IsNullOrWhiteSpace(f)) continue;
                        if (!Directory.Exists(f)) continue;
                        var files = Directory.GetFiles(f, "*.NCD", SearchOption.TopDirectoryOnly);
                        if (files.Length > 0) return files[0];
                    }
                }
                catch { /* 忽略 IO/權限問題 */ }
                return null;
            }

            bool IsMatchToCandidate(string foundPath, string candidate)
            {
                if (string.IsNullOrEmpty(foundPath) || string.IsNullOrEmpty(candidate)) return false;
                var fileName = Path.GetFileNameWithoutExtension(foundPath) ?? "";
                if (fileName.StartsWith(candidate, StringComparison.OrdinalIgnoreCase)) return true;

                // 檢查目錄名稱是否包含或以 candidate 結尾
                var parts = foundPath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in parts)
                {
                    if (string.Equals(p, candidate, StringComparison.OrdinalIgnoreCase)) return true;
                    if (p.EndsWith(candidate, StringComparison.OrdinalIgnoreCase)) return true;
                }
                return false;
            }

            string[] lines;
            try
            {
                lines = File.ReadAllLines(csvPath);
            }
            catch (IOException)
            {
                new DialogMessageWindow("檔案被其他程式使用中，請先關閉該檔案後再試。").ShowDialog();
                return;
            }

            if (lines.Length == 0) return;

            // 假設第一列為標題，從第2列開始讀
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var cols = line.Split(',');
                if (cols.Length == 0) continue;

                var workpieceNameRaw = cols[0]?.Trim();
                if (string.IsNullOrWhiteSpace(workpieceNameRaw)) continue;

                // 產生候選名稱（清理常見尾碼與附註）
                var candidate = workpieceNameRaw;
                if (candidate.Contains(' ')) candidate = candidate.Split(' ')[0].Trim();
                candidate = System.Text.RegularExpressions.Regex.Replace(candidate, @"(-W\d*$|_W\d*$|-W$|_W$)", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();

                string? found = null;

                var tryFolders = new[]
                {
                    Path.Combine(baseDir, "EDM", "Workpieces", candidate),
                    Path.Combine(baseDir, "EDM", "Workpieces", candidate, candidate),
                    Path.Combine(baseDir, "Workpieces", candidate),
                    Path.Combine(baseDir, "Workpieces", candidate, candidate),
                    Path.Combine(baseDir, candidate),
                    Path.Combine(baseDir, "EDM", candidate),
                };

                found = FindFirstNcd(tryFolders);

                if (found == null)
                {
                    try
                    {
                        var edmRoot = Path.Combine(baseDir, "EDM");
                        if (Directory.Exists(edmRoot))
                        {
                            var dirs = Directory.GetDirectories(edmRoot, "*", SearchOption.AllDirectories);
                            foreach (var d in dirs)
                            {
                                if (d.EndsWith(candidate, StringComparison.OrdinalIgnoreCase))
                                {
                                    var f = FindFirstNcd(d);
                                    if (f != null) { found = f; break; }
                                }
                            }
                        }
                    }
                    catch { /* 忽略 */ }
                }

                // 最後保守搜尋 baseDir 下任一 .NCD，但之後必須檢查是否符合 candidate
                if (found == null)
                {
                    try { found = Directory.GetFiles(baseDir, "*.NCD", SearchOption.AllDirectories).FirstOrDefault(); }
                    catch { /* 忽略 */ }
                }

                // 若找不到或找到的檔案與候選名稱不符 -> 視為格式錯誤
                if (string.IsNullOrEmpty(found) || !IsMatchToCandidate(found, candidate))
                {
                    new DialogMessageWindow("Format Error").ShowDialog();
                    return;
                }

                var measurementProgram = Path.GetFileNameWithoutExtension(found);

                WorkItems.Add(new WorkItem
                {
                    IsSelected = true,
                    WorkpieceName = workpieceNameRaw,
                    Status = "new",
                    MeasurementProgram = measurementProgram ?? "",
                    SetupUser = CurrentUserName
                });
            }
        }

        private void LoadElectrodeCsv(string csvPath)
        {
            ElectrodeItems.Clear();
            if (!File.Exists(csvPath)) return;

            var baseDir = Path.GetDirectoryName(csvPath) ?? "";

            // 初步檢查：是否有 EDM\Electrodes 或 Electrodes 資料夾，且是否有任何 .NCD 檔
            bool hasElectrodesFolder = Directory.Exists(Path.Combine(baseDir, "EDM", "Electrodes")) || Directory.Exists(Path.Combine(baseDir, "Electrodes"));
            bool hasAnyNcd = false;
            try { hasAnyNcd = Directory.GetFiles(baseDir, "*.NCD", SearchOption.AllDirectories).Any(); }
            catch { /* 忽略權限錯誤 */ }

            if (!hasElectrodesFolder || !hasAnyNcd)
            {
                new DialogMessageWindow("Format Error").ShowDialog();
                return;
            }

            string? FindFirstNcd(params string[] folders)
            {
                try
                {
                    foreach (var f in folders)
                    {
                        if (string.IsNullOrWhiteSpace(f)) continue;
                        if (!Directory.Exists(f)) continue;
                        var files = Directory.GetFiles(f, "*.NCD", SearchOption.TopDirectoryOnly);
                        if (files.Length > 0) return files[0];
                    }
                }
                catch { /* 忽略 IO/權限問題 */ }
                return null;
            }

            bool IsMatchToCandidate(string foundPath, string candidate)
            {
                if (string.IsNullOrEmpty(foundPath) || string.IsNullOrEmpty(candidate)) return false;
                var fileName = Path.GetFileNameWithoutExtension(foundPath) ?? "";
                if (fileName.StartsWith(candidate, StringComparison.OrdinalIgnoreCase)) return true;

                var parts = foundPath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in parts)
                {
                    if (string.Equals(p, candidate, StringComparison.OrdinalIgnoreCase)) return true;
                    if (p.EndsWith(candidate, StringComparison.OrdinalIgnoreCase)) return true;
                }
                return false;
            }

            string[] lines;
            try
            {
                lines = File.ReadAllLines(csvPath);
            }
            catch (IOException)
            {
                new DialogMessageWindow("檔案被其他程式使用中，請先關閉該檔案後再試。").ShowDialog();
                return;
            }

            if (lines.Length < 2) return; // 沒有資料

            // 以標題行找出欄位索引（更穩定）：Electrode, Work_pieces（代表 lifeTimes）, Offset
            var headers = lines[0].Split(',');
            int idxElectrode = Array.FindIndex(headers, h => (h ?? "").IndexOf("electrode", StringComparison.OrdinalIgnoreCase) >= 0);
            int idxWorkPieces = Array.FindIndex(headers, h => (h ?? "").IndexOf("work_piec", StringComparison.OrdinalIgnoreCase) >= 0);
            int idxOffset = Array.FindIndex(headers, h => (h ?? "").IndexOf("offset", StringComparison.OrdinalIgnoreCase) >= 0);

            // 假如沒找到標題，退回到預設索引（保守處理）
            if (idxElectrode < 0) idxElectrode = 0;

            for (int i = 1; i < lines.Length; i++)
            {
                var cols = lines[i].Split(',');
                if (cols.Length == 0) continue;

                var electrodeRaw = idxElectrode >= 0 && cols.Length > idxElectrode ? cols[idxElectrode].Trim() : (cols.Length > 0 ? cols[0].Trim() : "");
                if (string.IsNullOrWhiteSpace(electrodeRaw)) continue;

                // 產生候選名稱（清理常見尾碼與附註）
                var candidate = electrodeRaw;
                if (candidate.Contains(' ')) candidate = candidate.Split(' ')[0].Trim();
                candidate = System.Text.RegularExpressions.Regex.Replace(candidate, @"(-W\d*$|_W\d*$|-W$|_W$)", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();

                string? found = null;

                // 可能的資料夾位置（以 CSV 所在資料夾為基準）
                var tryFolders = new[]
                {
                    Path.Combine(baseDir, "EDM", "Electrodes", candidate),
                    Path.Combine(baseDir, "EDM", "Electrodes", candidate, candidate),
                    Path.Combine(baseDir, "Electrodes", candidate),
                    Path.Combine(baseDir, "Electrodes", candidate, candidate),
                    Path.Combine(baseDir, candidate),
                    Path.Combine(baseDir, "EDM", candidate),
                };

                found = FindFirstNcd(tryFolders);

                if (found == null)
                {
                    try
                    {
                        var edmRoot = Path.Combine(baseDir, "EDM");
                        if (Directory.Exists(edmRoot))
                        {
                            var dirs = Directory.GetDirectories(edmRoot, "*", SearchOption.AllDirectories);
                            foreach (var d in dirs)
                            {
                                if (d.EndsWith(candidate, StringComparison.OrdinalIgnoreCase))
                                {
                                    var f = FindFirstNcd(d);
                                    if (f != null) { found = f; break; }
                                }
                            }
                        }

                        // 也嘗試 EDM\Electrodes 的所有子資料夾去抓第一個 .NCD
                        if (found == null)
                        {
                            var electrodesRoot = Path.Combine(baseDir, "EDM", "Electrodes");
                            if (Directory.Exists(electrodesRoot))
                            {
                                var any = Directory.GetFiles(electrodesRoot, "*.NCD", SearchOption.AllDirectories).FirstOrDefault();
                                if (!string.IsNullOrEmpty(any)) found = any;
                            }
                        }
                    }
                    catch { /* 忽略 */ }
                }

                // 最後保守搜尋 baseDir 下任一 .NCD，但之後必須檢查是否符合 candidate
                if (found == null)
                {
                    try { found = Directory.GetFiles(baseDir, "*.NCD", SearchOption.AllDirectories).FirstOrDefault(); }
                    catch { /* 忽略 */ }
                }

                // 若找不到或找到的檔案與候選名稱不符 -> 視為格式錯誤
                if (string.IsNullOrEmpty(found) || !IsMatchToCandidate(found, candidate))
                {
                    new DialogMessageWindow("Format Error").ShowDialog();
                    return;
                }

                // 量測程式名稱：以 .NCD 檔名（不含副檔名）為主，找不到再使用資料夾名稱
                var measurementProgram = Path.GetFileNameWithoutExtension(found) ?? candidate;

                // 解析 lifeTimes（來自 Work_pieces 欄位）與 offset（Offset 欄位）
                int lifeTimes = 0;
                if (idxWorkPieces >= 0 && cols.Length > idxWorkPieces)
                    int.TryParse(cols[idxWorkPieces].Trim(), out lifeTimes);

                int offsetVal = 0;
                if (idxOffset >= 0 && cols.Length > idxOffset)
                    int.TryParse(cols[idxOffset].Trim(), out offsetVal);

                ElectrodeItems.Add(new ElectrodeItem
                {
                    IsSelected = true,
                    ElectrodeName = electrodeRaw,
                    WorkPieces = (idxWorkPieces >= 0 && cols.Length > idxWorkPieces) ? cols[idxWorkPieces].Trim() : "",
                    LifeTimes = lifeTimes,
                    OffsetStatus = offsetVal,
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

            string worksheetNumber = DateTime.Now.ToString("yyyyMMddHHmmss"); // 年月日時分秒
            string targetEDM
;            if (SelectedEdm.Contains("EDM"))
            {
                targetEDM = MapTargetEdm(SelectedEdm);   // EDM1 -> EDM-
            }
            else 
            {
                targetEDM = "";
            }
                string pairedEDM = MapPairedEdm(SelectedEdm);   // EDM1 -> EMD1
            string coordinate = SelectedCoordinate;

            int extraOffset = selectedEles.Count(e => IsOffsetOneOrTwo(e.OffsetStatus));
            int totalProcessStep = selectedWorks.Count + selectedEles.Count + extraOffset;

            // 1) 先上傳工單

            WorksheetsDto wsDto = new WorksheetsDto
            {
                worksheetNumber = worksheetNumber,
                workpieceName = selectedWorks.FirstOrDefault()?.WorkpieceName ?? "",
                workPriority = 0,
                workEnabled = true,
                workStatus = "New",
                workPercentage = "0",
                targetEDM = targetEDM,
                processStep = 0,
                totalProcessStep = totalProcessStep,
                coordinate = coordinate,
                setupUser = "admin"
            };
            try
            {
                ok = await _worksheetsService.InsertNewWorkSheetDataAsync(wsDto);
                if (!ok)
                {
                    new DialogMessageWindow("工單上傳失敗").ShowDialog();
                    return;
                }
            }
            catch { }
            

            // 2) 依照工件數量上傳
            foreach (var w in selectedWorks)
            {
                WorkpieceDto wpDto = new WorkpieceDto
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
                };
                try
                {
                    ok = await _workpieceService.InsertWorkpieceAsync(wpDto);
                    if (!ok)
                    {
                        new DialogMessageWindow("工件上傳失敗: " + w.WorkpieceName.ToString()).ShowDialog();
                        return;
                    }
                }
                catch { }
            }

            // 3) 依照電極數量上傳
            string singleWorkName = selectedWorks.Count == 1 ? selectedWorks[0].WorkpieceName : string.Empty;

            foreach (var e in selectedEles)
            {
                ElectrodeDto elDto = new ElectrodeDto
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
                    shared = false,
                    shareLink = "",
                    worksheetDone = "",
                    setupUser = "admin"
                };
                try
                {
                    ok = await _electrodeService.DB_InsertElectrodeAsync(elDto);
                    if (!ok)
                    {
                        new DialogMessageWindow("電極資料上傳失敗").ShowDialog();
                        return;
                    }
                }
                catch { }
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

        [RelayCommand]
        private void Share(ElectrodeItem item)
        {
            if (item == null) return;

            // 從 electrodeName 萃取工件名稱（第一個 '_' 之前的字串）
            var workpieceName = ExtractWorkpieceNameFromElectrodeName(item.ElectrodeName);
            if (string.IsNullOrEmpty(workpieceName)) return;

            // 開窗（把工件名稱丟進去）
            _windowService.ShowSelectSharedElectrodeWindow(workpieceName);

            // 若你還要接回傳結果，可用：
            // if (_windowService.ShowSelectSharedElectrodeWindow(workpieceName, out var selection) && selection != null)
            // { ...後續處理... }

        }
        /// <summary>
        /// 例： "2504AF019617-001_3-001A-01" -> "2504AF019617-001"
        /// 規則：取第一個 '_' 前的所有字元；若沒有 '_'，就回傳原字串。
        /// </summary>
        private static string ExtractWorkpieceNameFromElectrodeName(string? electrodeName)
        {
            if (string.IsNullOrWhiteSpace(electrodeName))
                return string.Empty;

            // 方式一：最快，找第一個底線
            int idx = electrodeName.IndexOf('_');
            if (idx > 0)
                return electrodeName.Substring(0, idx);

            // 方式二（防極端格式）：取到 "-01"、"-02" 前面的主體再去掉後段，但通常不會走到這
            // var m = Regex.Match(electrodeName, @"^(.+?)_");
            // return m.Success ? m.Groups[1].Value : electrodeName;

            return electrodeName;
        }
    }

    public partial class ElectrodeItem : ObservableObject
    {
        [ObservableProperty] public bool isSelected;
        [ObservableProperty] public string electrodeName = "";
        [ObservableProperty] public string workPieces = "";
        [ObservableProperty] public int lifeTimes = 0;
        [ObservableProperty] public int offsetStatus = 0;
        [ObservableProperty] public string state = "new";
        [ObservableProperty] public string measurementProgram = "";
        [ObservableProperty] public string setupUser = "";

        // 根據電極名稱尾碼決定是否可分享 (例如尾碼為 "02")
        public bool CanShare => !string.IsNullOrWhiteSpace(ElectrodeName) && ElectrodeName.Trim().EndsWith("01", StringComparison.OrdinalIgnoreCase);

        // 當影響 CanShare 的欄位變更時發通知以更新 UI
        partial void OnElectrodeNameChanged(string value) => OnPropertyChanged(nameof(CanShare));
        partial void OnOffsetStatusChanged(int value) => OnPropertyChanged(nameof(CanShare));
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
