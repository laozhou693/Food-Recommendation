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
    /// 商家信息类，包含商家详细信息和关联菜品
    /// </summary>
    public class Merchant
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 商家名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 商家描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 详细地址
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 联系电话
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// 营业时间
        /// </summary>
        public string BusinessHours { get; set; } = string.Empty;

        /// <summary>
        /// 地理位置
        /// </summary>
        public GeoLocation Location { get; set; } = new GeoLocation();

        /// <summary>
        /// 商家分类列表
        /// </summary>
        public List<string> Categories { get; set; } = new List<string>();

        /// <summary>
        /// 商家菜品列表
        /// </summary>
        public List<Dish> Dishes { get; set; } = new List<Dish>();

        /// <summary>
        /// 商家评分(1-5)
        /// </summary>
        public double Rating { get; set; }

        /// <summary>
        /// 评价数量
        /// </summary>
        public int ReviewCount { get; set; }

        /// <summary>
        /// 各平台信息
        /// </summary>
        public List<PlatformInfo> PlatformInfos { get; set; } = new List<PlatformInfo>();

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// 商家图片URL列表
        /// </summary>
        public List<string> ImageUrls { get; set; } = new List<string>();

        /// <summary>
        /// 所在区域
        /// </summary>
        public Region Region { get; set; }

        /// <summary>
        /// 人均消费(元)
        /// </summary>
        public decimal AveragePrice { get; set; }

        /// <summary>
        /// 特色标签(如"24小时营业", "免费WiFi"等)
        /// </summary>
        public List<string> Features { get; set; } = new List<string>();

        /// <summary>
        /// 临时存储爬取的评价，不会存储到数据库
        /// </summary>
        [BsonIgnore]
        public List<Review> Reviews { get; set; } = new List<Review>();
    }

    /// <summary>
    /// 平台信息类，用于存储商家在各平台的数据
    /// </summary>
    public class PlatformInfo
    {
        /// <summary>
        /// 平台名称(如美团、饿了么等)
        /// </summary>
        public string PlatformName { get; set; } = string.Empty;

        /// <summary>
        /// 平台上的商家ID
        /// </summary>
        public string PlatformMerchantId { get; set; } = string.Empty;

        /// <summary>
        /// 平台商家页面URL
        /// </summary>
        public string PlatformUrl { get; set; } = string.Empty;

        /// <summary>
        /// 平台上的评分
        /// </summary>
        public double PlatformRating { get; set; }

        /// <summary>
        /// 平台上的评价数量
        /// </summary>
        public int PlatformReviewCount { get; set; }

        /// <summary>
        /// 平台特有活动信息
        /// </summary>
        public string PromotionInfo { get; set; } = string.Empty;
    }

    /// <summary>
    /// 武汉市区域枚举
    /// </summary>
    public enum Region
    {
        WuchangDistrict,  // 武昌区
        HanKouDistrict,   // 汉口区
        HanYangDistrict,  // 汉阳区
        QingshanDistrict, // 青山区
        HongShanDistrict, // 洪山区
        JiangXiaDistrict, // 江夏区
        JiangHanDistrict  // 江汉区
    }
}