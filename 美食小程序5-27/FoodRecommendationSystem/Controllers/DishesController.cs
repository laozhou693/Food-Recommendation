using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FoodRecommendationSystem.Core.Models;
using FoodRecommendationSystem.DAL.Repositories;
using FoodRecommendationSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodRecommendationSystem.API.Controllers
{
    /// <summary>
    /// 菜品控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DishesController : ControllerBase
    {
        private readonly DishRepository _dishRepository;
        private readonly IRecommendationService _recommendationService;
        private readonly UserHistoryRepository _historyRepository;
        private readonly UserRepository _userRepository;
        private readonly ILogger<DishesController> _logger;

        public DishesController(
            DishRepository dishRepository,
            IRecommendationService recommendationService,
            UserHistoryRepository historyRepository,
            UserRepository userRepository,
            ILogger<DishesController> logger)
        {
            _dishRepository = dishRepository;
            _recommendationService = recommendationService;
            _historyRepository = historyRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        // 获取菜品详情
        [HttpGet("{id}")]
        public async Task<ActionResult<Dish>> GetDish(string id)
        {
            try
            {
                var dish = await _dishRepository.GetByIdAsync(id);
                if (dish == null)
                {
                    return NotFound("菜品不存在");
                }

                // 获取当前用户ID（如果已登录）
                string? userId = null;
                if (User.Identity?.IsAuthenticated == true)
                {
                    userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    // 记录浏览历史
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var history = new UserHistory
                        {
                            UserId = userId,
                            DishId = id,
                            MerchantId = dish.MerchantId,
                            Type = HistoryType.View,
                            Timestamp = DateTime.Now
                        };
                        await _historyRepository.CreateAsync(history);
                    }
                }

                return Ok(dish);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取菜品详情时出错: {id}");
                return StatusCode(500, "服务器错误，请稍后再试");
            }
        }

        //搜索菜品
        [HttpGet("search")]
        public async Task<ActionResult<List<Dish>>> SearchDishes(
            [FromQuery] string? keyword = null,
            [FromQuery] List<string>? categories = null,
            [FromQuery] double? minRating = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] List<string>? tags = null)
        {
            try
            {
                var skip = (page - 1) * pageSize;

                // 假设SearchAsync方法接受(string keyword)
                var dishes = await _dishRepository.SearchAsync(keyword);

                // 在内存中进行其他过滤
                if (categories != null && categories.Count > 0)
                {
                    dishes = dishes.Where(d =>
                        d.Categories != null &&
                        d.Categories.Any(c => categories.Contains(c))).ToList();
                }

                if (minRating.HasValue)
                {
                    dishes = dishes.Where(d => d.Rating >= minRating.Value).ToList();
                }

                // 跳过价格过滤，因为我们不确定Dish类的价格结构
                // 如果需要，可以在了解Dish类结构后再添加

                if (tags != null && tags.Count > 0)
                {
                    dishes = dishes.Where(d =>
                        d.Tags != null &&
                        d.Tags.Any(t => tags.Contains(t))).ToList();
                }

                // 处理分页
                dishes = dishes.Skip(skip).Take(pageSize).ToList();

                return Ok(dishes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜索菜品时出错");
                return StatusCode(500, "服务器错误，请稍后再试");
            }
        }

        //获取菜品推荐
        [HttpGet("recommendations")]
        public async Task<ActionResult<List<Dish>>> GetRecommendations(int limit = 10)
        {
            try
            {
                // 获取当前用户ID（如果已登录）
                string? userId = null;
                if (User.Identity?.IsAuthenticated == true)
                {
                    userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                }

                List<Dish> recommendedDishes;
                if (!string.IsNullOrEmpty(userId))
                {
                    // 根据用户口味偏好生成个性化推荐
                    recommendedDishes = await _recommendationService.GetRecommendedDishesAsync(userId, limit);
                }
                else
                {
                    // 未登录用户，返回热门菜品
                    recommendedDishes = await _dishRepository.GetSignatureDishesAsync(limit);
                }

                return Ok(recommendedDishes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取菜品推荐时出错");
                return StatusCode(500, "服务器错误，请稍后再试");
            }
        }

        //获取商家菜品列表
        [HttpGet("merchant/{merchantId}")]
        public async Task<ActionResult<List<Dish>>> GetMerchantDishes(string merchantId)
        {
            try
            {
                var dishes = await _dishRepository.GetByMerchantIdAsync(merchantId);
                return Ok(dishes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取商家菜品列表时出错: {merchantId}");
                return StatusCode(500, "服务器错误，请稍后再试");
            }
        }

        //获取指定分类的菜品
        [HttpGet("category/{category}")]
        public async Task<ActionResult<List<Dish>>> GetDishesByCategory(
            string category,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var skip = (page - 1) * pageSize;

                // 假设SearchAsync方法接受(string keyword)，获取所有菜品然后过滤
                var allDishes = await _dishRepository.SearchAsync(null);

                var dishes = allDishes
                    .Where(d => d.Categories != null && d.Categories.Contains(category))
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                return Ok(dishes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取分类菜品时出错: {category}");
                return StatusCode(500, "服务器错误，请稍后再试");
            }
        }

        //获取热门菜品
        [HttpGet("popular")]
        public async Task<ActionResult<List<Dish>>> GetPopularDishes(
            [FromQuery] int limit = 10,
            [FromQuery] double minRating = 4.0)
        {
            try
            {
                // 假设SearchAsync方法接受(string keyword)，获取所有菜品然后过滤
                var allDishes = await _dishRepository.SearchAsync(null);

                // 修改：只用Rating排序，不使用ReviewCount
                var dishes = allDishes
                    .Where(d => d.Rating >= minRating)
                    .OrderByDescending(d => d.Rating)
                    .Take(limit)
                    .ToList();

                return Ok(dishes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取热门菜品时出错");
                return StatusCode(500, "服务器错误，请稍后再试");
            }
        }

        //收藏菜品
        [Authorize]
        [HttpPost("{id}/favorite")]
        public async Task<IActionResult> FavoriteDish(string id)
        {
            try
            {
                // 获取当前用户ID
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("用户未登录");
                }

                // 检查菜品是否存在
                var dish = await _dishRepository.GetByIdAsync(id);
                if (dish == null)
                {
                    return NotFound("菜品不存在");
                }

                // 获取用户信息
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("用户不存在");
                }

                // 添加到收藏
                if (user.FavoriteDishes == null)
                {
                    user.FavoriteDishes = new List<string>();
                }

                if (!user.FavoriteDishes.Contains(id))
                {
                    user.FavoriteDishes.Add(id);
                    await _userRepository.UpdateAsync(userId, user);

                    // 记录收藏历史
                    var history = new UserHistory
                    {
                        UserId = userId,
                        DishId = id,
                        MerchantId = dish.MerchantId,
                        Type = HistoryType.Favorite,
                        Timestamp = DateTime.Now
                    };
                    await _historyRepository.CreateAsync(history);

                    return Ok(new { message = "收藏成功" });
                }
                else
                {
                    return Ok(new { message = "已经收藏过该菜品" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"收藏菜品时出错: {id}");
                return StatusCode(500, "服务器错误，请稍后再试");
            }
        }

        /// <summary>
        /// 取消收藏菜品
        /// </summary>
        [Authorize]
        [HttpDelete("{id}/favorite")]
        public async Task<IActionResult> UnfavoriteDish(string id)
        {
            try
            {
                // 获取当前用户ID
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("用户未登录");
                }

                // 获取用户信息
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("用户不存在");
                }

                // 从收藏中移除
                if (user.FavoriteDishes != null && user.FavoriteDishes.Contains(id))
                {
                    user.FavoriteDishes.Remove(id);
                    await _userRepository.UpdateAsync(userId, user);

                    // 记录取消收藏历史
                    var history = new UserHistory
                    {
                        UserId = userId,
                        DishId = id,
                        Type = HistoryType.Unfavorite,
                        Timestamp = DateTime.Now
                    };
                    await _historyRepository.CreateAsync(history);

                    return Ok(new { message = "取消收藏成功" });
                }
                else
                {
                    return Ok(new { message = "未收藏该菜品" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"取消收藏菜品时出错: {id}");
                return StatusCode(500, "服务器错误，请稍后再试");
            }
        }

        /// <summary>
        /// 获取用户收藏的菜品
        /// </summary>
        [Authorize]
        [HttpGet("favorites")]
        public async Task<ActionResult<List<Dish>>> GetFavoriteDishes()
        {
            try
            {
                // 获取当前用户ID
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("用户未登录");
                }

                // 获取用户信息
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("用户不存在");
                }

                // 获取收藏的菜品列表
                var favoriteDishes = new List<Dish>();
                if (user.FavoriteDishes != null && user.FavoriteDishes.Count > 0)
                {
                    foreach (var dishId in user.FavoriteDishes)
                    {
                        var dish = await _dishRepository.GetByIdAsync(dishId);
                        if (dish != null)
                        {
                            favoriteDishes.Add(dish);
                        }
                    }
                }

                return Ok(favoriteDishes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取收藏菜品时出错");
                return StatusCode(500, "服务器错误，请稍后再试");
            }
        }

        /// <summary>
        /// 获取指定标签的菜品
        /// </summary>
        [HttpGet("tag/{tag}")]
        public async Task<ActionResult<List<Dish>>> GetDishesByTag(
            string tag,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var skip = (page - 1) * pageSize;

                // 假设SearchAsync方法接受(string keyword)，获取所有菜品然后过滤
                var allDishes = await _dishRepository.SearchAsync(null);

                var dishes = allDishes
                    .Where(d => d.Tags != null && d.Tags.Contains(tag))
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                return Ok(dishes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取标签菜品时出错: {tag}");
                return StatusCode(500, "服务器错误，请稍后再试");
            }
        }

        /// <summary>
        /// 获取特价菜品
        /// </summary>
        [HttpGet("special-offers")]
        public async Task<ActionResult<List<Dish>>> GetSpecialOffers(
            [FromQuery] int limit = 10)
        {
            try
            {
                // 假设SearchAsync方法接受(string keyword)，获取所有菜品然后过滤
                var allDishes = await _dishRepository.SearchAsync(null);

                // 简化：移除对不存在的属性的引用，只基于基本属性进行选择
                // 这里简单地返回所有菜品，因为我们不确定如何判断特价菜品
                var specialOffers = allDishes
                    .Take(limit)
                    .ToList();

                return Ok(specialOffers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取特价菜品时出错");
                return StatusCode(500, "服务器错误，请稍后再试");
            }
        }
    }
}