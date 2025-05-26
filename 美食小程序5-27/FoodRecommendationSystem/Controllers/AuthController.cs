using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FoodRecommendationSystem.Core.Models;
using FoodRecommendationSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace FoodRecommendationSystem.API.Controllers
{
    //身份验证控制器
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUserService userService,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _userService = userService;
            _configuration = configuration;
            _logger = logger;
        }

        // 用户登录
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // 利用ModelState自动验证
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // 验证用户凭据
                var user = await _userService.ValidateUserAsync(
                    request.Username,
                    request.Email,
                    request.Password);

                if (user == null)
                {
                    return Unauthorized("用户名或密码错误");
                }

                // 生成JWT令牌
                var token = GenerateJwtToken(user);

                // 返回用户信息和令牌
                return Ok(new
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登录过程中发生错误");
                return StatusCode(500, "登录失败，请稍后再试");
            }
        }

        //用户注册
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // 利用ModelState自动验证
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // 创建用户
                var newUser = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    Preference = new UserPreference
                    {
                        FavoriteCuisines = new List<string>(),
                        FrequentRegions = new List<string>(),
                        PricePreference = PricePreference.Moderate
                    }
                };

                var result = await _userService.CreateUserAsync(newUser, request.Password);

                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }

                // 生成JWT令牌
                var token = GenerateJwtToken(newUser);

                // 返回用户信息和令牌
                return Ok(new
                {
                    UserId = newUser.Id,
                    Username = newUser.Username,
                    Email = newUser.Email,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "注册过程中发生错误");
                return StatusCode(500, "注册失败，请稍后再试");
            }
        }

        // 生成JWT令牌
        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    //登录请求模型
    public class LoginRequest
    {
        [Required(ErrorMessage = "请提供用户名或邮箱")]
        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "密码不能为空")]
        public string Password { get; set; } = string.Empty;
    }

    // 注册请求模型
    public class RegisterRequest
    {
        [Required(ErrorMessage = "用户名不能为空")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "用户名长度必须在3-50个字符之间")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "邮箱不能为空")]
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "密码不能为空")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度必须至少为6个字符")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "确认密码不能为空")]
        [Compare("Password", ErrorMessage = "两次输入的密码不一致")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}