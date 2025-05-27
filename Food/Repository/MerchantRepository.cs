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
        public async Task<List<Merchant>> SearchAsync(string keyword, string tag = null)
        {
            var filter = Builders<Merchant>.Filter.Empty;

            // 如果提供了关键词，则添加关键词过滤条件
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                if (keyword != "空值")
                {
                    filter &= Builders<Merchant>.Filter.Regex(m => m.Name, new BsonRegularExpression(keyword, "i"));
                }
            }

            // 如果提供了标签，则添加标签过滤条件
            if (!string.IsNullOrWhiteSpace(tag))
            {
                if (tag != "全部分类")
                {
                     filter &= Builders<Merchant>.Filter.AnyEq(m => m.Tags,tag);
                }
            }

            // 执行查询
            return await _merchants.Find(filter).ToListAsync();
        }

        // 推荐商家 - 综合排序（评分 × 0.7 + 收藏数 × 0.3）
        public async Task<List<Merchant>> GetRecommendedAsync(int limit = 20)
        {
            var pipeline = new BsonDocument[]
            {
        // 添加计算字段：总评分 = RatingEle + RatingMeituan
        new BsonDocument("$addFields", new BsonDocument
        {
            { "TotalRating", new BsonDocument("$add", new BsonArray { "$RatingEle", "$RatingMeituan" }) }
        }),
        // 添加计算字段：推荐分数 = 总评分*0.7 + 收藏数*0.3
        new BsonDocument("$addFields", new BsonDocument
        {
            { "RecommendationScore",
                new BsonDocument("$add",
                    new BsonArray
                    {
                        new BsonDocument("$multiply", new BsonArray { "$TotalRating", 0.7 }),
                        new BsonDocument("$multiply", new BsonArray { "$FavoriteCount", 0.3 })
                    })
            }
        }),
        // 按推荐分数降序排序
        new BsonDocument("$sort", new BsonDocument("RecommendationScore", -1)),
        // 限制结果数量
        new BsonDocument("$limit", limit),
        // 移除临时添加的计算字段（可选）
        new BsonDocument("$project", new BsonDocument
        {
            { "TotalRating", 0 },
            { "RecommendationScore", 0 }
        })
            };

            return await _merchants.Aggregate<Merchant>(pipeline).ToListAsync();
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
            // 使用聚合管道计算总评分并排序
            var pipeline = new BsonDocument[]
            {
        // 添加计算字段 TotalRating 表示总评分
        new BsonDocument("$addFields", new BsonDocument
        {
            { "TotalRating", new BsonDocument("$add", new BsonArray { "$RatingEle", "$RatingMeituan" }) }
        }),
        // 按总评分降序排序
        new BsonDocument("$sort", new BsonDocument("TotalRating", -1)),
        // 限制结果数量
        new BsonDocument("$limit", limit),
        // 移除临时添加的 TotalRating 字段（可选）
        new BsonDocument("$project", new BsonDocument
        {
            { "TotalRating", 0 }
        })
            };

            return await _merchants.Aggregate<Merchant>(pipeline).ToListAsync();
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
