using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Standard;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using MahApps.Metro.Controls;
using OSCARMAXFMS_V3.DBmodels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using static FMSFrontend.ViewModels.ElectrodeDetailViewModel;
using System.Collections.Concurrent;

namespace FMSFrontend.ViewModels.Production
{
    public partial class StorageOverviewViewModel : ObservableObject
    {
        private readonly ProductionLinesViewModel _parent;
        private readonly IHttpService _httpService;

        public ObservableCollection<StorageUnitViewModel> StorageUnits { get; set; } = new();

        public StorageOverviewViewModel(ProductionLinesViewModel parent, IHttpService httpService)
        {
            _parent = parent;
            _httpService = httpService;

            // 啟動即載入
            _ = LoadStorageUnitsAsync();
        }

        [RelayCommand]
        private void ToggleExpand()
        {
            _parent.ShowDetail("ES1");
        }

        [RelayCommand]
        private void OpenMaterial(StorageSlotViewModel slot)
        {
            _parent.OpenMaterial(slot);
        }

        // 依需求撈資料 + 更新 StorageUnits
        private async Task LoadStorageUnitsAsync()
        {
            // 1) 取得 Storage JSON（依你的需求先抓 JsonElement）
            JsonElement json;
            try
            {
                json = await _httpService.GetJsonAsync<JsonElement>("Storage/DB_GetAllStorageData");
            }
            catch
            {
                return;
            }

            // 2) 轉成模型並解析
            List<Storage> storages;
            try
            {
                storages = JsonSerializer.Deserialize<List<Storage>>(json.GetRawText()) ?? new List<Storage>();
            }
            catch
            {
                return;
            }

            // 無資料
            if (storages.Count == 0)
            {
                await Application.Current.Dispatcher.InvokeAsync(() => StorageUnits.Clear());
                return;
            }

            // 依 StorageName + StorageNumber 分組（例如 E + 1 → ES1、W + 1 → W1，這裡直接 StorageName+StorageNumber）
            var groups = storages
                .GroupBy(s => new { s.storageName, s.storageNumber })
                .OrderBy(g => g.Key.storageName)
                .ThenBy(g => g.Key.storageNumber)
                .ToList();

            var newUnits = new List<StorageUnitViewModel>();

            foreach (var g in groups)
            {
                var key = g.Key;
                var isElectrodeStore = key.storageName?.StartsWith("E", StringComparison.OrdinalIgnoreCase) == true;
                // 2.2 顯示名稱為 "StorageName + StorageNumber"
                var unitName = $"{key.storageName}{key.storageNumber}";

                // 取最大列/行當作格數
                int maxRow = Math.Max(1, g.Max(x => x.row));     // 行
                int maxCol = Math.Max(1, g.Max(x => x.column));  // 列

                var unit = new StorageUnitViewModel(unitName, maxRow, maxCol);
                unit.Slots.Clear();

                // 2.3 取得該倉別所有 OndeskTagserial（非空）
                var tagSerials = g.Select(x => x.ondeskTagserial)
                                  .Where(ts => !string.IsNullOrWhiteSpace(ts))
                                  .Distinct()
                                  .ToList();

                // 用 ConcurrentDictionary 快速且安全回填
                var tagStateMap = new ConcurrentDictionary<string, (string State, bool Restriction)>(StringComparer.OrdinalIgnoreCase);

                // 3) 依類別，批次查詢 TagSerial → 狀態與限制
                if (tagSerials.Count > 0)
                {
                    var tasks = tagSerials.Select(async ts =>
                    {
                        try
                        {
                            if (isElectrodeStore)
                            {
                                var route = $"Electrode/DB_GetElectrodesByTagSerial/{ts}";
                                var list = await _httpService.GetJsonAsync<List<Electrode>>(route) ?? new List<Electrode>();
                                var e = list.FirstOrDefault();
                                if (e != null)
                                {
                                    tagStateMap[ts] = (e.state ?? "Empty", e.restriction);
                                }
                            }
                            else
                            {
                                var route = $"Workpiece/DB_GetWorkpieceByTagSerial/{ts}";
                                var list = await _httpService.GetJsonAsync<List<Workpiece>>(route) ?? new List<Workpiece>();
                                var w = list.FirstOrDefault();
                                if (w != null)
                                {
                                    tagStateMap[ts] = (w.status ?? "Empty", w.restriction ?? false);
                                }
                            }
                        }
                        catch
                        {
                            // 忽略單筆錯誤
                        }
                    });
                    await Task.WhenAll(tasks);
                }

                // 4) 依 row/col 建立所有格位，並依查回的資料更新 Status/IsReserved
                for (int r = 1; r <= maxRow; r++)
                {
                    for (int c = 1; c <= maxCol; c++)
                    {
                        var rec = g.FirstOrDefault(x => x.row == r && x.column == c);
                        var tag = rec?.ondeskTagserial;

                        // 預設值
                        string stateValue = "Empty";
                        bool restrictionValue = false;

                        // 優先使用 tagStateMap（由電極/工件資料取得）
                        if (!string.IsNullOrWhiteSpace(tag) && tagStateMap.TryGetValue(tag, out var sr))
                        {
                            stateValue = string.IsNullOrWhiteSpace(sr.State) ? "Empty" : sr.State;
                            restrictionValue = sr.Restriction;
                        }
                        else
                        {
                            // 回退使用 Storage 原始欄位
                            stateValue = string.IsNullOrWhiteSpace(rec?.state) ? "Empty" : rec.state!;
                            restrictionValue = rec?.restriction ?? false;
                        }

                        var slot = new StorageSlotViewModel
                        {
                            IsElectrode = isElectrodeStore,
                            Line = ParseLineNumberFromName(unitName), // ES1 → 1、W2 → 2
                            Row = r,
                            Col = c,
                            Layer = 1,
                            Status = stateValue,
                            IsReserved = restrictionValue,
                            Material = new MaterialRef
                            {
                                Kind = isElectrodeStore ? MaterialKind.Electrode : MaterialKind.Workpiece,
                                Electrode = new ElectrodeModel
                                {
                                    TagSerial = rec?.ondeskTagserial,
                                    Restriction = restrictionValue
                                },
                                Workpiece = new WorkpieceModel
                                {
                                    SerialCode = rec?.ondeskTagserial,
                                    Restriction = restrictionValue
                                }
                            }
                        };

                        unit.Slots.Add(slot);
                    }
                }

                newUnits.Add(unit);
            }

            // 一次性套用到 UI
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                StorageUnits.Clear();
                foreach (var u in newUnits)
                    StorageUnits.Add(u);
            });

