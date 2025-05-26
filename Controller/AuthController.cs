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
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(model.Username, model.Password);

            if (!result.Success)
            {
                return BadRequest(new ErrorResponse { Message = result.Message });
            }

            User? user = result.User;
            if (user == null)
            {
                return BadRequest(new ErrorResponse { Message = "用户信息不可用" });
            }

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
    }
}