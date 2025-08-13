using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Interfaces;
using FMSFrontend.Services;
using FMSFrontend.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace FMSFrontend.ViewModels
{
    public partial class WorkOrderPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private int selectedTabIndexParameter;
        [ObservableProperty]
        private int eDMSelectedTab; // 決定 EDM 子頁籤

        public List<string> DateFilterOptions { get; set; } = new() { "今天", "過去7天", "自訂" };
        [ObservableProperty]
        private string selectedFilterOption = "今天";

        private DateTime? lastValidFromDate = DateTime.Today;
        private DateTime? lastValidToDate = DateTime.Today;

        private readonly IWindowService _windowService;

        partial void OnSelectedFilterOptionChanged(string value)
        {
            OnPropertyChanged(nameof(IsCustomDateMode));
            ApplyDateFilter();
        }
        [ObservableProperty]
        private DateTime? fromDate = DateTime.Today;

        partial void OnFromDateChanged(DateTime? value)
        {
            if (value == null || ToDate == null)
            {
                lastValidFromDate = value;
                return;
            }

            if (value > ToDate)
            {
                _windowService.ShowMessage("開始日期不能大於結束日期");
                FromDate = lastValidFromDate;
                return;
            }

            if ((ToDate - value)?.TotalDays > 31)
            {
                _windowService.ShowMessage("選擇的日期範圍不能超過一個月");
                FromDate = lastValidFromDate;
                return;
            }

            lastValidFromDate = value;
        }

        [ObservableProperty]
        private DateTime? toDate = DateTime.Today;

        partial void OnToDateChanged(DateTime? value)
        {
            if (value == null || FromDate == null)
            {
                lastValidToDate = value;
                return;
            }

            if (value < FromDate)
            {
                ShowWarning("結束日期不能小於開始日期");
                ToDate = lastValidToDate;
                return;
            }

            if ((value - FromDate)?.TotalDays > 31)
            {
                ShowWarning("選擇的日期範圍不能超過一個月");
                ToDate = lastValidToDate;
                return;
            }

            lastValidToDate = value;
        }
        public bool IsCustomDateMode => SelectedFilterOption == "自訂";

        public ObservableCollection<WorkOrderData> WorkOrderList { get; set; }
        public ObservableCollection<WorkOrderData> FailureWorkOrders { get; set; } = new();


        private void ApplyDateFilter()
        {
            switch (SelectedFilterOption)
            {
                case "今天":
                    ToDate = DateTime.Today;
                    FromDate = DateTime.Today;
                    break;
                case "過去7天":
                    ToDate = DateTime.Today;
                    FromDate = DateTime.Today.AddDays(-6); // 包含今天一共7天
                   
                    break;
                case "自訂":
                default:
                    break;
            }
        }
        public ICommand DeleteCommand { get; }

        public WorkOrderPageViewModel(IWindowService windowService)
        {
            _windowService = windowService;

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

        private void ShowWarning(string message)
        {
            FMSFrontend.Extensions.DialogMessageWindow dd = new Extensions.DialogMessageWindow(message);
            dd.Show();
        }
        [RelayCommand]
        private void OpenUploadSheet()
        {
            _windowService.ShowUploadSheetWindow();
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
