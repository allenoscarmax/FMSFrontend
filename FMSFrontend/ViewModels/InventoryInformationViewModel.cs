using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Standard;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services.FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Models;
using FMSFrontend.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FMSFrontend.ViewModels
{


    public partial class InventoryInformationViewModel : ObservableObject
    {
        // === Services ===
        private readonly IStorageService _storageService;

        // === Singleton ===
        private readonly StorageStore _storageStore;
        public ObservableCollection<StorageModel> Storages => _storageStore.StorageGroup.Storage;
        // ==LiveUpdater===
        private readonly StorageLiveUpdater _storageLiveUpdater;
        // 原始資料
        public ObservableCollection<InventoryItem> AllItems { get; } = new();

        // 提供給 XAML 綁定的清單（已套用過濾）
        public ICollectionView InventoryList { get; }

        [ObservableProperty] private int selectedTabIndex = 0;
        [ObservableProperty] private string searchText = "";

        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand<object> SearchCommand { get; }

        public InventoryInformationViewModel(IStorageService storageService,
            StorageStore storageStore,
            StorageLiveUpdater storageLiveUpdater)
        {
            _storageService = storageService;
            _storageStore = storageStore;
            _storageLiveUpdater = storageLiveUpdater;

            InventoryList = CollectionViewSource.GetDefaultView(AllItems);
            InventoryList.Filter = FilterRow;

            RefreshCommand = new RelayCommand(() =>
            {
                _ = RefreshFromSelectedTabIndex();
                InventoryList.Refresh();
            });

            // 由 Enter 觸發（XAML 的 KeyDownEnterOnlyConverter 會限制只在 Enter 執行）
            SearchCommand = new RelayCommand<object>(param =>
            {
                if (param is TextBox tb) SearchText = tb.Text;
                InventoryList.Refresh();
            });

            // 初始化資料
            _ = RefreshFromSelectedTabIndex();
            InventoryList.Refresh();
        }

        // 在 SelectedTabIndex 變更時刷新資料與過濾
        partial void OnSelectedTabIndexChanged(int value)
        {
            _ = RefreshFromSelectedTabIndex();
            InventoryList?.Refresh();
        }

        // 在 SearchText 變更時刷新過濾
        partial void OnSearchTextChanged(string value)
        {
            InventoryList?.Refresh();
        }

        private async Task RefreshFromSelectedTabIndex()
        {
            try
            {
                await _storageLiveUpdater.UpdateStatusAsync();
                AllItems.Clear();
                foreach (var storage in Storages)
                {
                    foreach (var slot in storage.Slots)
                    {
                        InventoryKind kind = InventoryKind.unKnow;
                        bool IsOffShelf = slot.MaterialStatus.Equals("OffShelf", StringComparison.OrdinalIgnoreCase);
                        if (!IsOffShelf && storage.Name.IndexOf("W") == -1) kind = InventoryKind.OnShelfElectrode;
                        else if (!IsOffShelf && storage.Name.IndexOf("E") == -1) kind = InventoryKind.OnShelfWork;
                        else if (IsOffShelf && storage.Name.IndexOf("W") == -1) kind = InventoryKind.OffShelfElectrode;
                        else if (IsOffShelf && storage.Name.IndexOf("E") == -1) kind = InventoryKind.OffShelfWork;
                        string[] sr = slot.SlotCode.Split(':');
                        string newSlotCode = sr[0];
                        for (int i = 1; i < sr.Length; i++)
                        {
                            newSlotCode += ":" + sr[i].PadLeft(2, ' ');
                        }
                        AllItems.Add(new InventoryItem
                        {
                            Kind = kind,
                            Name = slot.Name,
                            TagSerial = slot.Serial,
                            Status = slot.MaterialStatus,
                            Location = slot.Location,
                            SlotCode = newSlotCode,
                            Worksheet = slot.Worksheet,
                            Program = slot.Program,
                        });
                    }
                }

            }
            catch { }
        }
        public void OnPageActivated() // 開啟警報視窗
        {

        }
        public void OnPageDeactivated() // 關閉警報視窗
        {

        }
        private bool FilterRow(object obj)
        {
            if (obj is not InventoryItem r) return false;

            // 依 Tab 過濾
            bool tabOK = (int)r.Kind == SelectedTabIndex;
            if (!tabOK) return false;

            // 搜尋文字
            var q = (SearchText ?? string.Empty).Trim();
            if (q.Length == 0) return true;

            return
                Contains(r.Name, q) ||
                Contains(r.TagSerial, q) ||
                Contains(r.Status, q) ||
                Contains(r.Location, q) ||
                Contains(r.SlotCode, q) ||
                Contains(r.Worksheet, q) ||
                Contains(r.Program, q);
        }

        private static bool Contains(string? src, string q) =>
            (src ?? string.Empty).IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
        public class InventoryItem
        {
            public InventoryKind Kind { get; set; }
            public string Name { get; set; } = ""; //名稱
            public string TagSerial { get; set; } = ""; //標籤序號
            public string Status { get; set; } = ""; //狀態
            public string Location { get; set; } = ""; //位置編碼
            public string SlotCode { get; set; } = ""; //位置編碼
            public string Worksheet { get; set; } = ""; //工單編號
            public string Program { get; set; } = ""; //稱是名稱

        }
        public enum InventoryKind
        {
            OnShelfWork = 0,       // 架上工件
            OnShelfElectrode = 1,  // 架上電極
            OffShelfWork = 2,      // 已下架工件
            OffShelfElectrode = 3,  // 已下架電極
            unKnow = 4  // 未知電極
        }
    }
}
