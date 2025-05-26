using System.ComponentModel.DataAnnotations;

namespace Food.Models
{

        /// <summary>
        /// 用户注册请求模型
        /// </summary>
        public class RegisterModel
        {
            /// <summary>
            /// 用户名
            /// </summary>
            [Required]
            [StringLength(20, MinimumLength = 3, ErrorMessage = "用户名长度必须在3-20个字符之间")]
            public string Username { get; set; } = string.Empty;

            /// <summary>
            /// 密码
            /// </summary>
            [Required]
            [StringLength(50, MinimumLength = 6, ErrorMessage = "密码长度必须在6-50个字符之间")]
            public string Password { get; set; } = string.Empty;
        }

        /// <summary>
        /// 登录请求模型
        /// </summary>
        public class LoginModel
        {
            /// <summary>
            /// 用户名
            /// </summary>
            [Required]
            public string Username { get; set; } = string.Empty;

            /// <summary>
            /// 密码
            /// </summary>
            [Required]
            public string Password { get; set; } = string.Empty;
        }

        /// <summary>
        /// 注册响应模型
        /// </summary>
        public class RegisterResponse
        {
            /// <summary>
            /// 操作结果消息
            /// </summary>
            public string Message { get; set; } = string.Empty;

            /// <summary>
            /// 用户信息
            /// </summary>
            public required UserDto User { get; set; }
        }

        /// <summary>
        /// 登录响应模型
        /// </summary>
        public class LoginResponse
        {
            /// <summary>
            /// 操作结果消息
            /// </summary>
            public string Message { get; set; } = string.Empty;

            /// <summary>
            /// JWT 令牌
            /// </summary>
            public string Token { get; set; } = string.Empty;

            /// <summary>
            /// 用户信息
            /// </summary>
            public required UserDto User { get; set; }
        }

        /// <summary>
        /// 用户信息传输对象
        /// </summary>
        public class UserDto
        {
            /// <summary>
            /// 用户 ID
            /// </summary>
            public string Id { get; set; } = string.Empty;

            /// <summary>
            /// 用户名
            /// </summary>
            public string Username { get; set; } = string.Empty;
        }

        /// <summary>
        /// 错误响应模型
        /// </summary>
        public class ErrorResponse
        {
            /// <summary>
            /// 错误消息
            /// </summary>
            public string Message { get; set; } = string.Empty;
        }
    }
