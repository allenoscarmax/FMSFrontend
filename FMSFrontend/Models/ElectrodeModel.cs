
namespace FMSFrontend.Models
{
    public class ElectrodeModel
    {
        
        public string Id { get; set; } = "";                //唯一識別碼
        public string Name { get; set; } = "";              //名稱
        public string No { get; set; } = "";                //編號
        public string Type { get; set; } = "";              // 方/圓/自定
        public string Status { get; set; } = "";            //狀態
        public string TagSerial { get; set; } = "";         // RFID 
        public string Compensation { get; set; } = "";      //電極補償值
        public string ProcessedCount { get; set; } = "";    //被加工次數
        public string MaxDischargeCount { get; set; } = ""; //最大放電次數
        public string SlotCode { get; set; } = "";          //位置編碼
        public string Program { get; set; } = "";           //加工程式


        //材料資訊才有
        public bool ElecRestriction { get; set; }           // 工件或電極 限制/鎖定
        public string StorageId { get; set; } = "";         // Storage 限制/鎖定
        public string StoragStatus { get; set; } = "";      // Storage 狀態
        public bool StorageRestriction { get; set; }        // Storage 限制/鎖定

        //庫存資訊才有
        public string Location { get; set; } = "";            //位置編碼
        
        //public string JigSerial { get; set; } = "";         //治具序號
        //public string HolderNo { get; set; } = "";          // 夾具代號
        //public string UsageRate { get; set; } = "";         //電極使用率
    }
}



















