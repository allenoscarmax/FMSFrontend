using CommunityToolkit.Mvvm.ComponentModel;
using ControlzEx.Standard;
using FMSFrontend.Controls;
using FMSFrontend.Features.Dtos;
using System.Collections.Generic;
using System.Collections.ObjectModel;
namespace FMSFrontend.Models
{
    public class WorkpieceModel
    {
        public string Id { get; set; } = "";
        public string JigSerial { get; set; } = "";   // 治具序號
        public string Name { get; set; } = "";         // 工件/零件名稱
        public string No { get; set; } = "";          // 工件編號
        public string WorkType { get; set; } = "";    // 工作類型
        public string PartNo { get; set; } = "";    // 零件件號
        public string OrderNo { get; set; } = "";     // 工單序號
        public string ClampNo { get; set; } = "";      // 夾釘器編號
        public string Status { get; set; } = "";      // 狀態
        public string BatchNo { get; set; } = "";    // 批號
        public string PartName { get; set; } = "";    // 零件名稱
        public string SerialCode { get; set; } = "";   // 序號順序碼
        public string RouteNo { get; set; } = "";     // 途程號碼
        public bool WorkRestriction { get; set; }        //限制/鎖定（DB 為 bool）
        public string StoragStatus { get; set; } = "";// 新增： Storage 限制/鎖定
        public bool StorageRestriction { get; set; }        //限制/鎖定（DB 為 bool）
    }
}



















