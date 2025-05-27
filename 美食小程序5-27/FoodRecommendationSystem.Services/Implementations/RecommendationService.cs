
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodRecommendationSystem.Core.Models;
using FoodRecommendationSystem.DAL.Repositories;
using FoodRecommendationSystem.Services.Interfaces;

namespace FoodRecommendationSystem.Services.Implementations
{
    /// <summary>
    /// 推荐服务实现类
    /// </summary>
    public class RecommendationService : IRecommendationService
    {
        private readonly MerchantRepository _merchantRepository;
        private readonly UserRepository _userRepository;
        private readonly DishRepository _dishRepository;
        private readonly UserHistoryRepository _historyRepository;
        private readonly ReviewRepository _reviewRepository;

        public RecommendationService(
            MerchantRepository merchantRepository,
            UserRepository userRepository,
            DishRepository dishRepository,
            UserHistoryRepository historyRepository,
            ReviewRepository reviewRepository)
        {
            _merchantRepository = merchantRepository;
            _userRepository = userRepository;
            _dishRepository = dishRepository;
            _historyRepository = historyRepository;
            _reviewRepository = reviewRepository;
        }

        /// <summary>
        /// 获取推荐商家列表
        /// </summary>
        public async Task<List<Merchant>> GetRecommendedMerchantsAsync(string userId, int limit = 10)
        {
            // 获取用户偏好
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return await _merchantRepository.GetTopRatedMerchantsAsync(20, limit);

            // 获取用户历史记录
            var history = await _historyRepository.GetByUserIdAsync(userId);

            // 获取用户常去区域
            var regions = new List<Region>();
            if (user.Preference?.FrequentRegions != null && user.Preference.FrequentRegions.Count > 0)
            {
                foreach (var regionStr in user.Preference.FrequentRegions)
                {
                    if (Enum.TryParse<Region>(regionStr, out var parsedRegion))
                    {
                        regions.Add(parsedRegion);
                    }
                }
            }

            // 获取用户喜欢的美食类型
            var categories = user.Preference?.FavoriteCuisines;

            // 构建查询条件
            double? minRating = 4.0; // 默认只推荐4星以上的
            Region? preferredRegion = null;
            if (regions.Count > 0)
            {
                // 随机选择一个常去区域
                var random = new Random();
                preferredRegion = regions[random.Next(regions.Count)];
            }

            // 查找符合条件的商家
            var recommendedMerchants = await _merchantRepository.SearchAsync(
                null, categories, minRating, preferredRegion);

            // 排除已收藏的商家
            if (user.FavoriteMerchants != null && user.FavoriteMerchants.Count > 0)
            {
                recommendedMerchants = recommendedMerchants
                    .Where(m => !user.FavoriteMerchants.Contains(m.Id))
                    .ToList();
            }

            // 获取用户最近浏览的商家ID列表
            var recentViewedIds = await _historyRepository.GetRecentViewedMerchantIdsAsync(userId);

            // 排序策略：结合评分、用户偏好和浏览历史
            var scoredMerchants = new List<(Merchant merchant, double score)>();

            foreach (var merchant in recommendedMerchants)
            {
                double score = CalculateMerchantScore(merchant, user, history, recentViewedIds);
                scoredMerchants.Add((merchant, score));
            }

            // 按得分降序排序并返回指定数量
            return scoredMerchants
                .OrderByDescending(m => m.score)
                .Select(m => m.merchant)
                .Take(limit)
                .ToList();
        }

        /// <summary>
        /// 获取推荐菜品列表
        /// </summary>
        public async Task<List<Dish>> GetRecommendedDishesAsync(string userId, int limit = 10)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return await _dishRepository.GetSignatureDishesAsync(limit);

            var history = await _historyRepository.GetByUserIdAsync(userId);
            var categories = user.Preference?.FavoriteCuisines;

            // 查找符合用户口味的菜品
            var recommendedDishes = await _dishRepository.SearchAsync(null, categories, 4.5);

            // 排除已收藏的菜品
            if (user.FavoriteDishes != null && user.FavoriteDishes.Count > 0)
            {
                recommendedDishes = recommendedDishes
                    .Where(d => !user.FavoriteDishes.Contains(d.Id))
                    .ToList();
            }

            // 获取用户历史上浏览过的商家的菜品
            var viewedMerchantIds = history
                .Where(h => h.Type == HistoryType.View && !string.IsNullOrEmpty(h.MerchantId))
                .Select(h => h.MerchantId)
                .Distinct()
                .ToList();

            var viewedMerchantDishes = new List<Dish>();
            foreach (var merchantId in viewedMerchantIds.Take(5)) // 限制查询数量
            {
                var merchantDishes = await _dishRepository.GetByMerchantIdAsync(merchantId);
                viewedMerchantDishes.AddRange(merchantDishes);
            }

            // 合并结果
            var allDishes = recommendedDishes.Union(viewedMerchantDishes).ToList();

