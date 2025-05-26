
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using FoodRecommendationSystem.Core.Models;
using FoodRecommendationSystem.DAL.Repositories;
using FoodRecommendationSystem.Services.Interfaces;


namespace FoodRecommendationSystem.Services.Implementations
{
    /// <summary>
    /// 商家服务实现类
    /// </summary>
    public class MerchantService : IMerchantService
    {
        private readonly MerchantRepository _merchantRepository;
        private readonly DishRepository _dishRepository;
        private readonly ReviewRepository _reviewRepository;

        public MerchantService(
            MerchantRepository merchantRepository,
            DishRepository dishRepository,
            ReviewRepository reviewRepository)
        {
            _merchantRepository = merchantRepository;
            _dishRepository = dishRepository;
            _reviewRepository = reviewRepository;
        }

        /// <summary>
        /// 根据ID获取商家详情
        /// </summary>
        public async Task<MerchantDetailDto> GetByIdAsync(string id)
        {
            var merchant = await _merchantRepository.GetByIdAsync(id);

            if (merchant == null)
                throw new KeyNotFoundException($"未找到ID为{id}的商家");


            // 获取商家的菜品信息
            var dishes = await _dishRepository.GetByMerchantIdAsync(id);

            // 获取商家的评价信息
            var reviews = await _reviewRepository.GetByMerchantIdAsync(id);

            // 获取评价统计信息
            var reviewStats = await _reviewRepository.GetMerchantReviewStatsAsync(id);

            // 创建返回的DTO对象
            var result = new MerchantDetailDto
            {
                Merchant = merchant,
                Dishes = dishes,
                Reviews = reviews,
                ReviewStats = reviewStats
            };

            return result;
        }

        /// <summary>
        /// 搜索商家
        /// </summary>
        public async Task<List<Merchant>> SearchAsync(string keyword, List<string> categories, double? minRating, Region? region, decimal? maxPrice, int skip = 0, int limit = 20)
        {
            return await _merchantRepository.SearchAsync(keyword, categories, minRating, region, maxPrice, skip, limit);
        }

        /// <summary>
        /// 获取附近商家
        /// </summary>
        public async Task<List<Merchant>> GetNearbyAsync(GeoLocation location, double radiusKm, int limit = 20)
        {
            return await _merchantRepository.GetNearbyMerchantsAsync(location, radiusKm, limit);
        }

        /// <summary>
        /// 获取热门商家
        /// </summary>
        public async Task<List<Merchant>> GetHotMerchantsAsync(int limit = 10)
        {
            return await _merchantRepository.GetHotMerchantsAsync(limit);
        }

        /// <summary>
        /// 获取评分最高的商家
        /// </summary>
        public async Task<List<Merchant>> GetTopRatedMerchantsAsync(int limit = 10)
        {
            return await _merchantRepository.GetTopRatedMerchantsAsync(10, limit);
        }

        /// <summary>
        /// 获取最新更新的商家
        /// </summary>
        public async Task<List<Merchant>> GetRecentlyUpdatedMerchantsAsync(int limit = 10)
        {
            return await _merchantRepository.GetRecentlyUpdatedMerchantsAsync(limit);
        }
    }

    /// <summary>
    /// 商家详情DTO
    /// </summary>
    public class MerchantDetailDto
    {
        public required Merchant Merchant { get; set; }
        public required List<Dish> Dishes { get; set; }
        public required List<Review> Reviews { get; set; }
        public required ReviewStats ReviewStats { get; set; }
    }
}
