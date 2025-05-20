using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FoodRecommendationSystem.Core.Models
{
    /// <summary>
    /// 分类类，用于对商家和菜品进行分类
    /// </summary>
    public class Category
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 分类名称
        /// </summary> 
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 分类描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 分类类型
        /// </summary>
        public CategoryType Type { get; set; }

        /// <summary>
        /// 父分类ID(用于层级分类)
        /// </summary>
        public string? ParentCategoryId { get; set; }

        /// <summary>
        /// 分类图标URL
        /// </summary>
        public string? IconUrl { get; set; }

        /// <summary>
        /// 排序权重
        /// </summary>
        public int SortOrder { get; set; }

        // 添加默认构造函数
        public Category() { }

        // 添加带参数的构造函数，便于创建新分类
        public Category(string name, string description, CategoryType type, string? parentCategoryId = null, string? iconUrl = null, int sortOrder = 0)
        {
            Name = name;
            Description = description;
            Type = type;
            ParentCategoryId = parentCategoryId;
            IconUrl = iconUrl;
            SortOrder = sortOrder;
        }
    }

    /// <summary>
    /// 分类类型枚举
    /// </summary>
    public enum CategoryType
    {
        Cuisine,      // 菜系(如川菜、粤菜)
        DishType,     // 菜品类型(如主食、小吃)
        MerchantType  // 商家类型(如火锅店、烧烤店)
    }
}