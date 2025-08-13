using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Extensions;
using FMSFrontend.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FMSFrontend.ViewModels
{
    public partial class RFIDBindPageViewModel : ObservableObject
    {
        public ObservableCollection<BurnRecord> BurnHistoryList { get; set; }
        public List<string> DateFilterOptions { get; set; } = new() { "今天", "過去7天", "自訂" };
        [ObservableProperty]
        private string selectedFilterOption = "今天";

        private DateTime? lastValidFromDate = DateTime.Today;
        private DateTime? lastValidToDate = DateTime.Today;

        private readonly IWindowService _windowService;
        public bool IsCustomDateMode => SelectedFilterOption == "自訂";
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

        public RFIDBindPageViewModel(IWindowService windowService)
        {
            _windowService = windowService;
            BurnHistoryList = new ObservableCollection<BurnRecord>
        {
            new BurnRecord
            {
                Time = DateTime.Now.AddMinutes(-10),
                MaterialType = "電極",
                SerialNo = "SN-00123",
                TagSerial = "TAG-A1001"
            },
            new BurnRecord
            {
                Time = DateTime.Now.AddMinutes(-30),
                MaterialType = "工件",
                SerialNo = "SN-00124",
                TagSerial = "TAG-B2001"
            },
            new BurnRecord
            {
                Time = DateTime.Now.AddHours(-1),
                MaterialType = "治具",
                SerialNo = "SN-00125",
                TagSerial = "TAG-C3001"
            },
            new BurnRecord
            {
                Time = DateTime.Today.AddHours(-5),
                MaterialType = "工件",
                SerialNo = "SN-00126",
                TagSerial = "TAG-B2002"
            },
            new BurnRecord
            {
                Time = DateTime.Today.AddDays(-1).AddHours(2),
                MaterialType = "電極",
                SerialNo = "SN-00127",
                TagSerial = "TAG-A1002"
            }
        };

        }
        [RelayCommand]
        private void OpenMaterialTypeSelect()
        {
            if (_windowService.ShowMaterialTypeSelectWindow(out bool isElectrode))
            {
                var pairWindow = new MaterialPairWindow(isElectrode)
                {

                   // DataContext = new MaterialPairViewModel(isElectrode)
                };
                pairWindow.ShowDialog();
            }

        }

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
        private void ShowWarning(string message)
        {
            FMSFrontend.Extensions.DialogMessageWindow dd = new Extensions.DialogMessageWindow(message);
            dd.Show();
        }

        public class BurnRecord
        {
            public DateTime Time { get; set; }
            public string MaterialType { get; set; }
            public string SerialNo { get; set; }
            public string TagSerial { get; set; }
        }
    }
}
