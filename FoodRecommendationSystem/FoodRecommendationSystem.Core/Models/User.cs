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
    /// 用户信息类，包含基本用户信息和偏好设置
    /// </summary>
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 用户名，唯一
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 电子邮箱
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 密码哈希值
        /// </summary>
        public string? PasswordHash { get; set; }

        /// <summary>
        /// 注册日期
        /// </summary>
        public DateTime RegisterDate { get; set; }

        /// <summary>
        /// 最后登录日期
        /// </summary>
        public DateTime LastLoginDate { get; set; }

        /// <summary>
        /// 用户偏好设置
        /// </summary>
        public UserPreference Preference { get; set; } = new UserPreference();

        /// <summary>
        /// 收藏的商家ID列表
        /// </summary>
        public List<string> FavoriteMerchants { get; set; } = new List<string>();

        /// <summary>
        /// 收藏的菜品ID列表
        /// </summary>
        public List<string> FavoriteDishes { get; set; } = new List<string>();

        /// <summary>
        /// 用户头像URL
        /// </summary>
        public string AvatarUrl { get; set; } = string.Empty;

        /// <summary>
        /// 用户手机号码
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;
    }
}