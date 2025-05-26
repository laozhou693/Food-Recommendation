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
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Register model state invalid: {Errors}", ModelState.Errors());
                return BadRequest(new ErrorResponse { Message = "输入数据格式不正确", Details = ModelState.Errors() });
            }

            try
            {
                var result = await _authService.RegisterAsync(model.Username, model.Password);

                if (!result.Success)
                {
                    _logger.LogInformation("Registration failed for {Username}: {Reason}", model.Username, result.Message);
                    return BadRequest(new ErrorResponse { Message = result.Message });
                }

                var user = result.User;
                if (user == null)
                {
                    _logger.LogError("Registration succeeded but returned user is null for {Username}", model.Username);
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ErrorResponse { Message = "用户注册成功但用户信息不可用" });
                }

                _logger.LogInformation("User registered successfully: {Username} (Id: {UserId})", user.Username, user.Id);

                return Ok(new RegisterResponse
                {
                    Message = result.Message,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Username = user.Username
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error during registration for {Username}", model.Username);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "数据库错误，注册失败，请稍后重试" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for {Username}", model.Username);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "发生未知错误，注册失败，请联系我们的支持团队" });
            }
        }
    }
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(model.Username, model.Password);

            if (!result.Success)
            {
                return BadRequest(new ErrorResponse { Message = result.Message });
            }

            string? token = result.Token;
            User? user = result.User;

            if (token == null || user == null)
            {
                return BadRequest(new ErrorResponse { Message = "登录失败，用户信息或令牌不可用" });
            }

            return Ok(new LoginResponse
            {
                Message = result.Message,
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username
                }
            });
        }
        public static class ModelStateExtensions
    {
        public static string Errors(this ModelStateDictionary state)
        {
            return string.Join("; ", state.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
        }
    }
    }
