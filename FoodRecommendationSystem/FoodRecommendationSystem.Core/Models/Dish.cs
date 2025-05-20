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
    /// 菜品信息类，包含菜品基本信息和跨平台价格
    /// </summary>
    public class Dish
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string MerchantId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 菜品分类(如川菜、甜点等)
        /// </summary>
        public List<string> Categories { get; set; } = new List<string>();

        /// <summary>
        /// 菜品标签(如辣、甜、素食等)
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// 跨平台价格对比数据
        /// </summary>
        public List<PriceComparison> Prices { get; set; } = new List<PriceComparison>();

        /// <summary>
        /// 菜品平均评分
        /// </summary>
        public double Rating { get; set; }

        /// <summary>
        /// 是否为招牌菜
        /// </summary>
        public bool IsSignatureDish { get; set; }

        /// <summary>
        /// 菜品图片URL列表
        /// </summary>
        public List<string> ImageUrls { get; set; } = new List<string>();

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// 月销量
        /// </summary>
        public int MonthlySales { get; set; }
    }
}