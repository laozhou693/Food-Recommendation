using Food.Service;
using Food.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FoodRecommendationSystem.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    [Tags("认证")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // 参数校验
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    statusCode = StatusCodes.Status400BadRequest,
                    message = "请求参数无效",
                    data = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                        .ToList() // 返回具体的错误信息
                });
            }

            // 调用注册服务
            var result = await _authService.RegisterAsync(model.Username, model.Password);

            // 处理失败情况
            if (!result.Success)
            {
                return BadRequest(new
                {
                    statusCode = StatusCodes.Status400BadRequest,
                    message = result.Message ?? "注册失败",
                    data = (object)null // 无数据时返回 null
                });
            }

            // 检查用户数据
            if (result.User == null)
            {
                return BadRequest(new
                {
                    statusCode = StatusCodes.Status400BadRequest,
                    message = "用户信息不可用",
                    data = (object)null
                });
            }

            // 返回成功响应
            return Ok(new
            {
                statusCode = StatusCodes.Status200OK,
                message = "注册成功",
                data = new
                {
                    Id = result.User.Id,
                    Username = result.User.Username
                }
            });
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            // 1. 参数校验
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    statusCode = StatusCodes.Status400BadRequest,
                    message = "请求参数无效",
                    data = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                        .ToList()
                });
            }

            // 2. 调用登录服务
            var result = await _authService.LoginAsync(model.Username, model.Password);

            // 3. 处理登录失败情况
            if (!result.Success)
            {
                return BadRequest(new
                {
                    statusCode = StatusCodes.Status400BadRequest,
                    message = result.Message ?? "登录失败",
                    data = (object?)null
                });
            }

            // 4. 检查token和用户数据
            if (result.Token == null || result.User == null)
            {
                return BadRequest(new
                {
                    statusCode = StatusCodes.Status400BadRequest,
                    message = "登录失败，用户信息或令牌不可用",
                    data = (object?)null
                });
            }

            // 5. 返回成功响应
            return Ok(new
            {
                statusCode = StatusCodes.Status200OK,
                message = result.Message ?? "登录成功",
                data = new
                {
                    Token = result.Token,
                    User = new
                    {
                        Id = result.User.Id,
                        Username = result.User.Username
                    }
                }
            });
        }
    }
}