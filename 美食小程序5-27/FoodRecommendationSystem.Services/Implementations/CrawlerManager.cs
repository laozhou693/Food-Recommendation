using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodRecommendationSystem.Core.Models;
using FoodRecommendationSystem.DAL.Repositories;
using FoodRecommendationSystem.Services.Interfaces;
using Microsoft.Extensions.Logging;


namespace FoodRecommendationSystem.Services.Implementations
{
    /// <summary>
    /// 爬虫管理服务
    /// </summary>
    public class CrawlerManager
    {
        private readonly IEnumerable<ICrawlerService> _crawlers;
        private readonly MerchantRepository _merchantRepository;
        private readonly DishRepository _dishRepository;
        private readonly ReviewRepository _reviewRepository;
        private readonly ILogger<CrawlerManager> _logger;

        public CrawlerManager(
            IEnumerable<ICrawlerService> crawlers,
            MerchantRepository merchantRepository,
            DishRepository dishRepository,
            ReviewRepository reviewRepository,
            ILogger<CrawlerManager> logger)
        {
            _crawlers = crawlers;
            _merchantRepository = merchantRepository;
            _dishRepository = dishRepository;
            _reviewRepository = reviewRepository;
            _logger = logger;
        }

        /// <summary>
        /// 爬取所有平台数据
        /// </summary>
        public async Task CrawlAllPlatformsAsync()
        {
            _logger.LogInformation("开始爬取所有平台数据");

            if (!_crawlers.Any())
            {
                _logger.LogWarning("没有注册爬虫服务");
                return;
            }

            foreach (var crawler in _crawlers)
            {
                try
                {
                    await CrawlPlatformAsync(crawler);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"爬取平台{crawler.PlatformName}数据失败");
                }
            }

            _logger.LogInformation("所有平台数据爬取完成");
        }

