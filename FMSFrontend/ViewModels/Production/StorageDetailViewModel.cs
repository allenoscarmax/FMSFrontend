using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Controls;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Interfaces;
using FMSFrontend.Models;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel; // for PropertyChangedEventArgs
using System.Diagnostics;
using System.Linq;  // ← 需要
using System.Security.Permissions;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using static FMSFrontend.ViewModels.ProductionLinesViewModel;
//using System.Collections.Specialized;
namespace FMSFrontend.ViewModels.Production
{
    public partial class StorageDetailViewModel : ObservableObject
    {
        private readonly StorageStore _storageStore;
        public StorageGroupModel StorageGroup => _storageStore.StorageGroup;
        // Expose public SelectStorage for XAML binding
        public StorageModel? SelectStorage => StorageGroup?.SelectStorage;

        private readonly ProductionLinesViewModel _parent;
        // ==LiveUpdater===
        public StorageLiveUpdater _storageUpdater;

        private readonly IHttpService _httpService;

        [ObservableProperty] private string selectStorageTitle = "";
        [ObservableProperty] private int selectedTabIndex;     // 0: Electrode, 1: Workpiece
        [ObservableProperty] private int currentPageIndex;     // 當前頁索引
        [ObservableProperty] private bool isElectrode = true;  // 當前是否為電極視圖
        [ObservableProperty] private string pageInfo = "";  // 頁數顯示（供 XAML 綁定）

        // 選到的格位序號（供外部使用或顯示）
        [ObservableProperty] private string selectedSlotSerial = string.Empty;

        public List<string> PageList => StorageGroup?.Storage // Page 清單 (依電極/工件過濾)
        ?.Where(s => IsElectrode ? s.Kind == MaterialType.Electrode : s.Kind == MaterialType.Workpiece)
        .Select(s => s.Title).ToList() ?? new List<string>();

        public StorageDetailViewModel(ProductionLinesViewModel parent, IHttpService httpService,
            StorageStore storageStore, StorageLiveUpdater storageLiveUpdater)
        {
            _parent = parent;
            _httpService = httpService;
            _storageStore = storageStore;
            _storageUpdater = storageLiveUpdater;

            selectedTabIndex = 0;
            currentPageIndex = 0;
            SelectStorageTitle = "";

            // 當 Store 的 StorageGroup 內容變更時，轉發必要的通知以更新畫面
            _storageStore.StorageGroup.PropertyChanged += OnStorageGroupPropertyChanged;
            RefreshFromStore();
        }

        private void OnStorageGroupPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StorageGroupModel.SelectStorage))
            {
                OnPropertyChanged(nameof(SelectStorage));
            }
            if (e.PropertyName == nameof(StorageGroupModel.Storage))
            {
                OnPropertyChanged(nameof(PageList));
            }
            // 讓標題、頁碼等由外部更新時也能同步
            if (e.PropertyName is nameof(StorageGroupModel.SelectStorage) or nameof(StorageGroupModel.Storage))
            {
                RefreshFromStore();
            }
        }

        void RefreshFromStore()
        {
            PageInfo = $"{CurrentPageIndex + 1} / {PageList.Count}";
            if (PageList.Count > 0 && SelectStorageTitle == "")
            {
                SelectStorageTitle = PageList[CurrentPageIndex];
                _storageUpdater.SelectTitle = SelectStorageTitle;
            }
        }
        [RelayCommand]
        private void ShowOverview()
        {
            _parent.ShowOverview();
        }
        [RelayCommand]
        private void PrevPage() //換頁
        {
            if (PageList.Count <= 1)
                CurrentPageIndex = 0;
            else if (CurrentPageIndex > 0)
                CurrentPageIndex--;
            if (PageList.Count > 0)
                SelectStorageTitle = PageList[CurrentPageIndex];
            _storageUpdater.SelectTitle = SelectStorageTitle;
            _ = _storageUpdater.UpdateStatusAsync();
        }

        [RelayCommand]
        private void NextPage() //換頁
        {
            if (PageList.Count <= 1)
                CurrentPageIndex = 0;
            else if (CurrentPageIndex < PageList.Count - 1)
                CurrentPageIndex++;
            if (PageList.Count > 0)
                SelectStorageTitle = PageList[CurrentPageIndex];
            _storageUpdater.SelectTitle = SelectStorageTitle;
            _ = _storageUpdater.UpdateStatusAsync();
        }

        partial void OnSelectedTabIndexChanged(int value)      // 當 Tab 切換時重置頁索引並更新電極狀態與 PageList
        {
            IsElectrode = value == 0;
            CurrentPageIndex = 0;
            OnPropertyChanged(nameof(PageList));
            if (PageList.Count > 0)
                SelectStorageTitle = PageList[CurrentPageIndex];
            _storageUpdater.SelectTitle = SelectStorageTitle;
            _ = _storageUpdater.UpdateStatusAsync();
        }

        [RelayCommand]
        private async Task OpenMaterial(Slot slot) //打開材料資訊視窗
        {
            await _parent.OpenMaterialAsync(slot, MaterialOpenMode.SlotWindow);
        }
    }
}
