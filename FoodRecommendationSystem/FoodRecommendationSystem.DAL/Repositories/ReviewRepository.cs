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
    /// 评价数据访问仓储
    /// </summary>
    public class ReviewRepository
    {
        private readonly IMongoCollection<Review> _collection;

        public ReviewRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<Review>("reviews");

            // 创建商家ID索引
            var merchantIndexModel = new CreateIndexModel<Review>("{ MerchantId: 1 }");
            _collection.Indexes.CreateOne(merchantIndexModel);

            // 创建菜品ID索引
            var dishIndexModel = new CreateIndexModel<Review>("{ DishId: 1 }");
            _collection.Indexes.CreateOne(dishIndexModel);

            // 创建平台评价ID索引(确保不重复抓取)
            var platformIndexModel = new CreateIndexModel<Review>(
                "{ PlatformName: 1, PlatformReviewId: 1 }",
                new CreateIndexOptions { Unique = true });
            _collection.Indexes.CreateOne(platformIndexModel);
        }

        /// <summary>
        /// 根据商家ID获取评价
        /// </summary>
        public async Task<List<Review>> GetByMerchantIdAsync(string merchantId, int skip = 0, int limit = 20)
        {
            return await _collection.Find(r => r.MerchantId == merchantId)
                .SortByDescending(r => r.ReviewDate)
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();
        }

        /// <summary>
        /// 根据菜品ID获取评价
        /// </summary>
        public async Task<List<Review>> GetByDishIdAsync(string dishId, int skip = 0, int limit = 20)
        {
            return await _collection.Find(r => r.DishId == dishId)
                .SortByDescending(r => r.ReviewDate)
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();
        }

        /// <summary>
        /// 创建新评价
        /// </summary>
        public async Task CreateAsync(Review review)
        {
            if (string.IsNullOrEmpty(review.Id))
            {
                review.Id = ObjectId.GenerateNewId().ToString();
            }
            await _collection.InsertOneAsync(review);
        }

        /// <summary>
        /// 批量创建评价
        /// </summary>
        public async Task CreateManyAsync(List<Review> reviews)
        {
            if (reviews == null || reviews.Count == 0)
                return;

            // 确保所有评价都有ID
            foreach (var review in reviews)
            {
                if (string.IsNullOrEmpty(review.Id))
                {
                    review.Id = ObjectId.GenerateNewId().ToString();
                }
            }

            try
            {
                // 批量插入(可能会有重复项抛出异常)
                await _collection.InsertManyAsync(reviews, new InsertManyOptions
                {
                    IsOrdered = false // 允许部分失败
                });
            }
            catch (MongoBulkWriteException)
            {
                // 忽略重复项的错误，通常是因为平台评价ID重复
            }
        }

        /// <summary>
        /// 判断平台评价是否已存在
        /// </summary>
        public async Task<bool> ExistsByPlatformReviewIdAsync(string platformName, string platformReviewId)
        {
            var count = await _collection.CountDocumentsAsync(
                r => r.PlatformName == platformName && r.PlatformReviewId == platformReviewId);
            return count > 0;
        }

        /// <summary>
        /// 获取商家评价统计信息
        /// </summary>
        public async Task<ReviewStats> GetMerchantReviewStatsAsync(string merchantId)
        {
            var pipeline = new List<BsonDocument>
            {
                new BsonDocument("$match", new BsonDocument("MerchantId", merchantId)),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "averageRating", new BsonDocument("$avg", "$Rating") },
                    { "totalReviews", new BsonDocument("$sum", 1) },
                    { "rating5", new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray { new BsonDocument("$eq", new BsonArray { "$Rating", 5 }), 1, 0 })) },
                    { "rating4", new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray { new BsonDocument("$eq", new BsonArray { "$Rating", 4 }), 1, 0 })) },
                    { "rating3", new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray { new BsonDocument("$eq", new BsonArray { "$Rating", 3 }), 1, 0 })) },
                    { "rating2", new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray { new BsonDocument("$eq", new BsonArray { "$Rating", 2 }), 1, 0 })) },
                    { "rating1", new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray { new BsonDocument("$eq", new BsonArray { "$Rating", 1 }), 1, 0 })) }
                })
            };

            var result = await _collection.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();

            if (result == null)
            {
                return new ReviewStats { MerchantId = merchantId };
            }

            return new ReviewStats
            {
                MerchantId = merchantId,
                AverageRating = result["averageRating"].AsDouble,
                TotalReviews = result["totalReviews"].AsInt32,
                Rating5Count = result["rating5"].AsInt32,
                Rating4Count = result["rating4"].AsInt32,
                Rating3Count = result["rating3"].AsInt32,
                Rating2Count = result["rating2"].AsInt32,
                Rating1Count = result["rating1"].AsInt32
            };
        }
    }

    /// <summary>
    /// 评价统计信息类
    /// </summary>
    public class ReviewStats
    {
        public string MerchantId { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int Rating5Count { get; set; }
        public int Rating4Count { get; set; }
        public int Rating3Count { get; set; }
        public int Rating2Count { get; set; }
        public int Rating1Count { get; set; }
    }
}