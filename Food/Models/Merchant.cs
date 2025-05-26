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
        [BsonElement("ratingEle")]
        public double RatingEle { get; set; }
        [BsonElement("ratingMeituan")]
        public double RatingMeituan { get; set; }

        [BsonElement("distance")]
        public int Distance { get; set; } // 距离（米）

        [BsonElement("image")]
        public string Image { get; set; } = string.Empty;

        [BsonElement("price")]
        public string Price { get; set; } = string.Empty;

        [BsonElement("priceEle")]
        public string PriceEle { get; set; } = string.Empty;

        [BsonElement("priceMeituan")]
        public string PriceMeituan { get; set; } = string.Empty;

        [BsonElement("catogery")]
        public List<string> Tags { get; set; } = new List<string>(); // 分类标签

        [BsonElement("favoriteCount")]
        public int FavoriteCount { get; set; } = 0; // 被收藏次数

        [BsonElement("recentOrderNum")]
        public string RecentOrderNum { get; set; } = string.Empty;

        [BsonElement("lowestCost")]
        public int LowestCost { get; set; }
        [BsonElement("deliveryFee")]
        public string DeliveryFee { get; set; } = string.Empty;

        [BsonElement("deliveryMode")]
        public string DeliveryMode { get; set; } = string.Empty;
        [BsonElement("orderLeadTime")]
        public int OrderLeadTime { get; set; }

        [BsonElement("reasons")]
        public List<string> Reasons { get; set; } = new List<string>();
    }
}
