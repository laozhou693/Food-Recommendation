using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodRecommendationSystem.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace FoodRecommendationSystem.DAL.Repositories
{
    /// <summary>
    /// 商家数据仓储
    /// </summary>
    public class MerchantRepository
    {
        private readonly IMongoCollection<Merchant> _collection;
        private readonly ILogger<MerchantRepository> _logger;

        public MerchantRepository(IMongoDatabase database, ILogger<MerchantRepository> logger)
        {
            _collection = database.GetCollection<Merchant>("Merchants");
            _logger = logger;

            // 确保地理位置索引存在
            EnsureGeoIndexAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 根据ID获取商家
        /// </summary>
        public async Task<Merchant?> GetByIdAsync(string id)
        {
            try
            {
                return await _collection.Find(m => m.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取商家详情出错: {id}");
                return null;
            }
        }

        /// <summary>
        /// 根据平台ID获取商家
        /// </summary>
        public async Task<Merchant?> GetByPlatformIdAsync(string platformName, string platformId)
        {
            try
            {
                var filter = Builders<Merchant>.Filter.ElemMatch(
                    m => m.PlatformInfos,
                    p => p.PlatformName == platformName && p.PlatformMerchantId == platformId);

                return await _collection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"根据平台ID获取商家出错: {platformName} - {platformId}");
                return null;
            }
        }

        /// <summary>
        /// 创建商家
        /// </summary>
        public async Task<string> CreateAsync(Merchant merchant)
        {
            try
            {
                await _collection.InsertOneAsync(merchant);
                return merchant.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"创建商家出错: {merchant.Name}");
                throw;
            }
        }

        /// <summary>
        /// 更新商家
        /// </summary>
        public async Task UpdateAsync(string id, Merchant merchant)
        {
            try
            {
                await _collection.ReplaceOneAsync(m => m.Id == id, merchant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新商家出错: {id}");
                throw;
            }
        }

        /// <summary>
        /// 删除商家
        /// </summary>
        public async Task DeleteAsync(string id)
        {
            try
            {
                await _collection.DeleteOneAsync(m => m.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除商家出错: {id}");
                throw;
            }
        }

        /// <summary>
        /// 批量创建商家
        /// </summary>
        public async Task CreateManyAsync(List<Merchant> merchants)
        {
            try
            {
                if (merchants.Count > 0)
                {
                    await _collection.InsertManyAsync(merchants);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"批量创建商家出错: {merchants.Count}条数据");
                throw;
            }
        }

        /// <summary>
        /// 获取附近商家
        /// </summary>
        public async Task<List<Merchant>> GetNearbyMerchantsAsync(GeoLocation location, double radiusKm, int limit = 20)
        {
            try
            {
                // 使用聚合管道来计算附近的商家
                var pipeline = new BsonDocument[]
                {
                    new BsonDocument("$geoNear", new BsonDocument
                    {
                        { "near", new BsonDocument
                            {
                                { "type", "Point" },
                                { "coordinates", new BsonArray { location.Longitude, location.Latitude } }
                            }
                        },
                        { "distanceField", "distance" },
                        { "maxDistance", radiusKm * 1000 }, // 转换为米
                        { "spherical", true },
                        { "query", new BsonDocument() },  // 可以添加额外的查询条件
                        { "limit", limit }
                    })
                };

                var results = await _collection.Aggregate<Merchant>(pipeline).ToListAsync();
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取附近商家时出错: 位置({location.Latitude},{location.Longitude})，半径{radiusKm}公里");
                return new List<Merchant>();
            }
        }

        /// <summary>
        /// 搜索商家
        /// </summary>
        public async Task<List<Merchant>> SearchAsync(
            string? keyword = null,
            List<string>? categories = null,
            double? minRating = null,
            Region? region = null,
            decimal? maxPrice = null,
            int skip = 0,
            int limit = 20)
        {
            try
            {
                var filter = Builders<Merchant>.Filter.Empty;

                // 关键词搜索
                if (!string.IsNullOrEmpty(keyword))
                {
                    var keywordFilter = Builders<Merchant>.Filter.Regex(
                        m => m.Name, new BsonRegularExpression(keyword, "i"));

                    filter = Builders<Merchant>.Filter.And(filter, keywordFilter);
                }

                // 分类过滤
                if (categories != null && categories.Count > 0)
                {
                    var categoryFilter = Builders<Merchant>.Filter.AnyIn(m => m.Categories, categories);
                    filter = Builders<Merchant>.Filter.And(filter, categoryFilter);
                }

                // 评分过滤
                if (minRating.HasValue)
                {
                    var ratingFilter = Builders<Merchant>.Filter.Gte(m => m.Rating, minRating.Value);
                    filter = Builders<Merchant>.Filter.And(filter, ratingFilter);
                }

                // 区域过滤
                if (region.HasValue)
                {
                    var regionFilter = Builders<Merchant>.Filter.Eq(m => m.Region, region.Value);
                    filter = Builders<Merchant>.Filter.And(filter, regionFilter);
                }

                // 价格过滤
                if (maxPrice.HasValue)
                {
                    var priceFilter = Builders<Merchant>.Filter.Lte(m => m.AveragePrice, maxPrice.Value);
                    filter = Builders<Merchant>.Filter.And(filter, priceFilter);
                }

                // 执行查询
                return await _collection
                    .Find(filter)
                    .Skip(skip)
                    .Limit(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜索商家出错");
                return new List<Merchant>();
            }
        }

        /// <summary>
        /// 获取热门商家
        /// </summary>
        public async Task<List<Merchant>> GetHotMerchantsAsync(int limit = 10)
        {
            try
            {
                return await _collection
                    .Find(m => m.ReviewCount >= 50) // 评价数量大于50的商家
                    .SortByDescending(m => m.ReviewCount)
                    .Limit(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取热门商家出错");
                return new List<Merchant>();
            }
        }

        /// <summary>
        /// 获取评分最高的商家
        /// </summary>
        public async Task<List<Merchant>> GetTopRatedMerchantsAsync(int minReviews = 10, int limit = 10)
        {
            try
            {
                return await _collection
                    .Find(m => m.ReviewCount >= minReviews && m.Rating >= 4.0)
                    .SortByDescending(m => m.Rating)
                    .ThenByDescending(m => m.ReviewCount)
                    .Limit(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取评分最高商家出错");
                return new List<Merchant>();
            }
        }

        /// <summary>
        /// 获取最近更新的商家
        /// </summary>
        public async Task<List<Merchant>> GetRecentlyUpdatedMerchantsAsync(int limit = 10)
        {
            try
            {
                return await _collection
                    .Find(Builders<Merchant>.Filter.Empty)
                    .SortByDescending(m => m.LastUpdated)
                    .Limit(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取最近更新商家出错");
                return new List<Merchant>();
            }
        }

        /// <summary>
        /// 确保地理位置索引存在
        /// </summary>
        private async Task EnsureGeoIndexAsync()
        {
            try
            {
                var indexExists = false;
                using (var cursor = await _collection.Indexes.ListAsync())
                {
                    var indexes = await cursor.ToListAsync();
                    indexExists = indexes.Any(index =>
                        index["name"].AsString.Contains("Location"));
                }

                if (!indexExists)
                {
                    var keys = Builders<Merchant>.IndexKeys.Geo2DSphere("Location");
                    var options = new CreateIndexOptions { Name = "Location_2dsphere" };
                    var model = new CreateIndexModel<Merchant>(keys, options);

                    await _collection.Indexes.CreateOneAsync(model);
                    _logger.LogInformation("创建商家地理位置索引成功");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "确保地理位置索引存在时出错");
            }
        }

        /// <summary>
        /// 获取特定区域的商家
        /// </summary>
        public async Task<List<Merchant>> GetByRegionAsync(Region region, int limit = 20)
        {
            try
            {
                return await _collection
                    .Find(m => m.Region == region)
                    .Limit(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取区域商家出错: {region}");
                return new List<Merchant>();
            }
        }

        /// <summary>
        /// 获取特定分类的商家
        /// </summary>
        public async Task<List<Merchant>> GetByCategoryAsync(string category, int limit = 20)
        {
            try
            {
                var filter = Builders<Merchant>.Filter.AnyEq(m => m.Categories, category);

                return await _collection
                    .Find(filter)
                    .Limit(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取分类商家出错: {category}");
                return new List<Merchant>();
            }
        }

        /// <summary>
        /// 统计商家总数
        /// </summary>
        public async Task<long> CountAsync()
        {
            try
            {
                return await _collection.CountDocumentsAsync(Builders<Merchant>.Filter.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "统计商家总数出错");
                return 0;
            }
        }

        /// <summary>
        /// 获取商家推荐列表(使用加权算法)
        /// </summary>
        /// <summary>
        /// 获取商家推荐列表(使用加权算法)
        /// </summary>
        public async Task<List<Merchant>> GetRecommendedMerchantsAsync(int limit = 20)
        {
            try
            {
                // 创建一个基于评分、评价数量和更新时间的查询
                var now = DateTime.Now;
                var monthAgo = now.AddMonths(-1);

                // 使用C#类型安全的方式创建管道
                var filter = Builders<Merchant>.Filter.Gte(m => m.Rating, 4.0) &
                            Builders<Merchant>.Filter.Gte(m => m.ReviewCount, 10);

                // 先获取符合条件的商家
                var merchants = await _collection
                    .Find(filter)
                    .ToListAsync();

                // 计算推荐得分
                var scoredMerchants = merchants
                    .Select(m => new
                    {
                        Merchant = m,
                        Score = (m.Rating * 10) + (m.ReviewCount / 20.0) +
                                (m.LastUpdated >= monthAgo ? 5 : 0)
                    })
                    .OrderByDescending(x => x.Score)
                    .Take(limit)
                    .Select(x => x.Merchant)
                    .ToList();

                return scoredMerchants;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取推荐商家出错");
                return new List<Merchant>();
            }
        }
    }
}