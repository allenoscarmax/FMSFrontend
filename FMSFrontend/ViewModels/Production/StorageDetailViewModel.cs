using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Controls;
using FMSFrontend.Interfaces;
using FMSFrontend.ViewModels.Windows;
using System.Collections.ObjectModel;
using System.Windows.Media;
using static FMSFrontend.ViewModels.ElectrodeDetailViewModel;
using System.Linq;  // ← 需要


namespace FMSFrontend.ViewModels.Production
{
    public partial class StorageDetailViewModel : ObservableObject
    {
        private readonly ProductionLinesViewModel _parent;

        public StorageDetailViewModel(ProductionLinesViewModel parent)
        {
            _parent = parent;
            SelectedTabIndex = 0;

            // 假設規則是 ES 開頭是電極，W 開頭是工件
            if (storageName.StartsWith("W"))
                StorageType = StorageType.Workpiece;
            else
                StorageType = StorageType.Electrode;

            PageInfo = $"{CurrentPageIndex + 1} / {StoragePages.Count}";

            LoadStoragePages();
        }
        [ObservableProperty] private int selectedTabIndex;
        [ObservableProperty] private int currentPageIndex;
        [ObservableProperty] private string pageInfo;
        [ObservableProperty] private string storageName = "ES1";

        [ObservableProperty] private int waitingCount;
        [ObservableProperty] private int processingCount;
        [ObservableProperty] private int errorCount;
        [ObservableProperty] private int completedCount;
        [ObservableProperty] private int disabledCount;
        [ObservableProperty] private int bookedCount;
        [ObservableProperty] private bool isElectrode = true; //控制狀態列

        [ObservableProperty] private StorageType storageType = StorageType.Electrode;

        public ObservableCollection<StoragePageViewModel> StoragePages { get; } = new();

