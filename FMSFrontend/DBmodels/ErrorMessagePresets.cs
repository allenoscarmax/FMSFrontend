using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class ErrorMessagePresets
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string ErrorCode {  get; set; }
        public string Message_cn { get; set; }
        public string Message_en { get; set; }

    }
}
