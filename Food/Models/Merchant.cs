using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Food.Models
{
    public class Merchant
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }=string.Empty;

        [BsonElement("name")]
        public string Name { get; set; }=string.Empty ;

        [BsonElement("address")]
        public string Address { get; set; } = string.Empty;

        [BsonElement("businessHours")]
        public string BusinessHours { get; set; } = string.Empty;

        [BsonElement("rating")]
        public double Rating { get; set; }

        [BsonElement("distance")]
        public int Distance { get; set; } // 距离（米）

        [BsonElement("image")]
        public string Image { get; set; } = string.Empty;

        [BsonElement("price")]
        public string Price { get; set; } = string.Empty;

        [BsonElement("tags")]
        public List<string> Tags { get; set; } = new List<string>(); // 分类标签

        [BsonElement("favoriteCount")]
        public int FavoriteCount { get; set; } = 0; // 被收藏次数
    }
}
