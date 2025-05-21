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
    /// 评价类，存储从各平台爬取的评价信息
    /// </summary>
    public class Review
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 关联商家ID
        /// </summary>
        public string MerchantId { get; set; } = string.Empty;

        /// <summary>
        /// 关联菜品ID(可选)
        /// </summary>
        public string DishId { get; set; } = string.Empty;

        /// <summary>
        /// 评价来源平台
        /// </summary>
        public string PlatformName { get; set; } = string.Empty;

        /// <summary>
        /// 平台上的评价ID
        /// </summary>
        public string PlatformReviewId { get; set; } = string.Empty;

        /// <summary>
        /// 评论者名称
        /// </summary>
        public string ReviewerName { get; set; } = string.Empty;

        /// <summary>
        /// 评价内容
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 评分(1-5)
        /// </summary>
        public double Rating { get; set; }

        /// <summary>
        /// 评价日期
        /// </summary>
        public DateTime ReviewDate { get; set; }

        /// <summary>
        /// 点赞数
        /// </summary>
        public int LikesCount { get; set; }

        /// <summary>
        /// 评价图片URL列表
        /// </summary>
        public List<string> ImageUrls { get; set; } = new List<string>();

        /// <summary>
        /// 商家回复
        /// </summary>
        public string MerchantReply { get; set; } = string.Empty;
    }
}