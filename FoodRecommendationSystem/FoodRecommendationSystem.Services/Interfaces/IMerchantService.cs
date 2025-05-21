using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoodRecommendationSystem.Core.Models;
using FoodRecommendationSystem.Services.Implementations;

namespace FoodRecommendationSystem.Services.Interfaces
{
    /// <summary>
    /// 商家服务接口
    /// </summary>
    public interface IMerchantService
    {
        /// <summary>
        /// 根据ID获取商家详情
        /// </summary>
        Task<MerchantDetailDto> GetByIdAsync(string id);

        /// <summary>
        /// 搜索商家
        /// </summary>
        Task<List<Merchant>> SearchAsync(string keyword, List<string> categories, double? minRating, Region? region, decimal? maxPrice, int skip = 0, int limit = 20);

        /// <summary>
        /// 获取附近商家
        /// </summary>
        Task<List<Merchant>> GetNearbyAsync(GeoLocation location, double radiusKm, int limit = 20);

        /// <summary>
        /// 获取热门商家
        /// </summary>
        Task<List<Merchant>> GetHotMerchantsAsync(int limit = 10);

        /// <summary>
        /// 获取评分最高的商家
        /// </summary>
        Task<List<Merchant>> GetTopRatedMerchantsAsync(int limit = 10);

        /// <summary>
        /// 获取最新更新的商家
        /// </summary>
        Task<List<Merchant>> GetRecentlyUpdatedMerchantsAsync(int limit = 10);
    }
}