using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodRecommendationSystem.Core.Models
{
    /// <summary>
    /// 用户注册请求的数据传输对象（DTO）
    /// </summary>
    public class RegisterDto
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
    }
}