            static int ParseLineNumberFromName(string name)
            {
                var digits = new string(name.Where(char.IsDigit).ToArray());
                return int.TryParse(digits, out var n) ? n : 0;
            }
        }
    }

    public class StorageUnitViewModel
    {
        public string StorageName { get; set; } = "ES1";
        public int Rows { get; set; }
        public int Columns { get; set; }
        public ObservableCollection<StorageSlotViewModel> Slots { get; set; } = new();

        public Brush HeaderColor => StorageName.IndexOf("E") == 0
            ? new SolidColorBrush(Color.FromRgb(0x27, 0x79, 0xA7)) // 電極倉 = 藍色
            : new SolidColorBrush(Color.FromRgb(0xE0, 0x8E, 0x45)); // 工件倉 = 橘色

        public StorageUnitViewModel(string name, int rows, int cols)
        {
            StorageName = name;
            Rows = rows;
            Columns = cols;
        }
    }

    public class StorageSlotViewModel : ObservableObject
    {
        public string Status { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsReserved { get; set; }

        public Brush Background => Status switch
        {
            "Verified" => Brushes.Gold,
            "Working" => Brushes.Green,
            "Error" => Brushes.IndianRed,
            "Completed" => Brushes.RoyalBlue,
            "Reserved" => Brushes.Gray,
            "Empty" => Brushes.White,
            _ => Brushes.White
        };

        public MaterialRef Material { get; set; }

        public bool IsElectrode { get; set; }
        public int Line { get; set; }   // 倉線/倉號
        public int Row { get; set; }    // 行
        public int Col { get; set; }    // 列
        public int Layer { get; set; } = 1;

        public string SlotCode => $"{(IsElectrode ? "E" : "W")}:{Line}:{Row}:{Col}:{Layer}";
    }

    public class MaterialRef
    {
        public MaterialKind Kind { get; set; }
        public ElectrodeModel Electrode { get; set; }
        public WorkpieceModel Workpiece { get; set; }
        public IEnumerable<TimelineItemModel> Timeline { get; set; }
    }
}
