using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodRecommendationSystem.Core.Models;

namespace FoodRecommendationSystem.Services.Interfaces
{
    /// <summary>
    /// 推荐服务接口
    /// </summary>
    public interface IRecommendationService
    {
        /// <summary>
        /// 获取推荐商家列表
        /// </summary>
        Task<List<Merchant>> GetRecommendedMerchantsAsync(string userId, int limit = 10);

        /// <summary>
        /// 获取推荐菜品列表
        /// </summary>
        Task<List<Dish>> GetRecommendedDishesAsync(string userId, int limit = 10);

        /// <summary>
        /// 获取个性化的附近商家列表
        /// </summary>
        Task<List<Merchant>> GetPersonalizedNearbyMerchantsAsync(string userId, GeoLocation location, double radiusKm, int limit = 10);
    }
}