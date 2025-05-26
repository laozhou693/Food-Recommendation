using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Collections.Generic;

namespace Food.Models
{
    public class Category
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }=string.Empty;

        [BsonElement("Name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("type")]
        public CategoryType Type { get; set; }

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("sortOrder")]
        public int SortOrder { get; set; }
    }

    public enum CategoryType
    {
        Cuisine,      // 菜系
        MerchantType, // 商家类型
        DishType      // 菜品类型
    }
}
