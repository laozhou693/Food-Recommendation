using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoodRecommendationSystem.Core.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace FoodRecommendationSystem.DAL.Repositories
{
    /// <summary>
    /// 用户历史记录数据访问仓储
    /// </summary>
    public class UserHistoryRepository
    {
        private readonly IMongoCollection<UserHistory> _collection;

        public UserHistoryRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<UserHistory>("userhistory");

            // 创建用户ID索引
            var userIndexModel = new CreateIndexModel<UserHistory>("{ UserId: 1 }");
            _collection.Indexes.CreateOne(userIndexModel);

            // 创建商家ID索引
            var merchantIndexModel = new CreateIndexModel<UserHistory>("{ MerchantId: 1 }");
            _collection.Indexes.CreateOne(merchantIndexModel);

            // 创建菜品ID索引
            var dishIndexModel = new CreateIndexModel<UserHistory>("{ DishId: 1 }");
            _collection.Indexes.CreateOne(dishIndexModel);

            // 创建时间戳索引（用于按时间查询和自动过期）
            var ttlIndexModel = new CreateIndexModel<UserHistory>(
                "{ Timestamp: 1 }",
                new CreateIndexOptions { ExpireAfter = new System.TimeSpan(90, 0, 0, 0) }); // 90天后自动过期
            _collection.Indexes.CreateOne(ttlIndexModel);
        }

        /// <summary>
        /// 根据用户ID获取历史记录
        /// </summary>
        public async Task<List<UserHistory>> GetByUserIdAsync(string userId, int limit = 50)
        {
            return await _collection.Find(h => h.UserId == userId)
                .SortByDescending(h => h.Timestamp)
                .Limit(limit)
                .ToListAsync();
        }

        /// <summary>
        /// 根据用户ID和历史类型获取历史记录
        /// </summary>
        public async Task<List<UserHistory>> GetByUserIdAndTypeAsync(string userId, HistoryType type, int limit = 20)
        {
            return await _collection.Find(h => h.UserId == userId && h.Type == type)
                .SortByDescending(h => h.Timestamp)
                .Limit(limit)
                .ToListAsync();
        }

        /// <summary>
        /// 创建历史记录
        /// </summary>
        public async Task CreateAsync(UserHistory history)
        {
            if (string.IsNullOrEmpty(history.Id))
            {
                history.Id = ObjectId.GenerateNewId().ToString();
            }
            await _collection.InsertOneAsync(history);
        }

        /// <summary>
        /// 删除历史记录
        /// </summary>
        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(h => h.Id == id);
        }

        /// <summary>
        /// 清空用户的所有历史记录
        /// </summary>
        public async Task ClearUserHistoryAsync(string userId)
        {
            await _collection.DeleteManyAsync(h => h.UserId == userId);
        }

        /// <summary>
        /// 获取用户最近浏览的商家
        /// </summary>
        public async Task<List<string>> GetRecentViewedMerchantIdsAsync(string userId, int limit = 10)
        {
            var histories = await _collection.Find(h => h.UserId == userId && h.Type == HistoryType.View && !string.IsNullOrEmpty(h.MerchantId))
                .SortByDescending(h => h.Timestamp)
                .Limit(limit * 2) // 获取更多，因为可能有重复
                .ToListAsync();

            // 去重
            var merchantIds = new HashSet<string>();
            var result = new List<string>();

            foreach (var history in histories)
            {
                if (!merchantIds.Contains(history.MerchantId))
                {
                    merchantIds.Add(history.MerchantId);
                    result.Add(history.MerchantId);

                    if (result.Count >= limit)
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// 获取用户最近浏览的菜品
        /// </summary>
        public async Task<List<string>> GetRecentViewedDishIdsAsync(string userId, int limit = 10)
        {
            var histories = await _collection.Find(h => h.UserId == userId && h.Type == HistoryType.View && !string.IsNullOrEmpty(h.DishId))
                .SortByDescending(h => h.Timestamp)
                .Limit(limit * 2) // 获取更多，因为可能有重复
                .ToListAsync();

            // 去重
            var dishIds = new HashSet<string>();
            var result = new List<string>();

            foreach (var history in histories)
            {
                if (!dishIds.Contains(history.DishId))
                {
                    dishIds.Add(history.DishId);
                    result.Add(history.DishId);

                    if (result.Count >= limit)
                        break;
                }
            }

            return result;
        }
    }
}