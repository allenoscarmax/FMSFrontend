

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class StoragePresets
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string StorageName {  get; set; }
        public string StorageNumber {  get; set; }
        public int RegionNumber {  get; set; }
        public int ColumnCount {  get; set; }
        public int RowCount { get; set; }
        public string Note {  get; set; }
    }
}
