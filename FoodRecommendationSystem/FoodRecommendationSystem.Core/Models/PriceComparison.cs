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
    /// 跨平台价格比较类，用于比较同一菜品在不同平台的价格
    /// </summary>
    public class PriceComparison
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 平台名称(如美团、饿了么等)
        /// </summary>
        public string PlatformName { get; set; } = string.Empty;

        /// <summary>
        /// 关联菜品ID
        /// </summary>
        public string DishId { get; set; } = string.Empty;

        /// <summary>
        /// 当前价格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 价格最后更新时间
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// 平台菜品详情页URL
        /// </summary>
        public string PlatformUrl { get; set; } = string.Empty;

        /// <summary>
        /// 是否有折扣
        /// </summary>
        public bool HasDiscount { get; set; }

        /// <summary>
        /// 原始价格(无折扣时的价格)
        /// </summary>
        public decimal OriginalPrice { get; set; }

        /// <summary>
        /// 平台专属优惠(如满减、优惠券等描述)
        /// </summary>
        public string DiscountDescription { get; set; } = string.Empty;
    }
}