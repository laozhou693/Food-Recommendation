
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoodRecommendationSystem.Core.Models;

namespace FoodRecommendationSystem.Services.Interfaces
{
    /// <summary>
    /// 爬虫服务接口
    /// </summary>
    public interface ICrawlerService
    {
        /// <summary>
        /// 获取平台名称
        /// </summary>
        string PlatformName { get; }

        /// <summary>
        /// 爬取商家数据
        /// </summary>
        Task<List<Merchant>> CrawlMerchantsAsync(string platform);
    }
}
