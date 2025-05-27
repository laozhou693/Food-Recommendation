
﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using FoodRecommendationSystem.Services.Interfaces;
using FoodRecommendationSystem.Core.Models;
using System.Collections.Generic;

namespace FoodRecommendationSystem.Services.Implementations
{
    /// <summary>
    /// MongoDB数据导入服务
    /// </summary>
    public class MongoCrawlerService : ICrawlerService
    {
        private readonly ILogger<MongoCrawlerService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _pythonPath;
        private readonly string _crawlerScriptPath;
        private readonly string _mongoConnectionString;

        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName => "MongoDB";

        public MongoCrawlerService(
            ILogger<MongoCrawlerService> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            // 从配置中获取Python路径，默认为"python"
            _pythonPath = _configuration["MongoCrawler:PythonPath"] ?? "python";

            // 获取爬虫脚本路径
            var scriptPath = _configuration["MongoCrawler:ScriptPath"] ?? "PythonCrawler/mongodb_crawler.py";
            _crawlerScriptPath = Path.GetFullPath(scriptPath);

            // 获取MongoDB连接字符串
            _mongoConnectionString = _configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017/";

            _logger.LogInformation($"MongoDB爬虫服务初始化: 脚本路径={_crawlerScriptPath}, 连接字符串={_mongoConnectionString}");
        }

        /// <summary>
        /// 爬取商家数据
        /// </summary>
        public async Task<List<Merchant>> CrawlMerchantsAsync(string platform)
        {
            _logger.LogInformation("开始从Excel导入数据到MongoDB");

            try
            {
                // 从配置中获取Excel数据文件路径
                var elePath = _configuration["MongoCrawler:EleDataPath"];
                var meituanPath = _configuration["MongoCrawler:MeituanDataPath"];

                if (string.IsNullOrEmpty(elePath) || string.IsNullOrEmpty(meituanPath))
                {
                    _logger.LogError("未配置Excel数据文件路径，请在appsettings.json中配置MongoCrawler:EleDataPath和MongoCrawler:MeituanDataPath");
                    return new List<Merchant>();
                }

                // 验证文件是否存在
                if (!File.Exists(elePath))
                {
                    _logger.LogError($"饿了么数据文件不存在: {elePath}");
                    return new List<Merchant>();
                }

                if (!File.Exists(meituanPath))
                {
                    _logger.LogError($"美团数据文件不存在: {meituanPath}");
                    return new List<Merchant>();
                }

                if (!File.Exists(_crawlerScriptPath))
                {
                    _logger.LogError($"Python爬虫脚本不存在: {_crawlerScriptPath}");
                    return new List<Merchant>();
                }

                // 创建进程启动信息
                var startInfo = new ProcessStartInfo
                {
                    FileName = _pythonPath,
                    Arguments = $"\"{_crawlerScriptPath}\" \"{elePath}\" \"{meituanPath}\" \"{_mongoConnectionString}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                // 启动Python进程
                _logger.LogInformation($"执行Python命令: {startInfo.FileName} {startInfo.Arguments}");
                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    _logger.LogError("无法启动Python进程");
                    return new List<Merchant>();
                }

                // 读取输出
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                // 等待进程完成
                await process.WaitForExitAsync();

                if (!string.IsNullOrEmpty(error))
                {
                    _logger.LogWarning($"Python进程输出了错误信息: {error}");
                }

                _logger.LogInformation($"Python进程输出: {output}");

                if (process.ExitCode != 0)
                {
                    _logger.LogError($"Python进程退出码: {process.ExitCode}，表示执行失败");
                    return new List<Merchant>();
                }

                try
                {
                    // 尝试解析JSON结果
                    var result = JsonSerializer.Deserialize<ImportResult>(output);
                    if (result != null && result.Success)
                    {
                        _logger.LogInformation($"成功导入{result.Count}个商家数据");
                        // 返回空列表，因为数据已经直接导入到MongoDB
                        return new List<Merchant>();
                    }
                    else
                    {
                        _logger.LogWarning($"数据导入失败: {result?.Error ?? "未知错误"}");
                        return new List<Merchant>();
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, $"解析Python输出失败: {output}");
                    return new List<Merchant>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行MongoDB数据导入时出错");
                return new List<Merchant>();
            }
        }

        // 数据导入结果类
        private class ImportResult
        {
            public bool Success { get; set; }
            public int Count { get; set; }
            public string? Error { get; set; }
        }
    }
}
