using Food.Models;
using Food.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Food.Controller
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryRepository _categoryRepository;
        private readonly MerchantRepository _merchantRepository;

        public CategoryController(CategoryRepository categoryRepository, MerchantRepository merchantRepository)
        {
            _categoryRepository = categoryRepository;
            _merchantRepository = merchantRepository;
        }

        // 获取所有分类
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return Ok(new
            {
                statusCode = StatusCodes.Status200OK,
                message = "",
                data = new
                {
                    Categories=categories
                }
            });
        }

        // 按类型获取分类
        [HttpGet("type/{type}")]
        public async Task<IActionResult> GetByType(CategoryType type)
        {
            var categories = await _categoryRepository.GetByTypeAsync(type);
            return Ok(new
            {
                statusCode = StatusCodes.Status200OK,
                message = "",
                data = new
                {
                    Categories = categories
                }
            });
        }

        // 获取特定分类的商家
        [HttpGet("{tag}/merchants")]
        public async Task<IActionResult> GetMerchantsByTag(string tag)
        {
            var merchants = await _merchantRepository.GetByTagAsync(tag);
            return Ok(new
            {
                statusCode = StatusCodes.Status200OK,
                message = "",
                data = new
                {
                    Tag=tag,
                    Count=merchants.Count,
                    Merchants=merchants
                }
            });
        }
    }
}
