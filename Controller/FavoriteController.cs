using Food.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Food.Controller
{
    [ApiController]
    [Route("api/favorites")]
    [Authorize] // 需要登录
    public class FavoriteController : ControllerBase
    {
        private readonly FavoriteService _favoriteService;

        public FavoriteController(FavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        // 获取用户收藏
        [HttpGet]
        public async Task<IActionResult> GetUserFavorites()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var favorites = await _favoriteService.GetUserFavoritesAsync(userId);
            return Ok(favorites);
        }

        // 添加收藏
        [HttpPost("{merchantId}")]
        public async Task<IActionResult> AddFavorite(string merchantId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var success = await _favoriteService.AddFavoriteAsync(userId, merchantId);

            if (!success)
            {
                return BadRequest(new { message = "添加收藏失败" });
            }

            return Ok(new { message = "添加收藏成功" });
        }

        // 移除收藏
        [HttpDelete("{merchantId}")]
        public async Task<IActionResult> RemoveFavorite(string merchantId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var success = await _favoriteService.RemoveFavoriteAsync(userId, merchantId);

            if (!success)
            {
                return BadRequest(new { message = "移除收藏失败" });
            }

            return Ok(new { message = "移除收藏成功" });
        }

        // 检查是否已收藏
        [HttpGet("check/{merchantId}")]
        public async Task<IActionResult> CheckFavorite(string merchantId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var isFavorited = await _favoriteService.IsFavoritedAsync(userId, merchantId);
            return Ok(new { isFavorited });
        }
    }
}
