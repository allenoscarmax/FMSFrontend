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
namespace FMSFrontend.Models
{
    public class ElectrodeModel
    {
        public string Id { get; set; } = "";
        public string JigSerial { get; set; } = "";//治具序號
        public string Name { get; set; } = "";//名稱
        public string No { get; set; } = ""; //編號
        public string Type { get; set; } = "";  // 方/圓/自定
        public string Status { get; set; } = ""; //狀態
        public string HolderNo { get; set; } = "";   // 夾具代號
        public string TagSerial { get; set; } = "";   // RFID 
        public string MaxDischargeCount { get; set; } = ""; //最大放電次數
        public string UsageRate { get; set; } = "";//電極使用率
        public string Compensation { get; set; } = "";//電極補償值
        public string ProcessedCount { get; set; } = "";//被加工次數
        public string StoragStatus { get; set; } = "";// 新增： Storage 狀態
        public bool ElecRestriction { get; set; } // 新增：工件或電極 限制/鎖定
        public string StorageId { get; set; } = ""; // 新增： Storage 限制/鎖定
        public bool StorageRestriction { get; set; } // 新增： Storage 限制/鎖定
    }
}



















