using Food.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetUserFavorites()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new
                {
                    statusCode = StatusCodes.Status400BadRequest,
                    message = "用户未登录",
                    data = (object)null // 无数据时返回 null
                });
            }

            var favorites = await _favoriteService.GetUserFavoritesAsync(userId);
            return Ok(new
            {
                statusCode = StatusCodes.Status200OK,
                message = "查询成功",
                data = new
                {
                    Favorites = favorites
                }
            }) ;
        }

        // 添加收藏
        [HttpPost("{merchantId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddFavorite(string merchantId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new
                {
                    statusCode = StatusCodes.Status400BadRequest,
                    message = "用户未登录",
                    data = (object)null // 无数据时返回 null
                });
            }

            var success = await _favoriteService.AddFavoriteAsync(userId, merchantId);

            if (!success)
            {
                return BadRequest(new
                {
                    statusCode = StatusCodes.Status400BadRequest,
                    message = "添加收藏失败",
                    data = (object)null // 无数据时返回 null
                });
            }

            return Ok(new
            {
                statusCode = StatusCodes.Status200OK,
                message = "添加收藏成功",
                data = (object)null
            });
        }

        // 移除收藏
        [HttpDelete("{merchantId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveFavorite(string merchantId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new
                {
                    statusCode = StatusCodes.Status400BadRequest,
                    message = "用户未登录",
                    data = (object)null // 无数据时返回 null
                });
            }

            var success = await _favoriteService.RemoveFavoriteAsync(userId, merchantId);

            if (!success)
            {
                return BadRequest(new
                {
                    statusCode = StatusCodes.Status400BadRequest,
                    message = "移除收藏失败",
                    data = (object)null // 无数据时返回 null
                });
            }

            return Ok(new
            {
                statusCode = StatusCodes.Status200OK,
                message = "移除收藏成功",
                data = (object)null
            });
        }

        // 检查是否已收藏
        [HttpGet("check/{merchantId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CheckFavorite(string merchantId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new
                {
                    statusCode = StatusCodes.Status400BadRequest,
                    message = "用户未登录",
                    data = (object)null // 无数据时返回 null
                });
            }

            var isFavorited = await _favoriteService.IsFavoritedAsync(userId, merchantId);
            return Ok(new
            {
                statusCode = StatusCodes.Status200OK,
                message = "查询成功",
                data = new { 
                    IsFavorited=isFavorited
                }
            });
        }


        [HttpGet("count/{merchantId}")]
        public async Task<IActionResult> GetFavoriteCount(string merchantId)
        {
            try
            {
                var count = await _favoriteService.GetFavoriteCountAsync(merchantId);
                return Ok(new
                {
                    statusCode = StatusCodes.Status200OK,
                    message = "获取商家收藏次数成功",
                    data = new { favoriteCount = count }
                });
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "获取商家收藏次数失败");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    statusCode = StatusCodes.Status500InternalServerError,
                    message = "服务器内部错误",
                    data = (object?)null
                });
            }
        }
    }

}
