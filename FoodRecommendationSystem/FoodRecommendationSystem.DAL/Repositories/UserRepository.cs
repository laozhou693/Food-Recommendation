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
    /// 用户数据访问仓储
    /// </summary>
    public class UserRepository
    {
        private readonly IMongoCollection<User> _collection;

        public UserRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<User>("users");

            // 创建用户名唯一索引
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<User>("{ Username: 1 }", indexOptions);
            _collection.Indexes.CreateOne(indexModel);
        }

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        public async Task<User> GetByIdAsync(string id)
        {
            return await _collection.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 根据用户名获取用户
        /// </summary>
        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _collection.Find(u => u.Username == username).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 根据邮箱获取用户
        /// </summary>
        public async Task<User> GetByEmailAsync(string email)
        {
            return await _collection.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 创建新用户
        /// </summary>
        public async Task CreateAsync(User user)
        {
            if (string.IsNullOrEmpty(user.Id))
            {
                user.Id = ObjectId.GenerateNewId().ToString();
            }
            await _collection.InsertOneAsync(user);
        }

        /// <summary>
        /// 更新用户
        /// </summary>
        public async Task UpdateAsync(string id, User user)
        {
            await _collection.ReplaceOneAsync(u => u.Id == id, user);
        }

        /// <summary>
        /// 更新用户偏好
        /// </summary>
        public async Task UpdatePreferenceAsync(string userId, UserPreference preference)
        {
            var update = Builders<User>.Update.Set(u => u.Preference, preference);
            await _collection.UpdateOneAsync(u => u.Id == userId, update);
        }

        /// <summary>
        /// 添加商家或菜品到收藏
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="itemId">商家或菜品ID</param>
        /// <param name="isMerchant">是否为商家(true为商家，false为菜品)</param>
        public async Task AddToFavoritesAsync(string userId, string itemId, bool isMerchant)
        {
            var update = isMerchant
                ? Builders<User>.Update.AddToSet(u => u.FavoriteMerchants, itemId)
                : Builders<User>.Update.AddToSet(u => u.FavoriteDishes, itemId);

            await _collection.UpdateOneAsync(u => u.Id == userId, update);
        }

        /// <summary>
        /// 从收藏中移除商家或菜品
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="itemId">商家或菜品ID</param>
        /// <param name="isMerchant">是否为商家(true为商家，false为菜品)</param>
        public async Task RemoveFromFavoritesAsync(string userId, string itemId, bool isMerchant)
        {
            var update = isMerchant
                ? Builders<User>.Update.Pull(u => u.FavoriteMerchants, itemId)
                : Builders<User>.Update.Pull(u => u.FavoriteDishes, itemId);

            await _collection.UpdateOneAsync(u => u.Id == userId, update);
        }

        /// <summary>
        /// 更新最后登录时间
        /// </summary>
        public async Task UpdateLastLoginAsync(string userId)
        {
            var update = Builders<User>.Update.Set(u => u.LastLoginDate, DateTime.Now);
            await _collection.UpdateOneAsync(u => u.Id == userId, update);
        }

        /// <summary>
        /// 添加搜索关键词到历史记录
        /// </summary>
        public async Task AddSearchKeywordAsync(string userId, string keyword)
        {
            // 获取用户
            var user = await GetByIdAsync(userId);
            if (user?.Preference == null)
                return;

            // 确保搜索历史不为null
            if (user.Preference.RecentSearches == null)
            {
                user.Preference.RecentSearches = new List<string>();
            }

            // 如果关键词已存在则先移除
            user.Preference.RecentSearches.Remove(keyword);

            // 添加到列表前端
            user.Preference.RecentSearches.Insert(0, keyword);

            // 保持列表最大数量为10
            if (user.Preference.RecentSearches.Count > 10)
            {
                user.Preference.RecentSearches.RemoveAt(10);
            }

            // 更新用户偏好
            await UpdatePreferenceAsync(userId, user.Preference);
        }
    }
}