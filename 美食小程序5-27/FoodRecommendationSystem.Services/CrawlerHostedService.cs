
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// CrawlerHostedService.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FoodRecommendationSystem.Services.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FoodRecommendationSystem.Services
{
    /// <summary>
    /// 爬虫定时任务服务
    /// </summary>
    public class CrawlerHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CrawlerHostedService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(6); // 每6小时爬取一次

        public CrawlerHostedService(
            IServiceProvider serviceProvider,
            ILogger<CrawlerHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("爬虫定时任务服务已启动");

            using var timer = new PeriodicTimer(_period);

            // 首次启动后先运行一次
            await CrawlDataAsync();

            // 然后按定时继续运行
            do
            {
                try
                {
                    await CrawlDataAsync();
                    _logger.LogInformation("定时爬虫任务完成");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "定时爬虫任务出错");
                }
            } while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested);

            _logger.LogInformation("爬虫定时任务服务已停止");
        }

        private async Task CrawlDataAsync()
        {
            _logger.LogInformation("开始定时数据爬取");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var crawlerManager = scope.ServiceProvider.GetRequiredService<CrawlerManager>();

                await crawlerManager.CrawlAllPlatformsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "爬取过程中发生错误");
            }
        }
    }
}
