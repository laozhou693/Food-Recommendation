using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using FoodRecommendationSystem.Core.Models;
using FoodRecommendationSystem.DAL.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace FoodRecommendationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly CategoryRepository _categoryRepository;

        public CategoriesController(CategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        //获取所有分类
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return Ok(categories);
        }

        //根据类型获取分类
        [HttpGet("by-type/{type}")]
        public async Task<IActionResult> GetByType(CategoryType type)
        {
            var categories = await _categoryRepository.GetByTypeAsync(type);
            return Ok(categories);
        }

        //获取顶级分类
        [HttpGet("top/{type}")]
        public async Task<IActionResult> GetTopCategories(CategoryType type)
        {
            var categories = await _categoryRepository.GetTopCategoriesAsync(type);
            return Ok(categories);
        }

        //获取子分类
        [HttpGet("children/{parentId}")]
        public async Task<IActionResult> GetChildren(string parentId)
        {
            var categories = await _categoryRepository.GetChildrenAsync(parentId);
            return Ok(categories);
        }

        //搜索分类
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string name, [FromQuery] CategoryType? type = null)
        {
            var categories = await _categoryRepository.SearchByNameAsync(name, type);
            return Ok(categories);
        }

        //获取分类树
        [HttpGet("tree/{type}")]
        public async Task<IActionResult> GetCategoryTree(CategoryType type)
        {
            var tree = await _categoryRepository.GetCategoryTreeAsync(type);
            return Ok(tree);
        }

        //创建分类(仅管理员)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] Category category)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _categoryRepository.CreateAsync(category);
            return CreatedAtAction(nameof(GetAll), new { id = category.Id }, category);
        }

        //更新分类(仅管理员)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(string id, [FromBody] Category category)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            category.Id = id;
            await _categoryRepository.UpdateAsync(id, category);
            return Ok(category);
        }

        //删除分类(仅管理员)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            await _categoryRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}