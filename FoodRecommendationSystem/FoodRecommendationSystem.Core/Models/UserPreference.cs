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
    /// 用户偏好设置类，用于个性化推荐
    /// </summary>
    public class UserPreference
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 关联用户ID
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 喜好的菜系列表
        /// </summary>
        public List<string> FavoriteCuisines { get; set; } = new List<string>();

        /// <summary>
        /// 价格偏好
        /// </summary>
        public PricePreference PricePreference { get; set; } = PricePreference.Any;

        /// <summary>
        /// 饮食限制(如素食、无麸质等)
        /// </summary>
        public List<string> DietaryRestrictions { get; set; } = new List<string>();

        /// <summary>
        /// 最大接受距离(公里)
        /// </summary>
        public double MaxDistance { get; set; } = 5.0;

        /// <summary>
        /// 常去区域列表
        /// </summary>
        public List<string> FrequentRegions { get; set; } = new List<string>();

        /// <summary>
        /// 最近搜索关键词
        /// </summary>
        public List<string> RecentSearches { get; set; } = new List<string>();

        /// <summary>
        /// 喜好口味(如辣、酸、甜等)
        /// </summary>
        public List<string> FavoriteTastes { get; set; } = new List<string>();

        /// <summary>
        /// 用餐时段偏好
        /// </summary>
        public List<MealTime> PreferredMealTimes { get; set; } = new List<MealTime>();
    }

    /// <summary>
    /// 价格偏好枚举
    /// </summary>
    public enum PricePreference
    {
        Budget,        // 经济(人均 ≤ 30元)
        Moderate,      // 适中(人均 31-80元)
        Expensive,     // 高端(人均 81-150元)
        VeryExpensive, // 奢华(人均 > 150元)
        Any            // 不限
    }

    /// <summary>
    /// 用餐时段枚举
    /// </summary>
    public enum MealTime
    {
        Breakfast,  // 早餐
        Lunch,      // 午餐
        Dinner,     // 晚餐
        LateNight   // 夜宵
    }
}