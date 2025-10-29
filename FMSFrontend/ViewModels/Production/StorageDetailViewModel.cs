using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Controls;
using FMSFrontend.Interfaces;
using FMSFrontend.ViewModels.Windows;
using System.Collections.ObjectModel;
using System.Windows.Media;
using static FMSFrontend.ViewModels.ElectrodeDetailViewModel;
using System.Linq;  // ← 需要
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows;
using OSCARMAXFMS_V3.DBmodels;
using FMSFrontend.Services;
namespace FMSFrontend.ViewModels.Production
{
    public partial class StorageDetailViewModel : ObservableObject
    {
        private readonly ProductionLinesViewModel _parent;
        private readonly IHttpService _httpService;
        public Task RefreshAsync() => Task.Run(async () => await UpdateStoragePagesAsync());
        public StorageDetailViewModel(ProductionLinesViewModel parent, IHttpService httpService)
        {
            _parent = parent;
            _httpService = httpService;

            SelectedTabIndex = 0;

            // 假設規則是 ES 開頭是電極，W 開頭是工件
            if (storageName.StartsWith("W"))
                StorageType = StorageType.Workpiece;
            else
                StorageType = StorageType.Electrode;

            PageInfo = $"{CurrentPageIndex + 1} / {StoragePages.Count}";

            _ = LoadStoragePagesAsync();
        }
        [ObservableProperty] private int selectedTabIndex;
        [ObservableProperty] private int currentPageIndex;
        [ObservableProperty] private string pageInfo = "";
        [ObservableProperty] private string storageName = "ES1";

        [ObservableProperty] private string waitingCount = "";
        [ObservableProperty] private string processingCount = "";
        [ObservableProperty] private string errorCount = "";
        [ObservableProperty] private string completedCount = "";
        [ObservableProperty] private string bookedCount = "";
        [ObservableProperty] private string restrictionCount = "";
        [ObservableProperty] private bool isElectrode = true; //控制狀態列

        [ObservableProperty] private StorageType storageType = StorageType.Electrode;

        public ObservableCollection<StoragePageViewModel> StoragePages { get; } = new();

        public StoragePageViewModel? CurrentPage => StoragePages.Count > CurrentPageIndex ? StoragePages[CurrentPageIndex] : null;
        [RelayCommand]
        private void ShowOverview()
        {
            // 假設你要展開到特定 StorageId 的 DetailControl
            _parent.ShowOverview();
        }
        [RelayCommand]
        private void PrevPage()
        {
            if (CurrentPageIndex > 0)
            {
                CurrentPageIndex--;
                PageInfo = $"{CurrentPageIndex + 1} / {StoragePages.Count}";
                OnPropertyChanged(nameof(CurrentPage));
                UpdateCurrentStorageName();
            }
        }

        [RelayCommand]
        private void NextPage()
        {
            if (CurrentPageIndex < StoragePages.Count - 1)
            {
                CurrentPageIndex++;
                PageInfo = $"{CurrentPageIndex + 1} / {StoragePages.Count}";
                OnPropertyChanged(nameof(CurrentPage));
                UpdateCurrentStorageName();
            }
        }
        [RelayCommand]
        private void SelectSlot(SlotViewModel slot)
        {
            _parent.OpenMaterial(slot);  // 直接丟給父 VM（多載會吃到）
        }

        partial void OnSelectedTabIndexChanged(int value)
        {
            var type = value == 0 ? StorageType.Electrode : StorageType.Workpiece;
            
            if (StorageType != type)
            {
                StorageType = type;
                IsElectrode = StorageType == StorageType.Electrode;
                _ = LoadStoragePagesAsync();
            }
        }

        // 重新實作：與 StorageOverviewViewModel.LoadStorageUnitsAsync 相同模式
        private async Task LoadStoragePagesAsync()
        {
            StoragePages.Clear();

            // 1) 先抓全部 Storage 資料
            JsonElement json;
            try
            {
                json = await _httpService.GetJsonAsync<JsonElement>("Storage/DB_GetAllStorageData");
            }
            catch
            {
                // API 失敗就保留空頁面集合
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    StoragePages.Clear();
                });
                return;
            }

