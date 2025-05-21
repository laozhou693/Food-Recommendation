using System;

namespace FoodRecommendationSystem.Core.Models
{
    /// <summary>
    /// 用户登录请求的数据传输对象（DTO）
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; }

        public string Password { get; set; }
    }
}
