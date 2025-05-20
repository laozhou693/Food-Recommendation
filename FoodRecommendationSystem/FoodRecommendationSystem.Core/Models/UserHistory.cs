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
    /// 用户历史记录类，用于记录用户行为
    /// </summary>
    public class UserHistory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 关联用户ID
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 关联商家ID(可选)
        /// </summary>
        public string MerchantId { get; set; } = string.Empty;

        /// <summary>
        /// 关联菜品ID(可选)
        /// </summary>
        public string DishId { get; set; } = string.Empty;

        /// <summary>
        /// 历史记录时间
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 历史记录类型
        /// </summary>
        public HistoryType Type { get; set; }

        /// <summary>
        /// 搜索关键词(当Type为Search时有值)
        /// </summary>
        public string SearchKeyword { get; set; }= string.Empty;

        /// <summary>
        /// 用户所在位置(当记录地理位置相关操作时有值)
        /// </summary>
        public GeoLocation UserLocation { get; set; } = new GeoLocation();
    }

    /// <summary>
    /// 历史记录类型枚举
    /// </summary>
    public enum HistoryType
    {
        View,       // 浏览
        Search,     // 搜索
        Favorite,   // 收藏
        Unfavorite, // 取消收藏
        Click,      // 点击
        Share       // 分享
    }
}