            List<Storage> storages;
            try
            {
                storages = JsonSerializer.Deserialize<List<Storage>>(json.GetRawText()) ?? new List<Storage>();
            }
            catch
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    StoragePages.Clear();
                });
                return;
            }

            if (storages.Count == 0)
            {
                await Application.Current.Dispatcher.InvokeAsync(() => StoragePages.Clear());
                return;
            }

            // 依 StorageName + StorageNumber 分組（例如 E + 1 → ES1、W + 1 → W1）
            var groups = storages
                .GroupBy(s => new { s.storageName, s.storageNumber })
                .OrderBy(g => g.Key.storageName)
                .ThenBy(g => g.Key.storageNumber)
                .ToList();

            var newPages = new List<StoragePageViewModel>();

            foreach (var g in groups)
            {
                var key = g.Key;
                var unitName = $"{key.storageName}{key.storageNumber}";
                var isElectrodeStore = key.storageName?.IndexOf("E") != -1;

                // 只建立與目前選取 Tab（電極/工件）相符的頁面
                if (isElectrodeStore && StorageType != StorageType.Electrode) continue;
                if (!isElectrodeStore && StorageType != StorageType.Workpiece) continue;

                int maxRow = Math.Max(1, g.Max(x => x.row));     // 行
                int maxCol = Math.Max(1, g.Max(x => x.column));  // 列

                var page = new StoragePageViewModel
                {
                    StorageName = unitName,
                    Rows = maxRow,
                    Columns = maxCol
                };

                // 取得該倉別所有 OndeskTagserial（非空）
                var tagSerials = g.Select(x => x.ondeskTagserial)
                          .Where(ts => !string.IsNullOrWhiteSpace(ts))
                          .Distinct()
                          .ToList();

                // 現在也儲存 status 字串（若有）
                var tagMap = new ConcurrentDictionary<string, (string Tag, bool Restriction, string Status)>(System.StringComparer.OrdinalIgnoreCase);

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
                                    tagMap[ts] = (e.tagSerial ?? ts, e.restriction, e.state);
                                }
                            }
                            else
                            {
                                var route = $"Workpiece/DB_GetWorkpieceByTagSerial/{ts}";
                                JsonElement? json = await _httpService.GetJsonAsync<JsonElement>(route, default);
                                Workpiece w = (json.HasValue && json.Value.ValueKind != JsonValueKind.Undefined) ?
                                JsonSerializer.Deserialize<Workpiece>(json.Value.GetRawText()) ?? new Workpiece() :
                                new Workpiece();
                                if (w != null)
                                {
                                    tagMap[ts] = (w.tagSerial ?? ts, w.restriction ?? false, w.status);
                                }
                            }
                        }
                        catch
                        {
                            // 單筆錯誤忽略
                        }
                    });

                    await Task.WhenAll(tasks);
                }

                // 建立格位並回填資料
                for (int r = 1; r <= page.Rows; r++)
                {
                    for (int c = 1; c <= page.Columns; c++)
                    {
                        var rec = g.FirstOrDefault(x => x.row == r && x.column == c);
                        var tag = rec?.ondeskTagserial;

                        // 決定格位的 Status（優先：tagMap 的 DB status → rec.state → Empty）
                        string slotStatus = "";
                        if (!string.IsNullOrWhiteSpace(tag) && tagMap.TryGetValue(tag, out var info))
                        {
                            slotStatus = string.IsNullOrWhiteSpace(info.Status) ? "Empty" : info.Status;
                        }

                        var slot = new SlotViewModel
                        {
                            Status = slotStatus,
                            Text = "",
                            IsElectrode = isElectrodeStore,
                            Line = int.TryParse(new string(unitName.Where(char.IsDigit).ToArray()), out var n) ? n : 0,
                            Row = r,
                            Col = c,
                            Layer = 1,
                            IsLocked = rec?.restriction ?? false,
                            // 保留原本 enum 隨機或預設，這裡使用預設值
                            ResultStatus = isElectrodeStore ? ResultStatus.Checking : ResultStatus.CheckSuccess,
                            CheckStatus = isElectrodeStore ? CheckStatus.Unchecked : CheckStatus.Checked
                        };

                        // 如有 tag 且 API 有回來，就塞入 MaterialRef 的最小資訊（TagSerial + Restriction）
                        if (!string.IsNullOrWhiteSpace(tag) && tagMap.TryGetValue(tag, out var info2))
                        {
                            if (isElectrodeStore)
                            {
                                slot.Material = new MaterialRef
                                {
                                    Kind = MaterialKind.Electrode,
                                    Electrode = new ElectrodeModel
                                    {
                                        TagSerial = info2.Tag ?? tag,
                                        Restriction = info2.Restriction
                                    },
                                    Timeline = Enumerable.Empty<TimelineItemModel>()
                                };
                            }
                            else
                            {
                                slot.Material = new MaterialRef
                                {
                                    Kind = MaterialKind.Workpiece,
                                    Workpiece = new WorkpieceModel
                                    {
                                        SerialCode = info2.Tag ?? tag,
                                        Restriction = info2.Restriction
                                    },
                                    Timeline = Enumerable.Empty<TimelineItemModel>()
                                };
                            }
                        }
                        else
                        {
                            // 若無 tag 或 API 無資料，回退使用 Storage 原始欄位（如果存在 restriction）
                            if (!string.IsNullOrWhiteSpace(rec?.state) || rec?.restriction != null)
                            {
                                bool restr = rec?.restriction ?? false;
                                var state = string.IsNullOrWhiteSpace(rec?.state) ? "Empty" : rec!.state!;
                                if (isElectrodeStore)
                                {
                                    slot.Material = new MaterialRef
                                    {
                                        Kind = MaterialKind.Electrode,
                                        Electrode = new ElectrodeModel
                                        {
                                            TagSerial = rec?.ondeskTagserial ?? string.Empty,
                                            Restriction = restr
                                        },
                                        Timeline = Enumerable.Empty<TimelineItemModel>()
                                    };
                                }
                                else
                                {
                                    slot.Material = new MaterialRef
                                    {
                                        Kind = MaterialKind.Workpiece,
                                        Workpiece = new WorkpieceModel
                                        {
                                            SerialCode = rec?.ondeskTagserial ?? string.Empty,
                                            Restriction = restr
                                        },
                                        Timeline = Enumerable.Empty<TimelineItemModel>()
                                    };
                                }
                            }
                        }
                        page.Slots.Add(slot);
                    }
                }
                newPages.Add(page);
            }
            // 計算電極相關狀態統計（僅計算 IsElectrode = true 的格位）
            var electrodeSlots = newPages.SelectMany(u => u.Slots).Where(s => s.IsElectrode).ToList();
            int waiting = electrodeSlots.Count(s => string.Equals(s.Status, "Verified", StringComparison.OrdinalIgnoreCase));
            int processing = electrodeSlots.Count(s => string.Equals(s.Status, "Working", StringComparison.OrdinalIgnoreCase));
            int error = electrodeSlots.Count(s => string.Equals(s.Status, "Error", StringComparison.OrdinalIgnoreCase));
            int completed = electrodeSlots.Count(s => string.Equals(s.Status, "Completed", StringComparison.OrdinalIgnoreCase));

            // 由 Storage 原始資料計算：restriction == true
            int restriction = storages.Count(s => s.restriction == true);
            // 由 Storage 原始資料計算：state == "Book"（大小寫不敏感）
            int booked = storages.Count(s => string.Equals(s.state, "Book", StringComparison.OrdinalIgnoreCase));
            // 一次性套用到 UI（包含 StorageUnits 與計數字串）
            // 套用到 UI
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                StoragePages.Clear();
                foreach (var p in newPages)
                    StoragePages.Add(p);

                CurrentPageIndex = 0;
                PageInfo = $"{CurrentPageIndex + 1} / {StoragePages.Count}";
                OnPropertyChanged(nameof(CurrentPage));
                UpdateCurrentStorageName();
                WaitingCount = waiting.ToString();
                ProcessingCount = processing.ToString();
                ErrorCount = error.ToString();
                CompletedCount = completed.ToString();
                BookedCount = booked.ToString();
                RestrictionCount = restriction.ToString();
            });
        }

        /// <summary>
        /// 與 LoadStoragePagesAsync 同樣向 API 查詢，但只更新現有 StoragePages 的內容（更新 slot / rows / cols），
        /// 不會一律清除整個 StoragePages 集合，以保留 UI 綁定的物件實體。
        /// </summary>
        private async Task UpdateStoragePagesAsync()
        {
            // 1) 先抓全部 Storage 資料
            JsonElement json;
            try
            {
                json = await _httpService.GetJsonAsync<JsonElement>("Storage/DB_GetAllStorageData");
            }
            catch
            {
                // 失敗就不更新
                return;
            }

            List<Storage> storages;
            try
            {
                storages = JsonSerializer.Deserialize<List<Storage>>(json.GetRawText()) ?? new List<Storage>();
            }
            catch
            {
                return;
            }

            if (storages.Count == 0)
                return;

            var groups = storages
                .GroupBy(s => new { s.storageName, s.storageNumber })
                .OrderBy(g => g.Key.storageName)
                .ThenBy(g => g.Key.storageNumber)
                .ToList();

            var newPages = new List<StoragePageViewModel>();

            foreach (var g in groups)
            {
                var key = g.Key;
                var unitName = $"{key.storageName}{key.storageNumber}";
                var isElectrodeStore = key.storageName?.IndexOf("E") != -1;

                if (isElectrodeStore && StorageType != StorageType.Electrode) continue;
                if (!isElectrodeStore && StorageType != StorageType.Workpiece) continue;

                int maxRow = Math.Max(1, g.Max(x => x.row));
                int maxCol = Math.Max(1, g.Max(x => x.column));

                var page = new StoragePageViewModel
                {
                    StorageName = unitName,
                    Rows = maxRow,
                    Columns = maxCol
                };

                var tagSerials = g.Select(x => x.ondeskTagserial)
                          .Where(ts => !string.IsNullOrWhiteSpace(ts))
                          .Distinct()
                          .ToList();

                var tagMap = new ConcurrentDictionary<string, (string Tag, bool Restriction, string Status)>(System.StringComparer.OrdinalIgnoreCase);

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
                                    tagMap[ts] = (e.tagSerial ?? ts, e.restriction, e.state);
                                }
                            }
                            else
                            {
                                var route = $"Workpiece/DB_GetWorkpieceByTagSerial/{ts}";
                                JsonElement? json2 = await _httpService.GetJsonAsync<JsonElement>(route, default);
                                Workpiece w = (json2.HasValue && json2.Value.ValueKind != JsonValueKind.Undefined) ?
                                JsonSerializer.Deserialize<Workpiece>(json2.Value.GetRawText()) ?? new Workpiece() :
                                new Workpiece();
                                if (w != null)
                                {
                                    tagMap[ts] = (w.tagSerial ?? ts, w.restriction ?? false, w.status);
                                }
                            }
                        }
                        catch
                        {
                            // ignore
                        }
                    });

                    await Task.WhenAll(tasks);
                }

                for (int r = 1; r <= page.Rows; r++)
                {
                    for (int c = 1; c <= page.Columns; c++)
                    {
                        var rec = g.FirstOrDefault(x => x.row == r && x.column == c);
                        var tag = rec?.ondeskTagserial;

                        string slotStatus = "";
                        if (!string.IsNullOrWhiteSpace(tag) && tagMap.TryGetValue(tag, out var info))
                        {
                            slotStatus = string.IsNullOrWhiteSpace(info.Status) ? "Empty" : info.Status;
                        }

                        var slot = new SlotViewModel
                        {
                            Status = slotStatus,
                            Text = "",
                            IsElectrode = isElectrodeStore,
                            Line = int.TryParse(new string(unitName.Where(char.IsDigit).ToArray()), out var n) ? n : 0,
                            Row = r,
                            Col = c,
                            Layer = 1,
                            IsLocked = rec?.restriction ?? false,
                            ResultStatus = isElectrodeStore ? ResultStatus.Checking : ResultStatus.CheckSuccess,
                            CheckStatus = isElectrodeStore ? CheckStatus.Unchecked : CheckStatus.Checked
                        };

                        if (!string.IsNullOrWhiteSpace(tag) && tagMap.TryGetValue(tag, out var info2))
                        {
                            if (isElectrodeStore)
                            {
                                slot.Material = new MaterialRef
                                {
                                    Kind = MaterialKind.Electrode,
                                    Electrode = new ElectrodeModel
                                    {
                                        TagSerial = info2.Tag ?? tag,
                                        Restriction = info2.Restriction
                                    },
                                    Timeline = Enumerable.Empty<TimelineItemModel>()
                                };
                            }
                            else
                            {
                                slot.Material = new MaterialRef
                                {
                                    Kind = MaterialKind.Workpiece,
                                    Workpiece = new WorkpieceModel
                                    {
                                        SerialCode = info2.Tag ?? tag,
                                        Restriction = info2.Restriction
                                    },
                                    Timeline = Enumerable.Empty<TimelineItemModel>()
                                };
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(rec?.state) || rec?.restriction != null)
                            {
                                bool restr = rec?.restriction ?? false;
                                var state = string.IsNullOrWhiteSpace(rec?.state) ? "Empty" : rec!.state!;
                                if (isElectrodeStore)
                                {
                                    slot.Material = new MaterialRef
                                    {
                                        Kind = MaterialKind.Electrode,
                                        Electrode = new ElectrodeModel
                                        {
                                            TagSerial = rec?.ondeskTagserial ?? string.Empty,
                                            Restriction = restr
                                        },
                                        Timeline = Enumerable.Empty<TimelineItemModel>()
                                    };
                                }
                                else
                                {
                                    slot.Material = new MaterialRef
                                    {
                                        Kind = MaterialKind.Workpiece,
                                        Workpiece = new WorkpieceModel
                                        {
                                            SerialCode = rec?.ondeskTagserial ?? string.Empty,
                                            Restriction = restr
                                        },
                                        Timeline = Enumerable.Empty<TimelineItemModel>()
                                    };
                                }
                            }
                        }
                        page.Slots.Add(slot);
                    }
                }
                newPages.Add(page);
            }

            // 計算統計
            var electrodeSlots2 = newPages.SelectMany(u => u.Slots).Where(s => s.IsElectrode).ToList();
            int waiting2 = electrodeSlots2.Count(s => string.Equals(s.Status, "Verified", StringComparison.OrdinalIgnoreCase));
            int processing2 = electrodeSlots2.Count(s => string.Equals(s.Status, "Working", StringComparison.OrdinalIgnoreCase));
            int error2 = electrodeSlots2.Count(s => string.Equals(s.Status, "Error", StringComparison.OrdinalIgnoreCase));
            int completed2 = electrodeSlots2.Count(s => string.Equals(s.Status, "Completed", StringComparison.OrdinalIgnoreCase));

            int restriction2 = storages.Count(s => s.restriction == true);
            int booked2 = storages.Count(s => string.Equals(s.state, "Book", StringComparison.OrdinalIgnoreCase));

            // 更新 UI：不會清空整個 StoragePages，而是更新現有頁面內容、移除不存在頁、並新增新的頁
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var newByName = newPages.ToDictionary(p => p.StorageName, StringComparer.OrdinalIgnoreCase);
                var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // 更新或移除現有頁面
                foreach (var existing in StoragePages.ToList())
                {
                    if (existing == null) continue;
                    var name = existing.StorageName ?? string.Empty;
                    if (newByName.TryGetValue(name, out var updatedPage))
                    {
                        // 更新 rows/cols
                        existing.Rows = updatedPage.Rows;
                        existing.Columns = updatedPage.Columns;

                        // 更新 Slots：不要清空集合，改为逐格比對更新已處理的 slots，並新增缺少的 slots
                        // 以 (Line,Row,Col,Layer) 作為鍵
                        var existingMap = existing.Slots.ToDictionary(s => (s.Line, s.Row, s.Col, s.Layer));

                        var processedSlots = new HashSet<(int Line, int Row, int Col, int Layer)>();

                        foreach (var updatedSlot in updatedPage.Slots)
                        {
                            var key = (updatedSlot.Line, updatedSlot.Row, updatedSlot.Col, updatedSlot.Layer);
                            processedSlots.Add(key);

                            if (existingMap.TryGetValue(key, out var existingSlot))
                            {
                                // 更新欄位（只更新會變動的欄位）
                                existingSlot.Status = updatedSlot.Status;
                                existingSlot.Text = updatedSlot.Text;
                                existingSlot.IsElectrode = updatedSlot.IsElectrode;
                                existingSlot.IsLocked = updatedSlot.IsLocked;
                                existingSlot.ResultStatus = updatedSlot.ResultStatus;
                                existingSlot.CheckStatus = updatedSlot.CheckStatus;
                                existingSlot.Material = updatedSlot.Material;
                                // 若需要同步其他欄位（Line/Row/Col/Layer）通常不需要，因為鍵已相同
                            }
                            else
                            {
                                // 新增缺少的 slot（保持物件型別一致）
                                existing.Slots.Add(updatedSlot);
                            }
                        }

                        // 不自動移除 existing.Slots 中未被更新到的 slots（遵照使用者要求「不清除只更新 processed」）

                        processed.Add(name);
                    }
                    else
                    {
                        // 移除不存在的頁
                        StoragePages.Remove(existing);
                    }
                }

                // 新增新頁
                foreach (var p in newPages)
                {
                    if (processed.Contains(p.StorageName)) continue;
                    StoragePages.Add(p);
                }

                // 調整 CurrentPageIndex
                if (CurrentPageIndex >= StoragePages.Count)
                    CurrentPageIndex = Math.Max(0, StoragePages.Count - 1);

                PageInfo = $"{CurrentPageIndex + 1} / {StoragePages.Count}";
                OnPropertyChanged(nameof(CurrentPage));
                UpdateCurrentStorageName();

                WaitingCount = waiting2.ToString();
                ProcessingCount = processing2.ToString();
                ErrorCount = error2.ToString();
                CompletedCount = completed2.ToString();
                BookedCount = booked2.ToString();
                RestrictionCount = restriction2.ToString();
            });
        }

        private void UpdateCurrentStorageName()
        {
            if (CurrentPage != null)
            {
                StorageName = CurrentPage.StorageName;
            }
        }
    }
    // -----------------------------
    // Sub ViewModel: StoragePageViewModel.cs
    // -----------------------------

    public class StoragePageViewModel : ObservableObject
    {
        public string StorageName { get; set; } = string.Empty;
        public int Rows { get; set; }
        public int Columns { get; set; }

        public ObservableCollection<SlotViewModel> Slots { get; } = new();
    }

    public partial class SlotViewModel : ObservableObject, IHasMaterial
    {
        // 讓 Status 有變更通知（CommunityToolkit 會產生公開屬性）
        [ObservableProperty]
        private string status = "Empty";

        [ObservableProperty]
        private string text = "";

        // 計算屬性，根據 Status 回傳 Brush
        public Brush Background => Status switch
        {
            "Verified" => new SolidColorBrush(Color.FromRgb(0xE6, 0xB9, 0x3E)), // 待加工 (黃)
            "Working" => new SolidColorBrush(Color.FromRgb(0x56, 0xC0, 0x6C)),  // 加工中 (綠)
            "Error" => new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B)),    // 異常 (紅)
            "Completed" => new SolidColorBrush(Color.FromRgb(0x2F, 0x64, 0xCF)),// 完成 (藍)
            "Reserved" => new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)), // 保留/預約 (灰)
            "Empty" => Brushes.White,
            _ => Brushes.White
        };

        public bool IsElectrode { get; set; }

        [ObservableProperty] private ResultStatus resultStatus;
        [ObservableProperty] private CheckStatus checkStatus;
        [ObservableProperty] private bool isLocked;

        public MaterialRef Material { get; set; } = new MaterialRef();

        public int Line { get; set; } = 1;
        public int Row { get; set; }
        public int Col { get; set; }
        public int Layer { get; set; } = 1;

        public MaterialKind Kind =>
            IsElectrode ? MaterialKind.Electrode :
            (Material != null ? MaterialKind.Workpiece : MaterialKind.None);

        public string SlotCode =>
            !string.IsNullOrWhiteSpace(Text)
                ? Text
                : $"{(IsElectrode ? "E" : "W")}:{Line}:{Row}:{Col}:{Layer}";

        // 當 Status 變更時，通知 Background 也更新
        partial void OnStatusChanged(string value)
        {
            OnPropertyChanged(nameof(Background));
        }
    }

    public class StatusItemViewModel
    {
        public string Label { get; set; } = "";
        public Brush Color { get; set; } = Brushes.Gray; //Allen
    }
    public enum StorageType
    {
        Electrode,
        Workpiece
    }
    public enum ResultStatus
    {
        CheckSuccess,
        Checking,
        CheckFail
    }

    public enum CheckStatus
    {
        Checked,
        Unchecked
    }
}
