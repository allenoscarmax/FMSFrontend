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
        public ObservableCollection<StorageModel>  Storages => _storageStore.StorageGroup.Storage;
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
                //InventoryList.Refresh();
            });

            // 由 Enter 觸發（XAML 的 KeyDownEnterOnlyConverter 會限制只在 Enter 執行）
            SearchCommand = new RelayCommand<object>(param =>
            {
                if (param is TextBox tb) SearchText = tb.Text;
               // InventoryList.Refresh();
            });

            // 切換分頁時自動刷新
            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(SelectedTabIndex))
                {
                    _ = RefreshFromSelectedTabIndex();
                    //InventoryList.Refresh();
                }
            };
            _ = RefreshFromSelectedTabIndex();
        }
        private async Task RefreshFromSelectedTabIndex()
        {
            //try
            //{
               await _storageLiveUpdater.UpdateStatusAsync();
                AllItems.Clear();
            foreach (var storage in Storages)
            {
                if (SelectedTabIndex == 0 && storage.Name.IndexOf("W") == -1) continue;
                else if (SelectedTabIndex == 1 && storage.Name.IndexOf("E") == -1) continue;
                else if (SelectedTabIndex == 2 && storage.Name.IndexOf("W") == -1) continue;
                else if (SelectedTabIndex == 3 && storage.Name.IndexOf("E") == -1) continue;
                foreach (var slot in storage.Slots)
                {
                    AllItems.Add(new InventoryItem
                    {
                        Kind = (InventoryKind)slot.Kind,        //類型??
                        BurnSerial = slot.Serial,               //序號??
                        TagSerial = "",                         //序號??
                        BindState = slot.StorageStatus,         //綁定狀態??
                        Status = slot.MaterialStatus,           //狀態
                        Location = slot.SlotCode,               //位置編碼
                        StorageArea = "",//slot.Region,              //區域??
                        WorkOrder ="", //slot.WorksheetNumber,       //工單
                        Process ="",// slot.,                        //執行程式
                        Machine ="", //slot,                         //配對機台
                        PartNo ="", //slot.StorageNumber.,           //工件編號
                        PartName ="", //slot.PartName,               //工件名稱
                        LotNo = ""                              //Storages[i].LotNo //??
                    });
                }
            }

            //}
            //catch { }
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

            return Contains(r.BurnSerial, q) || Contains(r.TagSerial, q) ||
                   Contains(r.BindState, q) || Contains(r.Status, q) ||
                   Contains(r.Location, q) || Contains(r.StorageArea, q) ||
                   Contains(r.WorkOrder, q) || Contains(r.Process, q) ||
                   Contains(r.Machine, q) || Contains(r.PartNo, q) ||
                   Contains(r.PartName, q) || Contains(r.LotNo, q);
        }

        private static bool Contains(string? src, string q) =>
            (src ?? string.Empty).IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
        private void SeedFakeData()
        {
            // 架上工件
            AllItems.Add(new InventoryItem
            {
                Kind = InventoryKind.OnShelfWork,
                BurnSerial = "WRP20240604111825",
                TagSerial = "TG2022100666541",
                BindState = "",
                Status = "Booked",
                Location = "inStore",
                StorageArea = "W:1:1:3:1",
                WorkOrder = "123",
                Process = "OP310 OP320",
                Machine = "EDM3",
                PartNo = "2748M04G01",
                PartName = "整流罩",
                LotNo = "NA"
            });
            AllItems.Add(new InventoryItem
            {
                Kind = InventoryKind.OnShelfWork,
                BurnSerial = "WRP20240530164555",
                TagSerial = "TG2022111441128",
                BindState = "",
                Status = "Completed",
                Location = "inStore",
                StorageArea = "W:1:4:1",
                WorkOrder = "CG4T014805",
                Process = "A07-2",
                Machine = "EDM4",
                PartNo = "2748M04G01",
                PartName = "整流罩",
                LotNo = "NA"
            });
            AllItems.Add(new InventoryItem
            {
                Kind = InventoryKind.OnShelfWork,
                BurnSerial = "WRP20240601150419",
                TagSerial = "TG2024024288513",
                BindState = "",
                Status = "Completed",
                Location = "inStore",
                StorageArea = "W:2:1:4:1",
                WorkOrder = "CG4T015199",
                Process = "OP310 OP320",
                Machine = "EDM4",
                PartNo = "2748M04G01",
                PartName = "整流罩",
                LotNo = "NA"        //
            });

            // 架上電極
            AllItems.Add(new InventoryItem
            {
                Kind = InventoryKind.OnShelfElectrode,
                BurnSerial = "ELD20240603090837",
                TagSerial = "TG2020700415758",
                BindState = "Y",
                Status = "Idle",
                Location = "Rack-E1",
                StorageArea = "E:1:2:1",
                WorkOrder = "-",
                Process = "—",
                Machine = "—",
                PartNo = "EL-6R-Ø4",
                PartName = "電極-Ø4R6",
                LotNo = "NA"
            });
            AllItems.Add(new InventoryItem
            {
                Kind = InventoryKind.OnShelfElectrode,
                BurnSerial = "ELD20240603210204",
                TagSerial = "TG2021060285013",
                BindState = "Y",
                Status = "Reserved",
                Location = "Rack-E3",
                StorageArea = "E:1:3:1",
                WorkOrder = "WO-7788",
                Process = "EDM",
                Machine = "EDM5",
                PartNo = "EL-Flat-Ø6",
                PartName = "電極-Ø6平頭",
                LotNo = "NA"
            });

            // 已下架工件
            AllItems.Add(new InventoryItem
            {
                Kind = InventoryKind.OffShelfWork,
                BurnSerial = "WRP20240701094522",
                TagSerial = "TG202301010001",
                BindState = "N",
                Status = "Processing",
                Location = "onEDM5",
                StorageArea = "-",
                WorkOrder = "CG4T015199",
                Process = "OP310 OP320",
                Machine = "EDM5",
                PartNo = "2748M04G01",
                PartName = "整流罩",
                LotNo = "B-202407"
            });

            // 已下架電極
            AllItems.Add(new InventoryItem
            {
                Kind = InventoryKind.OffShelfElectrode,
                BurnSerial = "ELD20240702081533",
                TagSerial = "TG202301019999",
                BindState = "Y",
                Status = "InUse",
                Location = "onEDM4",
                StorageArea = "-",
                WorkOrder = "WO-8899",
                Process = "EDM",
                Machine = "EDM4",
                PartNo = "EL-Point-Ø1",
                PartName = "電極-尖頭Ø1",
                LotNo = "NA"
            });
        }
        public class InventoryItem
        {
            public InventoryKind Kind { get; set; }

            public string BurnSerial { get; set; } = "";
            public string TagSerial { get; set; } = "";
            public string BindState { get; set; } = "";
            public string Status { get; set; } = "";
            public string Location { get; set; } = "";
            public string StorageArea { get; set; } = "";
            public string WorkOrder { get; set; } = "";
            public string Process { get; set; } = "";
            public string Machine { get; set; } = "";
            public string PartNo { get; set; } = "";
            public string PartName { get; set; } = "";
            public string LotNo { get; set; } = "";
        }
        public enum InventoryKind
        {
            OnShelfWork = 0,       // 架上工件
            OnShelfElectrode = 1,  // 架上電極
            OffShelfWork = 2,      // 已下架工件
            OffShelfElectrode = 3  // 已下架電極
        }
    }
}