        public StoragePageViewModel CurrentPage => StoragePages.Count > CurrentPageIndex ? StoragePages[CurrentPageIndex] : null;
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
                LoadStoragePages();
            }
        }
        /*
        private void LoadStoragePages()
        {
            StoragePages.Clear();
            string[] names;

            if (StorageType == StorageType.Electrode)
            {
                names = new[] { "ES1", "ES2", "ES3", "ES4" };
            }
            else
            {
                names = new[] { "W1" }; // 工件只顯示一頁 W1
            }

            var rnd = new Random();

            for (int i = 0; i < names.Length; i++)
            {
                var page = new StoragePageViewModel()
                {
                    Rows = 6,
                    Columns = StorageType == StorageType.Workpiece ? 5 : 10,
                    StorageName = names[i]
                };

                if (StorageType == StorageType.Electrode)
                {
                    for (int r = 0; r < page.Rows * page.Columns; r++)
                    {
                        var resultStatuses = new[] { ResultStatus.CheckSuccess, ResultStatus.Checking, ResultStatus.CheckFail };
                        var checkStatuses = new[] { CheckStatus.Checked, CheckStatus.Unchecked };

                        var slot = new SlotViewModel
                        {
                            Text = $"A{r + 1:D3}",
                            Background = "#FFFFFF",
                            ResultStatus = resultStatuses[rnd.Next(resultStatuses.Length)],
                            CheckStatus = checkStatuses[rnd.Next(checkStatuses.Length)],
                            IsElectrode = true
                        };

                        page.Slots.Add(slot);
                    }
                }
                else
                {
                    for (int r = 0; r < page.Rows * page.Columns; r++)
                    {

                        var slot = new SlotViewModel
                        {
                            Text = $"A{r + 1:D3}",
                            Background = "#FFFFFF",
                            IsElectrode = false
                        };

                        page.Slots.Add(slot);
                    }
                }
                
                StoragePages.Add(page);
            }

            CurrentPageIndex = 0;
            PageInfo = $"{CurrentPageIndex + 1} / {StoragePages.Count}";
            OnPropertyChanged(nameof(CurrentPage));
            UpdateCurrentStorageName();
        }
        */
        /*
        private void LoadStoragePages()
        {
            StoragePages.Clear();
            string[] names = StorageType == StorageType.Electrode
                ? new[] { "ES1", "ES2", "ES3", "ES4" }
                : new[] { "W1" }; // 工件只顯示一頁

            var rnd = new Random();

            foreach (var name in names)
            {
                var page = new StoragePageViewModel()
                {
                    Rows = 6,
                    Columns = StorageType == StorageType.Workpiece ? 5 : 10,
                    StorageName = name
                };

                for (int r = 0; r < page.Rows * page.Columns; r++)
                {
                    var slot = new SlotViewModel
                    {
                        Text = $"A{r + 1:D3}",
                        Background = "#FFFFFF",
                        IsElectrode = (StorageType == StorageType.Electrode),
                        ResultStatus = StorageType == StorageType.Electrode
                                       ? RandomEnum<ResultStatus>(rnd)
                                       : ResultStatus.CheckSuccess,
                        CheckStatus = StorageType == StorageType.Electrode
                                       ? RandomEnum<CheckStatus>(rnd)
                                       : CheckStatus.Checked
                    };

                    // ★ 假資料：90% 有物料，10% 空槽
                    bool makeEmpty = rnd.Next(100) < 10;
                    if (!makeEmpty)
                    {
                        if (StorageType == StorageType.Electrode)
                        {
                            slot.Material = new MaterialRef
                            {
                                Kind = MaterialKind.Electrode,
                                Electrode = new ElectrodeModel
                                {
                                    Name = $"ELE-{name}-{r:000}",
                                    No = $"E{DateTime.Now:MMdd}{r:000}",
                                    Type = rnd.Next(2) == 0 ? "Square" : "Round",
                                    HolderNo = $"H{rnd.Next(1, 20):00}",
                                    TagSerial = $"RF-{rnd.Next(100000, 999999)}"
                                },
                                Timeline = new[]
                                {
                            new TimelineItemModel { Text="入庫", Time=DateTime.Now.AddHours(-8), Status="✓"},
                            new TimelineItemModel { Text="檢驗完成", Time=DateTime.Now.AddHours(-6), Status="✓"},
                            new TimelineItemModel { Text="待派工", Time=DateTime.Now.AddHours(-2), Status="10%"}
                        }
                            };
                        }
                        else
                        {
                            slot.Material = new MaterialRef
                            {
                                Kind = MaterialKind.Workpiece,
                                Workpiece = new WorkpieceModel
                                {
                                    Name = $"WP-{name}-{r:000}",
                                    No = $"W{DateTime.Now:MMdd}{r:000}",
                                    BatchNo = $"B{rnd.Next(1, 9)}{rnd.Next(100, 999)}",
                                    RouteNo = $"R-{rnd.Next(1, 5)}"
                                },
                                Timeline = new[]
                                {
                            new TimelineItemModel { Text="入庫", Time=DateTime.Now.AddHours(-10), Status="✓"},
                            new TimelineItemModel { Text="檢驗完成", Time=DateTime.Now.AddHours(-7), Status="✓"},
                            new TimelineItemModel { Text="待加工", Time=DateTime.Now.AddHours(-1), Status="10%"}
                        }
                            };
                        }
                    }
                    // 若 makeEmpty == true，slot.Material 保持 null → 點擊會開「物料（空）」頁

                    page.Slots.Add(slot);
                }

                StoragePages.Add(page);
            }

            CurrentPageIndex = 0;
            PageInfo = $"{CurrentPageIndex + 1} / {StoragePages.Count}";
            OnPropertyChanged(nameof(CurrentPage));
            UpdateCurrentStorageName();

            // Local 函數：隨機 enum
            static T RandomEnum<T>(Random r) where T : Enum
            {
                var values = (T[])Enum.GetValues(typeof(T));
                return values[r.Next(values.Length)];
            }
        }
        */
        
        private void LoadStoragePages()
        {
            StoragePages.Clear();

            string[] names = StorageType == StorageType.Electrode
                ? new[] { "ES1"}
                : new[] { "W1" };

            var rnd = new Random();

            foreach (var name in names)
            {
                var page = new StoragePageViewModel()
                {
                    Rows = StorageType == StorageType.Workpiece ? 2 : 3,
                    Columns = StorageType == StorageType.Workpiece ? 5 : 10,
                    StorageName = name
                };

                for (int r = 0; r < page.Rows * page.Columns; r++)
                {
                    // 先算列/行/倉號
                    int rowIndex = r / page.Columns;   // 0-based
                    int colIndex = r % page.Columns;   // 0-based
                    var digits = new string(page.StorageName.Where(char.IsDigit).ToArray());
                    int lineNo = int.TryParse(digits, out var n) ? n : 0;

                    var slot = new SlotViewModel
                    {
                        // 讓 SlotCode 走數字格式 → Text 設 null
                        Text = null,
                        Background = "#FFFFFF",
                        IsElectrode = (StorageType == StorageType.Electrode),

                        // ★ 這些是 SlotViewModel 的座標（SlotCode 會用到）
                        Line = lineNo,
                        Row = rowIndex + 1,
                        Col = colIndex + 1,
                        Layer = 1,

                        ResultStatus = StorageType == StorageType.Electrode
                       ? RandomEnum<ResultStatus>(rnd)
                       : ResultStatus.CheckSuccess,
                        CheckStatus = StorageType == StorageType.Electrode
                       ? RandomEnum<CheckStatus>(rnd)
                       : CheckStatus.Checked
                    };


                    // ★（以下保留你的 Material 造假）
                    bool makeEmpty = rnd.Next(100) < 10;
                    if (!makeEmpty)
                    {
                        if (StorageType == StorageType.Electrode)
                        {
                            slot.Material = new MaterialRef
                            {
                                Kind = MaterialKind.Electrode,
                                Electrode = new ElectrodeModel
                                {
                                    Name = $"ELE-{name}-{r:000}",
                                    No = $"E{DateTime.Now:MMdd}{r:000}",
                                    Type = rnd.Next(2) == 0 ? "Square" : "Round",
                                    HolderNo = $"H{rnd.Next(1, 20):00}",
                                    TagSerial = $"RF-{rnd.Next(100000, 999999)}"
                                },
                                Timeline = new[]
                                {
                                    new TimelineItemModel { Text="入庫", Time=DateTime.Now.AddHours(-8), Status="✓"},
                                    new TimelineItemModel { Text="檢驗完成", Time=DateTime.Now.AddHours(-6), Status="✓"},
                                    new TimelineItemModel { Text="待派工", Time=DateTime.Now.AddHours(-2), Status="10%"}
                                }
                            };
                        }
                        else
                        {
                            slot.Material = new MaterialRef
                            {
                                Kind = MaterialKind.Workpiece,
                                Workpiece = new WorkpieceModel
                                {
                                    Name = $"WP-{name}-{r:000}",
                                    No = $"W{DateTime.Now:MMdd}{r:000}",
                                    BatchNo = $"B{rnd.Next(1, 9)}{rnd.Next(100, 999)}",
                                    RouteNo = $"R-{rnd.Next(1, 5)}"
                                },
                                Timeline = new[]
                                {
                            new TimelineItemModel { Text="入庫", Time=DateTime.Now.AddHours(-10), Status="✓"},
                            new TimelineItemModel { Text="檢驗完成", Time=DateTime.Now.AddHours(-7), Status="✓"},
                            new TimelineItemModel { Text="待加工", Time=DateTime.Now.AddHours(-1), Status="10%"}
                        }
                            };
                        }
                    }

                    page.Slots.Add(slot);
                }

                StoragePages.Add(page);
            }

            CurrentPageIndex = 0;
            PageInfo = $"{CurrentPageIndex + 1} / {StoragePages.Count}";
            OnPropertyChanged(nameof(CurrentPage));
            UpdateCurrentStorageName();

            static T RandomEnum<T>(Random r) where T : Enum
            {
                var values = (T[])Enum.GetValues(typeof(T));
                return values[r.Next(values.Length)];
            }
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
        public string Text { get; set; }
        public string Background { get; set; }
        public bool IsElectrode { get; set; }

        [ObservableProperty] private ResultStatus resultStatus;
        [ObservableProperty] private CheckStatus checkStatus;

        public MaterialRef Material { get; set; }

        // 位置資訊（數字）
        public int Line { get; set; } = 1;  // 倉線/倉號，例如 ES1 → 1、W3 → 3
        public int Row { get; set; }       // 列
        public int Col { get; set; }       // 行
        public int Layer { get; set; } = 1;  // 層（先固定 1）

        // 這格的類型（保留你的推導）
        public MaterialKind Kind =>
            IsElectrode ? MaterialKind.Electrode :
            (Material != null ? MaterialKind.Workpiece : MaterialKind.None);

        // ★ 倉位代碼：Text 有值就直接顯示；否則用數字組 "E:1:row:col:layer" 或 "W:1:..."
        public string SlotCode =>
            !string.IsNullOrWhiteSpace(Text)
                ? Text
                : $"{(IsElectrode ? "E" : "W")}:{Line}:{Row}:{Col}:{Layer}";
    }

    public class StatusItemViewModel
    {
        public string Label { get; set; }
        public Brush Color { get; set; }
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
