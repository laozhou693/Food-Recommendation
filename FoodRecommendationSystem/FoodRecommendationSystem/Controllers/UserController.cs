using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using FoodRecommendationSystem.Core.Models;
using FoodRecommendationSystem.DAL.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FoodRecommendationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly UserHistoryRepository _historyRepository;
        private readonly MerchantRepository _merchantRepository;
        private readonly DishRepository _dishRepository;

        public UsersController(
            UserRepository userRepository,
            UserHistoryRepository historyRepository,
            MerchantRepository merchantRepository,
            DishRepository dishRepository)
        {
            _userRepository = userRepository;
            _historyRepository = historyRepository;
            _merchantRepository = merchantRepository;
            _dishRepository = dishRepository;
        }

        /// <summary>
        /// 获取用户个人资料
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                // 处理 userId 为 null 的情况，例如返回错误或使用默认用户
                return BadRequest(new { message = "无法识别用户身份" });
            }
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "用户不存在" });

            // 不返回密码哈希等敏感信息
            user.PasswordHash = null;

            return Ok(user);
        }

        /// <summary>
        /// 更新用户偏好设置
        /// </summary>
        [HttpPut("preferences")]
        public async Task<IActionResult> UpdatePreferences([FromBody] UserPreference preferences)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                // 处理 userId 为 null 的情况，例如返回错误或使用默认用户
                return BadRequest(new { message = "无法识别用户身份" });
            }
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "用户不存在" });

            // 确保userId一致
            preferences.UserId = userId;

            await _userRepository.UpdatePreferenceAsync(userId, preferences);

            return Ok(new { message = "偏好设置已更新" });
        }

        /// <summary>
        /// 添加商家到收藏
        /// </summary>
        [HttpPost("favorites/merchants/{merchantId}")]
        public async Task<IActionResult> AddFavoriteMerchant(string merchantId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                // 处理 userId 为 null 的情况，例如返回错误或使用默认用户
                return BadRequest(new { message = "无法识别用户身份" });
            }
            // 验证商家是否存在
            var merchant = await _merchantRepository.GetByIdAsync(merchantId);
            if (merchant == null)
                return NotFound(new { message = "商家不存在" });

            await _userRepository.AddToFavoritesAsync(userId, merchantId, true);

            // 记录用户历史
            await _historyRepository.CreateAsync(new UserHistory
            {
                UserId = userId,
                MerchantId = merchantId,
                Type = HistoryType.Favorite,
                Timestamp = DateTime.Now
            });

            return Ok(new { message = "已添加到收藏" });
        }

        /// <summary>
        /// 从收藏中移除商家
        /// </summary>
        [HttpDelete("favorites/merchants/{merchantId}")]
        public async Task<IActionResult> RemoveFavoriteMerchant(string merchantId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                // 处理 userId 为 null 的情况，例如返回错误或使用默认用户
                return BadRequest(new { message = "无法识别用户身份" });
            }
            await _userRepository.RemoveFromFavoritesAsync(userId, merchantId, true);

            // 记录用户历史
            await _historyRepository.CreateAsync(new UserHistory
            {
                UserId = userId,
                MerchantId = merchantId,
                Type = HistoryType.Unfavorite,
                Timestamp = DateTime.Now
            });

            return Ok(new { message = "已从收藏中移除" });
        }

        /// <summary>
        /// 添加菜品到收藏
        /// </summary>
        [HttpPost("favorites/dishes/{dishId}")]
        public async Task<IActionResult> AddFavoriteDish(string dishId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                // 处理 userId 为 null 的情况，例如返回错误或使用默认用户
                return BadRequest(new { message = "无法识别用户身份" });
            }

            // 验证菜品是否存在
            var dish = await _dishRepository.GetByIdAsync(dishId);
            if (dish == null)
                return NotFound(new { message = "菜品不存在" });

            await _userRepository.AddToFavoritesAsync(userId, dishId, false);

            await _historyRepository.CreateAsync(new UserHistory
            {
                UserId = userId,
                DishId = dishId,
                Type = HistoryType.Favorite,
                Timestamp = DateTime.Now
            });

            return Ok(new { message = "已添加到收藏" });
        }

        /// <summary>
        /// 从收藏中移除菜品
        /// </summary>
        [HttpDelete("favorites/dishes/{dishId}")]
        public async Task<IActionResult> RemoveFavoriteDish(string dishId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                // 处理 userId 为 null 的情况，例如返回错误或使用默认用户
                return BadRequest(new { message = "无法识别用户身份" });
            }
            await _userRepository.RemoveFromFavoritesAsync(userId, dishId, false);

            await _historyRepository.CreateAsync(new UserHistory
            {
                UserId = userId,
                DishId = dishId,
                Type = HistoryType.Unfavorite,
                Timestamp = DateTime.Now
            });

            return Ok(new { message = "已从收藏中移除" });
        }

        /// <summary>
        /// 获取收藏的商家
        /// </summary>
        [HttpGet("favorites/merchants")]
        public async Task<IActionResult> GetFavoriteMerchants()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                // 处理 userId 为 null 的情况，例如返回错误或使用默认用户
                return BadRequest(new { message = "无法识别用户身份" });
            }
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null || user.FavoriteMerchants == null || user.FavoriteMerchants.Count == 0)
                return Ok(new List<Merchant>());

            var result = new List<Merchant>();
            foreach (var merchantId in user.FavoriteMerchants)
            {
                var merchant = await _merchantRepository.GetByIdAsync(merchantId);
                if (merchant != null)
                {
                    result.Add(merchant);
                }
            }

            return Ok(result);
        }

        /// <summary>
        /// 获取收藏的菜品
        /// </summary>
        [HttpGet("favorites/dishes")]
        public async Task<IActionResult> GetFavoriteDishes()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                // 处理 userId 为 null 的情况，例如返回错误或使用默认用户
                return BadRequest(new { message = "无法识别用户身份" });
            }
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null || user.FavoriteDishes == null || user.FavoriteDishes.Count == 0)
                return Ok(new List<Dish>());

            var result = new List<Dish>();
            foreach (var dishId in user.FavoriteDishes)
            {
                var dish = await _dishRepository.GetByIdAsync(dishId);
                if (dish != null)
                {
                    result.Add(dish);
                }
            }

            return Ok(result);
        }

        /// <summary>
        /// 记录浏览历史
        /// </summary>
        [HttpPost("history")]
        public async Task<IActionResult> AddHistory([FromBody] UserHistoryRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                // 处理 userId 为 null 的情况，例如返回错误或使用默认值
                return BadRequest(new { message = "用户未登录或无法识别用户身份" });
            }
            var history = new UserHistory
            {
                UserId = userId,
                MerchantId = request.MerchantId,
                DishId = request.DishId,
                Type = request.Type,
                Timestamp = DateTime.Now,
                SearchKeyword = request.SearchKeyword,
                UserLocation = request.UserLocation
            };

            await _historyRepository.CreateAsync(history);

            // 如果是搜索操作，更新用户搜索历史
            if (request.Type == HistoryType.Search && !string.IsNullOrEmpty(request.SearchKeyword))
            {
                if (userId == null)
                {
                    // 处理 userId 为 null 的情况，例如返回错误或使用默认用户
                    return BadRequest(new { message = "无法识别用户身份" });
                }
                await _userRepository.AddSearchKeywordAsync(userId, request.SearchKeyword);
            }

            return Ok(new { message = "历史记录已保存" });
        }

        /// <summary>
        /// 获取浏览历史
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int limit = 50)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                // 处理 userId 为 null 的情况，例如返回错误或使用默认用户
                return BadRequest(new { message = "无法识别用户身份" });
            }
            var history = await _historyRepository.GetByUserIdAsync(userId, limit);

            return Ok(history);
        }

        /// <summary>
        /// 清空浏览历史
        /// </summary>
        [HttpDelete("history")]
        public async Task<IActionResult> ClearHistory()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                // 处理 userId 为 null 的情况，例如返回错误或使用默认用户
                return BadRequest(new { message = "无法识别用户身份" });
            }
            await _historyRepository.ClearUserHistoryAsync(userId);

            return Ok(new { message = "浏览历史已清空" });
        }
    }

    /// <summary>
    /// 用户历史请求模型
    /// </summary>
    public class UserHistoryRequest
    {
        public string MerchantId { get; set; } = string.Empty;
        public string DishId { get; set; } = string.Empty;
        public HistoryType Type { get; set; }
        public string SearchKeyword { get; set; } = string.Empty;
        public required GeoLocation UserLocation { get; set; }
    }
}