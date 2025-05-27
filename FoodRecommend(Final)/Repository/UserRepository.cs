using Food.Models;
using MongoDB.Driver;

namespace Food.Repository
{
    public class UserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("users");
        }

        // 创建用户
        public async Task<User> CreateAsync(User user)
        {
            await _users.InsertOneAsync(user);
            return user;
        }

        // 查找用户
        public async Task<User> GetByIdAsync(string id)
        {
            return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        // 通过用户名查找用户
        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
        }

        // 更新用户
        public async Task<bool> UpdateAsync(User user)
        {
            var result = await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
            return result.ModifiedCount > 0;
        }
        // 添加收藏
        public async Task<bool> AddFavoriteAsync(string userId, string merchantId)
        {
            var update = Builders<User>.Update.AddToSet(u => u.Favorites, merchantId);
            var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
            return result.ModifiedCount > 0;
        }

        // 移除收藏
        public async Task<bool> RemoveFavoriteAsync(string userId, string merchantId)
        {
            var update = Builders<User>.Update.Pull(u => u.Favorites, merchantId);
            var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
            return result.ModifiedCount > 0;
        }

        // 获取用户收藏
        public async Task<List<string>> GetFavoritesAsync(string userId)
        {
            var user = await GetByIdAsync(userId);
            return user?.Favorites ?? new List<string>();
        }

        // 更新登录时间
        public async Task UpdateLastLoginAsync(string userId)
        {
            var update = Builders<User>.Update.Set(u => u.LastLoginAt, DateTime.UtcNow);
            await _users.UpdateOneAsync(u => u.Id == userId, update);
        }
    }
}
