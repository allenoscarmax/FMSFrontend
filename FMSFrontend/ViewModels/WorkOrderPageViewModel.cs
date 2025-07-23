using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace FMSFrontend.ViewModels
{
    public partial class WorkOrderPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private int selectedTabIndexParameter;
        public ObservableCollection<WorkOrderData> WorkOrderList { get; set; }

        public ICommand DeleteCommand { get; }

        public WorkOrderPageViewModel()
        {
            WorkOrderList = new ObservableCollection<WorkOrderData>
            {
                new WorkOrderData
                {
                    OrderNumber = "20250722001",
                    PartName = "24-018-003",
                    Status = "NEW",
                    StatusColor = Brushes.Gold,
                    
                    TargetEDM = "EDM1",
                    Coordinate = "G01",
                    Operator = "admin",
                    
                    EDMDetails =new ObservableCollection<EDMDetail>
                    {
                        new EDMDetail { ElectrodeName = "E01", LabelSerial = "A123", Status = "待機", NeedEDM = true, EDMProgram = "P1", Offset = 1 },
                        new EDMDetail { ElectrodeName = "E02", LabelSerial = "A124", Status = "加工中", NeedEDM = false, EDMProgram = "P2", Offset = 2 },
                    }
                },
                new WorkOrderData
                {
                    OrderNumber = "20250722001",
                    PartName = "24-018-003",
                    Status = "NEW",
                    StatusColor = Brushes.Gold,

                    TargetEDM = "EDM1",
                    Coordinate = "G01",
                    Operator = "admin",

                    EDMDetails =new ObservableCollection<EDMDetail>
                    {
                        new EDMDetail { ElectrodeName = "E01", LabelSerial = "A123", Status = "待機", NeedEDM = true, EDMProgram = "P1", Offset = 1 },
                        new EDMDetail { ElectrodeName = "E02", LabelSerial = "A124", Status = "加工中", NeedEDM = false, EDMProgram = "P2", Offset = 2 },
                    }
                },
                new WorkOrderData
                {
                    OrderNumber = "20250722001",
                    PartName = "24-018-003",
                    Status = "NEW",
                    StatusColor = Brushes.Gold,

                    TargetEDM = "EDM1",
                    Coordinate = "G01",
                    Operator = "admin",

                    EDMDetails =new ObservableCollection<EDMDetail>
                    {
                        new EDMDetail { ElectrodeName = "E01", LabelSerial = "A123", Status = "待機", NeedEDM = true, EDMProgram = "P1", Offset = 1 },
                        new EDMDetail { ElectrodeName = "E02", LabelSerial = "A124", Status = "加工中", NeedEDM = false, EDMProgram = "P2", Offset = 2 },
                        new EDMDetail { ElectrodeName = "E01", LabelSerial = "A123", Status = "待機", NeedEDM = true, EDMProgram = "P1", Offset = 1 },
                        new EDMDetail { ElectrodeName = "E02", LabelSerial = "A124", Status = "加工中", NeedEDM = false, EDMProgram = "P2", Offset = 2 },
                        new EDMDetail { ElectrodeName = "E01", LabelSerial = "A123", Status = "待機", NeedEDM = true, EDMProgram = "P1", Offset = 1 },
                        new EDMDetail { ElectrodeName = "E02", LabelSerial = "A124", Status = "加工中", NeedEDM = false, EDMProgram = "P2", Offset = 2 },
                        new EDMDetail { ElectrodeName = "E01", LabelSerial = "A123", Status = "待機", NeedEDM = true, EDMProgram = "P1", Offset = 1 },
                        new EDMDetail { ElectrodeName = "E02", LabelSerial = "A124", Status = "加工中", NeedEDM = false, EDMProgram = "P2", Offset = 2 },
                    }
                },
                new WorkOrderData
                {
                    OrderNumber = "20250722001",
                    PartName = "24-018-003",
                    Status = "NEW",
                    StatusColor = Brushes.Gold,

                    TargetEDM = "EDM1",
                    Coordinate = "G01",
                    Operator = "admin",

                    EDMDetails =new ObservableCollection<EDMDetail>
                    {
                        new EDMDetail { ElectrodeName = "E01", LabelSerial = "A123", Status = "待機", NeedEDM = true, EDMProgram = "P1", Offset = 1 },
                        new EDMDetail { ElectrodeName = "E02", LabelSerial = "A124", Status = "加工中", NeedEDM = false, EDMProgram = "P2", Offset = 2 },
                    }
                },
                new WorkOrderData
                {
                    OrderNumber = "20250722002",
                    PartName = "24-018-004",
                    Status = "Completed",
                    StatusColor = Brushes.DeepSkyBlue,
                    TargetEDM = "EDM2",
                    Coordinate = "G02",
                    Operator = "admin",
                    CurrentStage = 1,
                    TotalStage = 20,
                    EDMDetails = new ObservableCollection<EDMDetail>
                    {
                        new EDMDetail { ElectrodeName = "E03", LabelSerial = "B001", Status = "完成", NeedEDM = true, EDMProgram = "P3", Offset = 3 },
                    }
                }

            };


            DeleteCommand = new RelayCommand<WorkOrderData>(item =>
            {
                if (item != null)
                    WorkOrderList.Remove(item);
            });
        }
    }
    public class WorkOrderData : INotifyPropertyChanged
    {
        public string OrderNumber { get; set; }
        public string PartName { get; set; }
        public string Status { get; set; }
        public Brush StatusColor { get; set; }
        public string TargetEDM { get; set; }
        public string Coordinate { get; set; }
        public string Operator { get; set; }
        public int CurrentStage { get; set; } = 1;
        public int TotalStage { get; set; } = 3;

        // 進度條百分比（回傳 double）
        public double EDMStageProgress => TotalStage == 0 ? 0 : (100.0 * CurrentStage / TotalStage);

        // 顯示文字，如 "EDM加工階段：1 / 3"
        public string EDMStageDisplay => $"EDM加工階段：{CurrentStage} / {TotalStage}";

        public ObservableCollection<EDMDetail> EDMDetails { get; set; } = new ObservableCollection<EDMDetail>();


        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class EDMDetail 
    {
        public string ElectrodeName { get; set; }
        public string LabelSerial { get; set; }
        public string Status { get; set; }
        public bool NeedEDM { get; set; }
        public string EDMProgram { get; set; }
        public int Offset { get; set; }



    }
}
