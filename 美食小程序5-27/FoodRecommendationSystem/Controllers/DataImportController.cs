using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodRecommendationSystem.Services.Implementations;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.IO;

namespace FoodRecommendationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DataImportController : ControllerBase
    {
        private readonly CrawlerManager _crawlerManager;
        private readonly ILogger<DataImportController> _logger; // 声明日志记录器字段

        public DataImportController(
            CrawlerManager crawlerManager,
            ILogger<DataImportController> logger) // 在构造函数中注入ILogger
        {
            _crawlerManager = crawlerManager;
            _logger = logger; // 初始化日志记录器
        }

        /// <summary>
        /// 手动触发数据导入
        /// </summary>
        [HttpPost("run")]
        public async Task<IActionResult> ImportData()
        {
            try
            {
                await _crawlerManager.CrawlAllPlatformsAsync();
                return Ok(new { message = "数据导入任务已触发" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "手动触发数据导入失败"); // 现在可以使用_logger
                return StatusCode(500, new { error = "数据导入失败，请查看日志" });
            }
        }

        /// <summary>
        /// 上传Excel文件并导入数据
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadAndImport()
        {
            try
            {
                // 获取上传的文件
                var eleFile = Request.Form.Files["eleData"];
                var meituanFile = Request.Form.Files["meituanData"];

                if (eleFile == null || meituanFile == null)
                {
                    return BadRequest(new { error = "请提供饿了么和美团的数据文件" });
                }

                // 确保目录存在
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "PythonCrawler");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                // 保存饿了么数据文件
                var elePath = Path.Combine(uploadDir, "ele-data.xls");
                using (var stream = new FileStream(elePath, FileMode.Create))
                {
                    await eleFile.CopyToAsync(stream);
                }

                // 保存美团数据文件
                var meituanPath = Path.Combine(uploadDir, "meituan-data.xls");
                using (var stream = new FileStream(meituanPath, FileMode.Create))
                {
                    await meituanFile.CopyToAsync(stream);
                }

                // 触发数据导入
                await _crawlerManager.CrawlAllPlatformsAsync();

                return Ok(new { message = "文件上传成功并开始导入数据" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上传Excel文件并导入数据失败"); // 现在可以使用_logger
                return StatusCode(500, new { error = $"数据上传或导入失败：{ex.Message}" });
            }
        }
    }
}