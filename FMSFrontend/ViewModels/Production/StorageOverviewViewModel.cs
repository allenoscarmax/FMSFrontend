using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Standard;
using FMSFrontend.Interfaces;
using FMSFrontend.ViewModels.Windows;
using System.Collections.ObjectModel;
using System.Windows.Media;
using static FMSFrontend.ViewModels.ElectrodeDetailViewModel;

namespace FMSFrontend.ViewModels.Production
{
    public partial class StorageOverviewViewModel : ObservableObject
    {
        private readonly ProductionLinesViewModel _parent;

        public ObservableCollection<StorageUnitViewModel> StorageUnits { get; set; } = new();

        public StorageOverviewViewModel(ProductionLinesViewModel parent)
        {
            _parent = parent;

            // ES* → 自動判斷為電極倉；W* → 工件倉
            StorageUnits.Add(FakeDataFactory.CreateStorageUnit("ES1", 6, 8));
            StorageUnits.Add(FakeDataFactory.CreateStorageUnit("ES2", 6, 8));
            StorageUnits.Add(FakeDataFactory.CreateStorageUnit("W1", 2, 5));
            StorageUnits.Add(FakeDataFactory.CreateStorageUnit("W2", 2, 5));
            StorageUnits.Add(FakeDataFactory.CreateStorageUnit("W3", 2, 5));
            // ... 其他倉儲
        }

        [RelayCommand]
        private void ToggleExpand()
        {
            // 假設你要展開到特定 StorageId 的 DetailControl
            _parent.ShowDetail("ES1");
        }
        [RelayCommand]
        private void OpenMaterial(StorageSlotViewModel slot)
        {
            _parent.OpenMaterial(slot);


        }
    }



    public class StorageUnitViewModel
    {
        public string StorageName { get; set; } = "ES1";
        public int Rows { get; set; }
        public int Columns { get; set; }
        public ObservableCollection<StorageSlotViewModel> Slots { get; set; } = new();

        public Brush HeaderColor => StorageName.StartsWith("ES")
    ? new SolidColorBrush(Color.FromRgb(0x27, 0x79, 0xA7)) // 電極倉 = 藍色
    : new SolidColorBrush(Color.FromRgb(0xE0, 0x8E, 0x45)); // 工件倉 = 橘色

        public StorageUnitViewModel(string name, int rows, int cols)
        {
            StorageName = name;
            Rows = rows;
            Columns = cols;

           

        }
    }

    public class StorageSlotViewModel : ObservableObject, IHasMaterial
    {
        public string Status { get; set; } = "Verified";
        public bool IsDisabled { get; set; }
        public bool IsReserved { get; set; }

        // 顏色（保留）
        public Brush Background => Status switch
        {
            "Verified" => Brushes.Gold,
            "Working" => Brushes.Green,
            "Error" => Brushes.IndianRed,
            "Completed" => Brushes.RoyalBlue,
            "Reserved" => Brushes.Gray,
            _ => Brushes.White
        };

        public MaterialRef Material { get; set; }

        // ★ 新增：判斷前綴 E/W 與數字座標
        public bool IsElectrode { get; set; }
        public int Line { get; set; }   // 倉線/倉號
        public int Row { get; set; }   // 列
        public int Col { get; set; }   // 行
        public int Layer { get; set; } = 1;

        // ★ 計算屬性：E:1:row:col:layer / W:...
        public string SlotCode => $"{(IsElectrode ? "E" : "W")}:{Line}:{Row}:{Col}:{Layer}";
    }

    public class MaterialRef
    {
        public MaterialKind Kind { get; set; }
        public ElectrodeModel Electrode { get; set; }
        public WorkpieceModel Workpiece { get; set; }
        public IEnumerable<TimelineItemModel> Timeline { get; set; }
    }




public static class FakeDataFactory
    {
        // 固定種子，重建後資料可重現
        private static readonly Random Rng = new Random(24680);

        /// <summary>
        /// 依倉名自動判斷 ES=電極、W=工件，產生整個 StorageUnit（含 slots）
        /// </summary>
        public static StorageUnitViewModel CreateStorageUnit(string name, int rows, int cols)
        {
            var vm = new StorageUnitViewModel(name, rows, cols);
            vm.Slots.Clear();

            for (int i = 0; i < rows * cols; i++)
            {

                // 倉別
                bool isElectrode = name.StartsWith("ES", StringComparison.OrdinalIgnoreCase);

                // 先算座標與倉號
                int rowIndex = i / cols;
                int colIndex = i % cols;
                var digits = new string(name.Where(char.IsDigit).ToArray());
                int lineNo = int.TryParse(digits, out var n) ? n : 0;
                // 狀態與物料（保留你的原邏輯）
                var status = PickStatus();


                var slot = new StorageSlotViewModel
                {
                    // ★ 這些是計算 SlotCode 會用到的欄位
                    IsElectrode = isElectrode,
                    Line = lineNo,
                    Row = rowIndex + 1,
                    Col = colIndex + 1,
                    Layer = 1,

                    Status = status
                };

                if (slot.Status != "Empty")
                {
                    if (isElectrode)
                    {
                        var ele = GenerateElectrode(i, name);
                        var tl = GenerateElectrodeTimeline(slot.Status);
                        slot.Material = new MaterialRef
                        {
                            Kind = MaterialKind.Electrode,
                            Electrode = ele,
                            Timeline = tl
                        };
                    }
                    else
                    {
                        var wp = GenerateWorkpiece(i, name);
                        var tl = GenerateWorkpieceTimeline(slot.Status);
                        slot.Material = new MaterialRef
                        {
                            Kind = MaterialKind.Workpiece,
                            Workpiece = wp,
                            Timeline = tl
                        };
                    }
                }

                vm.Slots.Add(slot);
            }

            return vm;
        }


