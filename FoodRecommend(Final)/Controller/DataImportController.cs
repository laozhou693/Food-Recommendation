using Food.Service;
using Food.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FoodRecommendationSystem.Controllers
{
    /// <summary>
    /// 数据导入控制器
    /// </summary>
    [ApiController]
    [Route("api/admin/import")]
    [Authorize(Roles = "Admin")] // 可选：限制只有管理员可以访问
    [Tags("数据管理")]
    public class DataImportController : ControllerBase
    {
        private readonly DataImportService _dataImportService;

        public DataImportController(DataImportService dataImportService)
        {
            _dataImportService = dataImportService;
        }

        /// <summary>
        /// 使用 Python 脚本导入商家标签
        /// </summary>
        /// <remarks>
        /// 运行 Python 脚本导入预定义的商家标签数据
        /// </remarks>
        /// <response code="200">导入成功</response>
        /// <response code="400">导入失败</response>
        /// <response code="401">未授权</response>
        [HttpPost("merchant-tags/python")]
        [ProducesResponseType(typeof(ImportSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
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

        /// <summary>
        /// 直接使用 C# 导入商家标签
        /// </summary>
        /// <remarks>
        /// 使用 C# 代码直接导入预定义的商家标签数据
        /// </remarks>
        /// <response code="200">导入成功</response>
        /// <response code="400">导入失败</response>
        /// <response code="401">未授权</response>
        [HttpPost("merchant-tags/direct")]
        [ProducesResponseType(typeof(ImportSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImportMerchantTagsDirectly()
        {
            var result = await _dataImportService.ImportMerchantTagsDirectlyAsync();

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
    }
}