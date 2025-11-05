using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace OSCARMAXFMS_V3.DBmodels
{
    public class OperationPresets // 作業預設（工序模板/標準作業定義）
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = ""; // ID（MongoDB 物件識別碼）
        public string operationNumber { get; set; } = "";// 作業/工序代碼（模板編號）
        public string operationStructure {  get; set; } = ""; // 作業結構（JSON/DSL；定義步驟、順序、參數等）
        public string machiningWorkpieceName { get; set; } = ""; // 加工工件名稱（模板適用之品名/類別）
        public string setupUser {  get; set; } = ""; // 建置/設定人員（帳號或姓名）
    }
}
