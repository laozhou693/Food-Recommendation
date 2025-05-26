using Food.Models;
using Food.Repository;
using Food.Service;
using FoodRecommendationSystem.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Food.Controller
{
    /// <summary>
    /// 商家管理控制器
    /// </summary>
    /// <remarks>
    /// 提供商家查询、搜索和推荐功能
    /// </remarks>
    [ApiController]
    [Route("api/merchants")]
    [Produces("application/json")]
    [Tags("商家")]
    public class MerchantController : ControllerBase
    {
        private readonly MerchantRepository _merchantRepository;
        private readonly FavoriteService _favoriteService;
        private readonly DataImportService _dataImportService;

        public MerchantController(MerchantRepository merchantRepository, FavoriteService favoriteService, DataImportService dataImportService)
        {
            _merchantRepository = merchantRepository;
            _favoriteService = favoriteService;
            _dataImportService = dataImportService;
        }

        /// <summary>
        /// 获取所有商家
        /// </summary>
        /// <remarks>
        /// 返回系统中的所有商家列表
        /// </remarks>
        /// <response code="200">返回商家列表</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<Merchant>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var merchants = await _merchantRepository.GetAllAsync();
            return Ok(merchants);
        }

        /// <summary>
        /// 获取单个商家
        /// </summary>
        /// <remarks>
        /// 根据 ID 返回单个商家的详细信息
        /// </remarks>
        /// <param name="id">商家 ID</param>
        /// <response code="200">返回商家详情</response>
        /// <response code="404">商家不存在</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Merchant), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ImportMerchantTagsWithPython()
        {
            var result = await _dataImportService.ImportMerchantTagsAsync();

            if (result.Success)
            {
                return Ok(new ImportSuccessResponse
                {
                    Message = result.Message,
                    Count = result.Count
                });
            }
            else
            {
                return BadRequest(new ErrorResponse { Message = result.Message });
            }
        }
        // 搜索商家
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new { message = "关键词不能为空" });
            }

            var merchants = await _merchantRepository.SearchAsync(keyword);
            return Ok(merchants);
        }

        // 综合推荐商家
        [HttpGet("recommended")]
        public async Task<IActionResult> GetRecommended([FromQuery] int limit = 20)
        {
            var merchants = await _merchantRepository.GetRecommendedAsync(limit);
            return Ok(merchants);
        }

        // 距离最近商家
        [HttpGet("nearest")]
        public async Task<IActionResult> GetNearest([FromQuery] int limit = 20)
        {
            var merchants = await _merchantRepository.GetNearestAsync(limit);
            return Ok(merchants);
        }

        // 评分最高商家
        [HttpGet("highest-rated")]
        public async Task<IActionResult> GetHighestRated([FromQuery] int limit = 20)
        {
            var merchants = await _merchantRepository.GetHighestRatedAsync(limit);
            return Ok(merchants);
        }
    }
}
