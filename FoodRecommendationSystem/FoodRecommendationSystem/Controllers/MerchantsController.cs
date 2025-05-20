using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FoodRecommendationSystem.Core.Models;
using FoodRecommendationSystem.DAL.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodRecommendationSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using FoodRecommendationSystem.Services.Implementations;
using System.Security.Claims;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace FoodRecommendationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MerchantsController : ControllerBase
    {
        private readonly IMerchantService _merchantService;
        private readonly IRecommendationService _recommendationService;
        private readonly CrawlerManager _crawlerManager;

        public MerchantsController(
            IMerchantService merchantService,
            IRecommendationService recommendationService,
            CrawlerManager crawlerManager)
        {
            _merchantService = merchantService;
            _recommendationService = recommendationService;
            _crawlerManager = crawlerManager;
        }

        /// <summary>
        /// 获取商家详情
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var merchant = await _merchantService.GetByIdAsync(id);
            if (merchant == null)
                return NotFound(new { message = "商家不存在" });

            return Ok(merchant);
        }

        /// <summary>
        /// 获取附近商家
        /// </summary>
        [HttpGet("nearby")]
        public async Task<IActionResult> GetNearby([FromQuery] double lat, [FromQuery] double lng, [FromQuery] double radius = 5, [FromQuery] int limit = 20)
        {
            var location = new GeoLocation { Latitude = lat, Longitude = lng };
            var merchants = await _merchantService.GetNearbyAsync(location, radius, limit);
            return Ok(merchants);
        }

        /// <summary>
        /// 获取个性化附近商家
        /// </summary>
        [HttpGet("personalized-nearby")]
        [Authorize]
        public async Task<IActionResult> GetPersonalizedNearby([FromQuery] double lat, [FromQuery] double lng, [FromQuery] double radius = 5, [FromQuery] int limit = 20)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return BadRequest(new { message = "用户ID不存在，无法提供个性化推荐" });
            }
            var location = new GeoLocation { Latitude = lat, Longitude = lng };
            var merchants = await _recommendationService.GetPersonalizedNearbyMerchantsAsync(userId, location, radius, limit);
            return Ok(merchants);
        }

        /// <summary>
        /// 搜索商家
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search(
         [FromQuery] string? keyword = null,
         [FromQuery] List<string>? categories = null,
         [FromQuery] double? minRating = null,
         [FromQuery] Region? region = null,
         [FromQuery] decimal? maxPrice = null,
         [FromQuery] int skip = 0,
         [FromQuery] int limit = 20)
        {
            // 提供默认值以避免传递 null
            var keywordValue = keyword ?? string.Empty;
            var categoriesValue = categories ?? new List<string>();

            var merchants = await _merchantService.SearchAsync(keywordValue, categoriesValue, minRating, region, maxPrice, skip, limit);
            return Ok(merchants);
        }

        /// <summary>
        /// 获取推荐商家
        /// </summary>
        [HttpGet("recommended")]
        public async Task<IActionResult> GetRecommended([FromQuery] int limit = 10)
        {
            string? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            if (userId == null)
            {
                return BadRequest(new { message = "用户ID不存在，无法提供个性化推荐" });
            }
            var recommendedMerchants = await _recommendationService.GetRecommendedMerchantsAsync(userId, limit);
            return Ok(recommendedMerchants);
        }

        /// <summary>
        /// 获取热门商家
        /// </summary>
        [HttpGet("hot")]
        public async Task<IActionResult> GetHotMerchants([FromQuery] int limit = 10)
        {
            var hotMerchants = await _merchantService.GetHotMerchantsAsync(limit);
            return Ok(hotMerchants);
        }

        /// <summary>
        /// 获取评分最高的商家
        /// </summary>
        [HttpGet("top-rated")]
        public async Task<IActionResult> GetTopRatedMerchants([FromQuery] int limit = 10)
        {
            var topRatedMerchants = await _merchantService.GetTopRatedMerchantsAsync(limit);
            return Ok(topRatedMerchants);
        }

        /// <summary>
        /// 获取最新更新的商家
        /// </summary>
        [HttpGet("recently-updated")]
        public async Task<IActionResult> GetRecentlyUpdatedMerchants([FromQuery] int limit = 10)
        {
            var recentMerchants = await _merchantService.GetRecentlyUpdatedMerchantsAsync(limit);
            return Ok(recentMerchants);
        }

        /// <summary>
        /// 手动触发爬虫(仅管理用户)
        /// </summary>
        [HttpPost("trigger-crawl")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TriggerCrawl()
        {
            await _crawlerManager.CrawlAllPlatformsAsync();
            return Ok(new { message = "爬虫任务已触发" });
        }
    }
}