        /// <summary>
        /// 狀態分布：Working 30%、Verified 20%、Completed 20%、Reserved 15%、Error 5%、Empty 10%
        /// </summary>
        private static string PickStatus()
        {
            int p = Rng.Next(100);
            if (p < 30) return "Working";
            if (p < 50) return "Verified";
            if (p < 70) return "Completed";
            if (p < 85) return "Reserved";
            if (p < 90) return "Error";
            return "Empty";
        }

        // ===== 電極 =====
        private static ElectrodeModel GenerateElectrode(int index, string storageName)
        {
            // 產一點看起來像真的資料
            var types = new[] { "Square", "Round", "Custom" };
            return new ElectrodeModel
            {
                Name = $"ELE-{storageName}-{index:000}",
                No = $"E{DateTime.Now:MMdd}{index:000}",
                Type = types[Rng.Next(types.Length)],
                HolderNo = $"H{Rng.Next(1, 20):00}",
                TagSerial = $"RF-{Rng.Next(100000, 999999)}"
            };
        }

        private static IEnumerable<TimelineItemModel> GenerateElectrodeTimeline(string status)
        {
            var now = DateTime.Now;
            var list = new List<TimelineItemModel>();

            // 基礎節點
            list.Add(new TimelineItemModel { Text = "入庫", Time = now.AddHours(-8 - Rng.Next(6)), Status = "✓" });
            list.Add(new TimelineItemModel { Text = "檢驗完成", Time = now.AddHours(-6 - Rng.Next(6)), Status = "✓" });

            switch (status)
            {
                case "Verified":
                    list.Add(new TimelineItemModel { Text = "待派工", Time = now.AddHours(-2 - Rng.Next(2)), Status = "10%" });
                    break;

                case "Working":
                    // 隨機進度
                    var steps = new[] { "10%", "30%", "60%", "90%" };
                    int count = Rng.Next(1, steps.Length + 1);
                    for (int i = 0; i < count; i++)
                    {
                        list.Add(new TimelineItemModel
                        {
                            Text = "EDM 加工",
                            Time = now.AddMinutes(-(90 - i * 15) - Rng.Next(10)),
                            Status = steps[i]
                        });
                    }
                    break;

                case "Completed":
                    list.Add(new TimelineItemModel { Text = "EDM 完成", Time = now.AddHours(-1 - Rng.Next(2)), Status = "✓" });
                    list.Add(new TimelineItemModel { Text = "回庫上架", Time = now.AddMinutes(-15 - Rng.Next(15)), Status = "✓" });
                    break;

                case "Reserved":
                    list.Add(new TimelineItemModel { Text = "預約中", Time = now.AddHours(-1 - Rng.Next(3)), Status = "…" });
                    break;

                case "Error":
                    list.Add(new TimelineItemModel { Text = "加工異常（碰撞/放電異常）", Time = now.AddMinutes(-Rng.Next(60)), Status = "!" });
                    break;
            }

            return list.OrderBy(t => t.Time).ToList();
        }

        // ===== 工件 =====
        private static WorkpieceModel GenerateWorkpiece(int index, string storageName)
        {
            return new WorkpieceModel
            {
                Name = $"WP-{storageName}-{index:000}",
                No = $"W{DateTime.Now:MMdd}{index:000}",
                BatchNo = $"B{Rng.Next(1, 9)}{Rng.Next(100, 999)}",
                RouteNo = $"R-{Rng.Next(1, 5)}"
            };
        }

        private static IEnumerable<TimelineItemModel> GenerateWorkpieceTimeline(string status)
        {
            var now = DateTime.Now;
            var list = new List<TimelineItemModel>
        {
            new TimelineItemModel { Text = "入庫", Time = now.AddHours(-10 - Rng.Next(6)), Status = "✓" },
            new TimelineItemModel { Text = "檢驗完成", Time = now.AddHours(-7 - Rng.Next(3)), Status = "✓" }
        };

            switch (status)
            {
                case "Verified":
                    list.Add(new TimelineItemModel { Text = "待加工排程", Time = now.AddHours(-2 - Rng.Next(2)), Status = "10%" });
                    break;

                case "Working":
                    var steps = new[] { "10%", "40%", "70%" };
                    int count = Rng.Next(1, steps.Length + 1);
                    for (int i = 0; i < count; i++)
                    {
                        list.Add(new TimelineItemModel
                        {
                            Text = "加工中",
                            Time = now.AddMinutes(-(120 - i * 20) - Rng.Next(10)),
                            Status = steps[i]
                        });
                    }
                    break;

                case "Completed":
                    list.Add(new TimelineItemModel { Text = "加工完成", Time = now.AddHours(-1 - Rng.Next(2)), Status = "✓" });
                    list.Add(new TimelineItemModel { Text = "檢驗入庫", Time = now.AddMinutes(-20 - Rng.Next(20)), Status = "✓" });
                    break;

                case "Reserved":
                    list.Add(new TimelineItemModel { Text = "已預約工序", Time = now.AddHours(-1 - Rng.Next(3)), Status = "…" });
                    break;

                case "Error":
                    list.Add(new TimelineItemModel { Text = "加工異常（尺寸超差）", Time = now.AddMinutes(-Rng.Next(60)), Status = "!" });
                    break;
            }

            return list.OrderBy(t => t.Time).ToList();
        }
    }



}
