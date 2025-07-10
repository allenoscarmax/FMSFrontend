using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Controls;

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

    public partial class SlotViewModel : ObservableObject
    {
        public string Text { get; set; }
        public string Background { get; set; }
        public bool IsElectrode { get; set; }

        // 新增：加工結果（顯示左側 Path）
        [ObservableProperty]
        private ResultStatus resultStatus;

        // 新增：檢查狀態（顯示右側 Image）
        [ObservableProperty]
        private CheckStatus checkStatus;
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
