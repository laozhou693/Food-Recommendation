using Food.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Food.Repository
{
    public class MerchantRepository
    {
        private readonly IMongoCollection<Merchant> _merchants;

        public MerchantRepository(IMongoDatabase database)
        {
            _merchants = database.GetCollection<Merchant>("merchants");
        }

        // 获取所有商家
        public async Task<List<Merchant>> GetAllAsync()
        {
            return await _merchants.Find(_ => true).ToListAsync();
        }

        // 获取单个商家
        public async Task<Merchant> GetByIdAsync(string id)
        {
            return await _merchants.Find(m => m.Id == id).FirstOrDefaultAsync();
        }

        // 获取多个商家
        public async Task<List<Merchant>> GetByIdsAsync(List<string> ids)
        {
            var filter = Builders<Merchant>.Filter.In(m => m.Id, ids);
            return await _merchants.Find(filter).ToListAsync();
        }

        // 按分类标签查询
        public async Task<List<Merchant>> GetByTagAsync(string tag)
        {
            var filter = Builders<Merchant>.Filter.AnyEq(m => m.Tags, tag);
            return await _merchants.Find(filter).ToListAsync();
        }

        // 关键词搜索
        public async Task<List<Merchant>> SearchAsync(string keyword)
        {
            var filter = Builders<Merchant>.Filter.Regex(m => m.Name, new BsonRegularExpression(keyword, "i"));
            return await _merchants.Find(filter).ToListAsync();
        }

        // 推荐商家 - 综合排序（评分 × 0.7 + 收藏数 × 0.3）
        public async Task<List<Merchant>> GetRecommendedAsync(int limit = 20)
        {
            return await _merchants.Find(_ => true)
                .SortByDescending(m => m.Rating * 0.7 + m.FavoriteCount * 0.3)
                .Limit(limit)
                .ToListAsync();
        }

        // 推荐商家 - 距离最近
        public async Task<List<Merchant>> GetNearestAsync(int limit = 20)
        {
            return await _merchants.Find(_ => true)
                .SortBy(m => m.Distance)
                .Limit(limit)
                .ToListAsync();
        }

        // 推荐商家 - 评分最高
        public async Task<List<Merchant>> GetHighestRatedAsync(int limit = 20)
        {
            return await _merchants.Find(_ => true)
                .SortByDescending(m => m.Rating)
                .Limit(limit)
                .ToListAsync();
        }

        // 增加收藏数
        public async Task IncrementFavoriteCountAsync(string merchantId)
        {
            var update = Builders<Merchant>.Update.Inc(m => m.FavoriteCount, 1);
            await _merchants.UpdateOneAsync(m => m.Id == merchantId, update);
        }

        // 减少收藏数
        public async Task DecrementFavoriteCountAsync(string merchantId)
        {
            var update = Builders<Merchant>.Update.Inc(m => m.FavoriteCount, -1);
            await _merchants.UpdateOneAsync(m => m.Id == merchantId, update);
        }
    }
}
