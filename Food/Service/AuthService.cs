using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Food.Models;
using Food.Repository;
using BCrypt.Net;

namespace Food.Service
{
    public class AuthService
    {
        private readonly UserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(UserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<(bool Success, string Message, User? User)> RegisterAsync(string username, string password)
        {
            // 检查用户名是否已存在
            var existingUser = await _userRepository.GetByUsernameAsync(username);
            if (existingUser != null)
            {
                return (false, "用户名已存在", null);
            }

            // 创建新用户
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User
            {
                Username = username, // 确保 Username 不为 null
                PasswordHash = passwordHash
            };

            await _userRepository.CreateAsync(user);
            return (true, "注册成功", user);
        }

        public async Task<(bool Success, string Message, string? Token, User? User)> LoginAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
            {
                return (false, "用户名不存在", null, null);
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return (false, "密码错误", null, null);
            }

            // 更新登录时间
            await _userRepository.UpdateLastLoginAsync(user.Id);

            // 生成 JWT Token
            var token = GenerateJwtToken(user);

            return (true, "登录成功", token, user);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (jwtKey == null)
            {
                throw new InvalidOperationException("JWT 密钥未配置");
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(30), // 30天有效期
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}