            // 根据用户价格偏好进行筛选
            bool HasNonDefaultPricePreference(User user)
            {
                return user.Preference?.PricePreference != PricePreference.Any;
            }
            PricePreference GetPricePreference(User user)
            {
                return user.Preference?.PricePreference ?? PricePreference.Any;
            }
            // 使用方式
            if (HasNonDefaultPricePreference(user))
            {
                allDishes = FilterDishesByPricePreference(allDishes, GetPricePreference(user));
            }

            // 按评分和销量排序
            return allDishes
                .OrderByDescending(d => d.Rating)
                .ThenByDescending(d => d.MonthlySales)
                .Take(limit)
                .ToList();
        }

        /// <summary>
        /// 获取个性化的附近商家列表
        /// </summary>
        public async Task<List<Merchant>> GetPersonalizedNearbyMerchantsAsync(string userId, GeoLocation location, double radiusKm, int limit = 10)
        {
            // 获取附近商家
            var nearbyMerchants = await _merchantRepository.GetNearbyMerchantsAsync(location, radiusKm);

            if (string.IsNullOrEmpty(userId))
                return nearbyMerchants.Take(limit).ToList();

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return nearbyMerchants.Take(limit).ToList();

            var history = await _historyRepository.GetByUserIdAsync(userId);

            // 获取用户最近浏览的商家ID列表
            var recentViewedIds = await _historyRepository.GetRecentViewedMerchantIdsAsync(userId);

            // 结合用户偏好对附近商家进行个性化排序
            var scoredMerchants = new List<(Merchant merchant, double score)>();

            foreach (var merchant in nearbyMerchants)
            {
                double score = CalculateMerchantScore(merchant, user, history, recentViewedIds);
                scoredMerchants.Add((merchant, score));
            }

            return scoredMerchants
                .OrderByDescending(m => m.score)
                .Select(m => m.merchant)
                .Take(limit)
                .ToList();
        }

        /// <summary>
        /// 计算商家推荐分数
        /// </summary>
        private double CalculateMerchantScore(Merchant merchant, User user, List<UserHistory> history, List<string> recentViewedIds)
        {
            double score = 0;

            // 基础分数：评分(0-5分)
            score += merchant.Rating;

            // 流行度分数：基于评价数量(0-2分)
            score += Math.Min(merchant.ReviewCount / 500.0, 2.0);

            // 喜好类别加分(0-3分)
            if (user.Preference?.FavoriteCuisines != null)
            {
                foreach (var category in merchant.Categories)
                {
                    if (user.Preference.FavoriteCuisines.Contains(category))
                    {
                        score += 0.5;
                    }
                }
                score = Math.Min(score, 3.0); // 最多加3分
            }

            // 价格匹配度(0-2分)
            bool HasNonDefaultPricePreference(User user)
            {
                return user.Preference?.PricePreference != PricePreference.Any;
            }

            PricePreference GetPricePreference(User user)
            {
                return user.Preference?.PricePreference ?? PricePreference.Any;
            }
            if (HasNonDefaultPricePreference(user))
            {
                var priceMatch = IsMatchPricePreference(merchant.AveragePrice, GetPricePreference(user));
                if (priceMatch)
                {
                    score += 2.0;
                }
            }

            // 浏览历史相关性(0-1分)
            if (recentViewedIds.Contains(merchant.Id))
            {
                score += 1.0;
            }

            // 位置因素：如果在用户常去区域(0-1分)
            if (user.Preference?.FrequentRegions != null &&
                user.Preference.FrequentRegions.Contains(merchant.Region.ToString()))
            {
                score += 1.0;
            }

            // 最近更新加分(0-1分)，鼓励探索新商家
            var daysSinceUpdate = (DateTime.Now - merchant.LastUpdated).TotalDays;
            if (daysSinceUpdate < 7) // 一周内更新的
            {
                score += 1.0;
            }

            return score;
        }

        /// <summary>
        /// 根据价格偏好筛选菜品
        /// </summary>
        private List<Dish> FilterDishesByPricePreference(List<Dish> dishes, PricePreference preference)
        {
            return dishes.Where(d => {
                if (d.Prices == null || d.Prices.Count == 0)
                    return true;

                var avgPrice = d.Prices.Average(p => p.Price);

                return preference switch
                {
                    PricePreference.Budget => avgPrice <= 30,
                    PricePreference.Moderate => avgPrice > 30 && avgPrice <= 80,
                    PricePreference.Expensive => avgPrice > 80 && avgPrice <= 150,
                    PricePreference.VeryExpensive => avgPrice > 150,
                    _ => true
                };
            }).ToList();
        }

        /// <summary>
        /// 检查价格是否符合用户偏好
        /// </summary>
        private bool IsMatchPricePreference(decimal price, PricePreference preference)
        {
            return preference switch
            {
                PricePreference.Budget => price <= 30,
                PricePreference.Moderate => price > 30 && price <= 80,
                PricePreference.Expensive => price > 80 && price <= 150,
                PricePreference.VeryExpensive => price > 150,
                _ => true
            };
        }
    }
}
