using CommunityToolkit.Mvvm.ComponentModel;
using ControlzEx.Standard;
using FMSFrontend.Controls;
using FMSFrontend.Features.Dtos;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq; // ← for PageList
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
namespace FMSFrontend.Models
{
    public partial class MachineModel : ObservableObject
    {
        [ObservableProperty] private string machineName = ""; //機台名稱
        [ObservableProperty] private string status = "";
        [ObservableProperty] private string type = ""; //機台類型
        [ObservableProperty] private bool restriction; //機台鎖定
        public string onDeckElectrodeSerial { get; set; } = "";// 夾持中電極標籤序號（RFID）
        public string onDeckWorkpieceSerial { get; set; } = "";// 夾持中工件標籤序號（RFID）
        public string onDeckWorksheetSerial { get; set; } = "";// 當前工單號/序號  

    }
}



















