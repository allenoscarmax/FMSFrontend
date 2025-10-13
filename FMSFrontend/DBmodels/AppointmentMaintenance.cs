using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class AppointmentMaintenance
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string Maintenance { get; set; }   //保養設備或名稱
        public string ProductLine {  get; set; }  //第x條產線
        public int Index {  get; set; } //第幾台
        public string Type {  get; set; }   //保養的type
        public List<int> Day_values { get; set; }  // 例如 [1, 3, 5] Day_value {  get; set; }  //weekly : 1:日 ~ 6:六 ; monthly : 1~31 (第幾天)
        public int Hour { get; set; } // 幾點 (時)   記得存24時制
        public int Minute { get; set; } // 幾點(分)
        public bool IsEnabled { get; set; } = true; // 預設啟用
        public string Note {  get; set; } // 說明
    }
}