        /// <summary>
        /// 爬取指定平台数据
        /// </summary>
        public async Task CrawlPlatformAsync(ICrawlerService crawler)
        {
            var platformName = crawler.PlatformName;
            _logger.LogInformation($"开始爬取{platformName}平台数据");

            try
            {
                var merchants = await crawler.CrawlMerchantsAsync(platformName);

                // 如果是MongoDB导入服务，数据已经直接导入，无需再保存
                if (platformName == "MongoDB")
                {
                    _logger.LogInformation($"{platformName}平台数据已直接导入到数据库");
                    return;
                }

                if (merchants == null || merchants.Count == 0)
                {
                    _logger.LogWarning($"{platformName}平台未爬取到数据");
                    return;
                }

                _logger.LogInformation($"爬取到{platformName}平台商家数据{merchants.Count}条");

                // 保存商家和菜品数据
                await SaveMerchantsAndDishesAsync(merchants, platformName);

                // 保存评价数据
                await SaveReviewsAsync(merchants);

                _logger.LogInformation($"{platformName}平台数据爬取和保存完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"爬取{platformName}平台数据过程中发生错误");
                throw;
            }
        }

        /// <summary>
        /// 保存商家和菜品数据
        /// </summary>
        private async Task SaveMerchantsAndDishesAsync(List<Merchant> merchants, string platformName)
        {
            foreach (var merchant in merchants)
            {
                try
                {
                    // 获取平台商家ID
                    var platformInfo = merchant.PlatformInfos.FirstOrDefault(p => p.PlatformName == platformName);
                    if (platformInfo == null || string.IsNullOrEmpty(platformInfo.PlatformMerchantId))
                    {
                        _logger.LogWarning($"商家{merchant.Name}缺少平台ID信息");
                        continue;
                    }

                    var platformMerchantId = platformInfo.PlatformMerchantId;

                    // 检查商家是否已存在
                    var existingMerchant = await _merchantRepository.GetByPlatformIdAsync(platformName, platformMerchantId);

                    if (existingMerchant == null)
                    {
                        // 新商家，直接创建
                        merchant.LastUpdated = DateTime.Now;
                        await _merchantRepository.CreateAsync(merchant);

                        _logger.LogInformation($"创建新商家: {merchant.Name}");

                        // 处理菜品
                        if (merchant.Dishes != null && merchant.Dishes.Count > 0)
                        {
                            foreach (var dish in merchant.Dishes)
                            {
                                dish.MerchantId = merchant.Id;
                                dish.LastUpdated = DateTime.Now;
                            }

                            await _dishRepository.CreateManyAsync(merchant.Dishes);
                            _logger.LogInformation($"为商家{merchant.Name}创建{merchant.Dishes.Count}个菜品");
                        }
                    }
                    else
                    {
                        // 更新现有商家
                        _logger.LogInformation($"更新现有商家: {merchant.Name}");

                        // 保留现有商家ID
                        merchant.Id = existingMerchant.Id;

                        // 合并平台信息
                        var existingPlatformInfos = existingMerchant.PlatformInfos.Where(p => p.PlatformName != platformName).ToList();
                        existingPlatformInfos.Add(platformInfo);
                        merchant.PlatformInfos = existingPlatformInfos;

                        // 更新时间
                        merchant.LastUpdated = DateTime.Now;

                        await _merchantRepository.UpdateAsync(existingMerchant.Id, merchant);

                        // 处理菜品
                        if (merchant.Dishes != null && merchant.Dishes.Count > 0)
                        {
                            // 获取商家现有菜品
                            var existingDishes = await _dishRepository.GetByMerchantIdAsync(existingMerchant.Id);
                            var existingDishNameMap = existingDishes.ToDictionary(d => d.Name.ToLower(), d => d);

                            foreach (var dish in merchant.Dishes)
                            {
                                dish.MerchantId = existingMerchant.Id;
                                dish.LastUpdated = DateTime.Now;

                                // 检查菜品是否已存在
                                if (existingDishNameMap.TryGetValue(dish.Name.ToLower(), out var existingDish))
                                {
                                    // 合并价格信息
                                    var existingPriceInfo = existingDish.Prices.FirstOrDefault(p => p.PlatformName == platformName);
                                    var newPriceInfo = dish.Prices.FirstOrDefault(p => p.PlatformName == platformName);

                                    if (existingPriceInfo != null && newPriceInfo != null)
                                    {
                                        // 更新现有价格信息
                                        existingPriceInfo.Price = newPriceInfo.Price;
                                        existingPriceInfo.OriginalPrice = newPriceInfo.OriginalPrice;
                                        existingPriceInfo.HasDiscount = newPriceInfo.HasDiscount;
                                        existingPriceInfo.LastUpdated = DateTime.Now;
                                    }
                                    else if (newPriceInfo != null)
                                    {
                                        // 添加新价格信息
                                        existingDish.Prices.Add(newPriceInfo);
                                    }

                                    // 更新其他信息
                                    existingDish.MonthlySales = dish.MonthlySales > 0 ? dish.MonthlySales : existingDish.MonthlySales;
                                    existingDish.Rating = Math.Max(dish.Rating, existingDish.Rating);
                                    existingDish.LastUpdated = DateTime.Now;

                                    // 合并图片
                                    if (dish.ImageUrls != null && dish.ImageUrls.Count > 0)
                                    {
                                        if (existingDish.ImageUrls == null)
                                            existingDish.ImageUrls = new List<string>();

                                        foreach (var img in dish.ImageUrls)
                                        {
                                            if (!existingDish.ImageUrls.Contains(img))
                                                existingDish.ImageUrls.Add(img);
                                        }
                                    }

                                    await _dishRepository.UpdateAsync(existingDish.Id, existingDish);
                                }
                                else
                                {
                                    // 新菜品，创建
                                    await _dishRepository.CreateAsync(dish);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"保存商家{merchant.Name}数据失败");
                }
            }
        }

        /// <summary>
        /// 保存评价数据
        /// </summary>
        private async Task SaveReviewsAsync(List<Merchant> merchants)
        {
            foreach (var merchant in merchants)
            {
                try
                {
                    // 检查商家是否有评价字段
                    if (merchant.Reviews == null || merchant.Reviews.Count == 0)
                        continue;

                    var reviews = merchant.Reviews;

                    // 确保每条评价都有商家ID
                    foreach (var review in reviews)
                    {
                        review.MerchantId = merchant.Id;
                    }

                    // 批量创建评价(会自动跳过重复的评价)
                    await _reviewRepository.CreateManyAsync(reviews);

                    _logger.LogInformation($"为商家{merchant.Name}保存{reviews.Count}条评价");

                    // 清除临时评价数据
                    merchant.Reviews.Clear();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"保存商家{merchant.Name}评价数据失败");
                }
            }
        }
    }
}